using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;
using NS.Analytics;
using NS.Audio;
using NS.Config;
using NS.Core;
using NS.Core.GameModes.Basic;
using NS.Core.Model;
using NS.Utility;
using NS.Visuals;
using UnityEngine;
using UnityEngine.Assertions;
using Random = NS.Core.Random;

namespace NS
{
    [DefaultExecutionOrder(-100)]
    public class Game : MonoBehaviour
    {
        public enum State
        {
            Initial,
            MainMenu,
            Playing,
            Paused,
            GameOver,
            TutorialDialog,
        }

        public enum ModelChangeReason
        {
            Invalid,
            PlaceTile,
            Undo,
            Redo,
            LoadGame,
            NewGame,
        }

        private const int USE_RANDOM_SEED_VALUE = 0;

        [Tooltip("Set to 0 to have the seed randomized every run")]
        [SerializeField] private int gameSeed;
        [SerializeField] private GameConfig config;
        [SerializeField] private BasicGameMode gameMode;
        [SerializeField] private FeedbackTextController feedbackText;
        [SerializeField] private int numAllowedUndoActions = 1;

        [Header("Cheats")]
        [SerializeField, CanBeNull] private Cheats.DebugTileStack debugTileStack;

        public static Game Instance { get; private set; }

        /// Called when user placed a tile. Note the actual tile in the grid might be different from the tile inside
        /// the carried <see cref="TilePlacementData"/>, because game logic might have changed it (e.g., transformed
        /// into a station).
        public event Action<TilePlacementData> TilePlaced;

        public delegate void GameModelChangedHandler(GameModel oldModel, GameModel newModel, ModelChangeReason reason);
        public event GameModelChangedHandler ModelChanged;

        public delegate void GameStateChangedHandler(State oldState, State newState);
        public event GameStateChangedHandler StateChanged;

        public State CurrentState { get; private set; }
        public int CurrentTileRotation { get; private set; }
        public AnalyticsManager AnalyticsManager { get; private set; }
        public SaveManager SaveManager { get; private set; }
        public GameModel Model { get; private set; }
        public HexGrid Grid => Model.Grid;
        public ITileStack TileStack => debugTileStack?.enabled ?? false ? debugTileStack : Model.TileStack;
        public Cheats.DebugTileStack DebugTileStack => debugTileStack; // TODO : ugly, but can't downcast TileStack. How to do this?

        public GameConfig Config => config.IsInitialized? config : throw new InvalidOperationException("Config not initialized");
        public BasicGameMode GameMode => gameMode;
        public FeedbackTextController FeedbackText => feedbackText;
        public bool IsPaused => CurrentState == State.Paused;
        [CanBeNull] public string CurrentGameName => currentlyLoadedGameMetadata.saveFileNameExt?.Split(".")[0];
        public int StartingSeed => Model.StartingSeed;

        private bool isModelDirty; // TODO : should probably put on model itself?
        private GameValidator gameValidator;
        private readonly Stack<byte[]> undoStack = new();
        private readonly Stack<byte[]> redoStack = new();
        private GameSaveMetadata currentlyLoadedGameMetadata;

        private void Awake()
        {
            Assert.IsNotNull(config);
            Assert.IsNotNull(gameMode);
            Assert.IsNull(Instance);

            SerializationSettings.Set();

            config.Initialize();

            AnalyticsManager = new AnalyticsManager();
            SaveManager = new SaveManager();
            gameValidator = new GameValidator();

            CreateNewGameModel();

            ChangeState(State.MainMenu);

            Instance = this;
        }

        private void Start()
        {
            if (TryLoadLastPlayedGame() == false)
            {
                SetupNewGame();
            }
        }

        private void OnDestroy()
        {
            AnalyticsManager.StopRecording();
        }

        public bool BeginGame()
        {
            Assert.IsNotNull(Model);
            Assert.IsTrue(Model.Grid.TileCount > 0);

            ChangeState(State.Playing);

            return true;
        }

        public bool HasLastPlayedGameToContinue()
        {
            GameSaveMetadata? saveInfo = SaveManager.GetLastSaveMetadata();
            return saveInfo.HasValue;
        }

        private void CreateNewGameModel()
        {
            var oldModel = Model;

            Model = new GameModel();
            int seed = gameSeed == USE_RANDOM_SEED_VALUE ? UnityEngine.Random.Range(0, int.MaxValue) : gameSeed;
            Model.StartingSeed = seed;
            Model.TileStack = Config.DummyTileStackBlueprint.Generate(seed); // Random 1 - for tile stack
            Model.Rand = new Random(seed); // Random 2 - for other things... which means what??

            isModelDirty = false;

            ModelChanged?.Invoke(oldModel, Model, ModelChangeReason.NewGame);
        }

        public bool CanPlaceTile(TilePlacementData placementData)
        {
            return !Model.Grid.HasTile(placementData.cell) && gameMode.CanPlaceTile(Model, placementData);
        }

        public void PlaceTile(TilePlacementData placementData)
        {
            byte[] oldModel = MessagePackSerializer.Serialize(Model);

            SaveModelForUndo(oldModel, placementData);

            // game model updates
            {
                if (placementData.wasPlacedByPlayer)
                {
                    // pop tile before game mode updates, because they expect to read the next tile to place
                    // from the top of the tile stack
                    Model.TileStack.PopTile();
                }

                // execute after tile placement game logic which alters model a lot
                gameMode.UpdateGameModel(Model, placementData);
            }

            gameValidator?.ValidateSerialization(oldModel, Model, placementData);
            gameValidator?.ValidateUndo(this, placementData);

            isModelDirty = true;

            // TODO : make deep copies without serialization
            byte[] newModel = MessagePackSerializer.Serialize(Model);
            ModelChanged?.Invoke(
                MessagePackSerializer.Deserialize<GameModel>(oldModel),
                MessagePackSerializer.Deserialize<GameModel>(newModel),
                reason: ModelChangeReason.PlaceTile);

            TilePlaced?.Invoke(placementData);

            HandleGameOver();
        }

        private void HandleGameOver()
        {
            if (Model.TileStack.Count == 0)
            {
                ChangeState(State.GameOver);
            }
        }

        public void ReplaceTile(TilePlacementData placementData)
        {
            HexTileData rotatedTile = placementData.nonRotatedTile.Rotated(placementData.rotation);
            Grid.ReplaceTile(placementData.cell, rotatedTile);
        }

        public void OnCurrentTileRotated(int newTileRotation)
        {
            CurrentTileRotation = newTileRotation;
        }

        public bool TryUndoTilePlacement(bool silent = false)
        {
            if (redoStack.Count >= numAllowedUndoActions)
            {
                return false;
            }

            if (undoStack.TryPop(out byte[] serializedModel) == false)
            {
                return false;
            }

            redoStack.Push(MessagePackSerializer.Serialize(Model));

            var oldModel = Model;
            // Note: this invalidates references people hold to Grid and History
            Model = MessagePackSerializer.Deserialize<GameModel>(serializedModel);
            isModelDirty = true;

            if (silent == false)
            {
                ModelChanged?.Invoke(oldModel, Model, ModelChangeReason.Undo);
            }

            return true;
        }

        public bool TryRedoTilePlacement(bool silent = false)
        {
            if (redoStack.TryPop(out byte[] serializedModel) == false)
            {
                return false;
            }

            undoStack.Push(MessagePackSerializer.Serialize(Model));

            var oldModel = Model;
            // Note: this invalidates references people hold to Grid and History
            Model = MessagePackSerializer.Deserialize<GameModel>(serializedModel);
            isModelDirty = true;

            if (silent == false)
            {
                ModelChanged?.Invoke(oldModel, Model, ModelChangeReason.Redo);
            }

            return true;
        }

        private void SaveModelForUndo(byte[] model, TilePlacementData placementData)
        {
            if (placementData.wasPlacedByPlayer)
            {
                undoStack.Push(model);
            }
            else
            {
                undoStack.Clear();
            }

            redoStack.Clear();
        }

        public void SetupNewGame()
        {
            CreateNewGameModel();
            gameMode.PlaceStartingTiles(Model);

            isModelDirty = false; // starting tiles don't dirty the model

            currentlyLoadedGameMetadata = default;

            // TODO: fix hack (CameraController.ResetToDefault? .LookAtTile(0,0)?)
            // BEGIN HACK
            // when launching new game reset camera to the center of the board
            {
                Camera.main.transform.position = new Vector3(0, 9, -11);
                Camera.main.transform.rotation = Quaternion.Euler(40, 0, 0);
            }
            // END HACK
        }

        public void SetupAndBeginNewGame()
        {
            SetupNewGame();
            BeginGame();
        }

        private bool TryLoadLastPlayedGame()
        {
            GameSaveMetadata? saveMetadata = SaveManager.GetLastSaveMetadata();
            if (!saveMetadata.HasValue)
            {
                return false;
            }

            GameSave? save = SaveManager.LoadSaveFile(saveMetadata.Value);
            return save.HasValue && TryLoadGame(save.Value, saveMetadata.Value);
        }

        public bool TryLoadGame(GameSaveMetadata metadata)
        {
            GameSave? save = SaveManager.LoadSaveFile(metadata);
            Assert.IsTrue(save.HasValue);
            return TryLoadGame(save.Value, metadata);
        }

        private bool TryLoadGame(GameSave save, GameSaveMetadata metadata)
        {
            try
            {
                byte[] oldModel = MessagePackSerializer.Serialize(Model);
                Model = MessagePackSerializer.Deserialize<GameModel>(save.serializedModel);
                Debug.Log($"[Game] Save loaded:\n\n{Convert.ToBase64String(save.serializedModel)}");
                isModelDirty = false;
                ModelChanged?.Invoke(MessagePackSerializer.Deserialize<GameModel>(oldModel), Model, ModelChangeReason.LoadGame);
                currentlyLoadedGameMetadata = metadata;
                return true;
            }
            catch (MessagePackSerializationException)
            {
                Debug.LogError("[Game] Failed to deserialize model:\n\n" +
                               $"{Convert.ToBase64String(save.serializedModel)}");
                return false;
            }
        }

        private void ChangeState(State newState)
        {
            State oldState = CurrentState;
            EnsureValidTransition();
            Debug.Log($"[Game] Changing game state from {oldState} to {newState}");
            CurrentState = newState;
            OnStateChanged();
            StateChanged?.Invoke(oldState, newState);

            return;

            void OnStateChanged()
            {
                if (newState == State.Playing)
                {
                    Time.timeScale = 1;
                    AnalyticsManager.StartRecording();
                }
                else if (newState == State.Paused)
                {
                    Time.timeScale = 0;
                }
                else if (newState == State.GameOver)
                {
                    AnalyticsManager.StopRecording();
                    AnalyticsManager.Flush("game over");
                    AudioManager.Instance?.TryPlaySoundEffect(Sounds.GAME_OVER);
                }
            }

            void EnsureValidTransition()
            {
                (State from, State to)[] allowedTransitions =
                {
                    (State.Initial, State.MainMenu),
                    (State.MainMenu, State.Playing),
                    (State.Playing, State.Paused),
                    (State.Playing, State.GameOver),
                    (State.Paused, State.Playing),
                    (State.Paused, State.MainMenu),
                    (State.GameOver, State.Playing),
                    (State.GameOver, State.MainMenu),
                    (State.Playing, State.TutorialDialog),
                    (State.TutorialDialog, State.Playing)
                };

                bool isValidTransition = false;
                foreach ((State from, State to) in allowedTransitions)
                {
                    if (oldState == from && newState == to)
                    {
                        isValidTransition = true;
                        break;
                    }
                }

                if (!isValidTransition)
                {
                    throw new InvalidOperationException($"Game State transition from {oldState} to {newState} is not allowed");
                }
            }
        }

        public void TryPause()
        {
            if (CurrentState != State.Playing)
            {
                return;
            }

            ChangeState(State.Paused);
        }

        public void TryUnpause()
        {
            if (CurrentState != State.Paused)
            {
                return;
            }

            ChangeState(State.Playing);
        }

        public void ShowTutorial()
        {
            ChangeState(State.TutorialDialog);
        }

        public void HideTutorial()
        {
            ChangeState(State.Playing);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void MaybeSaveAndQuit(string saveName)
        {
            if (!string.IsNullOrEmpty(saveName))
            {
                SaveCurrentGame(saveName);
            }

            Application.Quit();
        }

        public void SaveCurrentGame(string saveName)
        {
            byte[] serializedModel = MessagePackSerializer.Serialize(Model);

            SaveManager sm = new();
            var gameSave = new GameSave(serializedModel);
            var gameInfo = new GameInfo(Model.Points, Model.TileStack.PeekTiles().Length);
            bool success = sm.TryWriteSaveFile(saveName, gameSave, gameInfo);
            if (!success)
            {
                Utils.ReportCriticalFailure("Failed to write a save file", false);
            }

            isModelDirty = false;
        }

        public bool RequiresSave()
        {
            return isModelDirty;
        }

        public void TryDeleteGame(GameSaveMetadata save)
        {
            SaveManager.TryDeleteSaveFile(save);
        }
    }
}
