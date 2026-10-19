using System.Collections.Generic;
using System.Linq;
using NS.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUILoadGameMenu : MonoBehaviour
    {
        private const string GAME_BUTTON_SELECTED_CLASS = "game-button-selected";

        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GUIDialog dialog;

        private readonly List<GameSaveMetadata> saves = new();
        private readonly List<Button> buttons = new();

        private VisualElement scrollView;
        private Button loadButton;
        private Button deleteButton;
        private Button backButton;
        private int selectedIndex = -1;

        private void Awake()
        {
            scrollView = uiDocument.rootVisualElement.Q<VisualElement>("SavesScrollView");
            loadButton = uiDocument.rootVisualElement.Q<Button>("LoadGame");
            deleteButton = uiDocument.rootVisualElement.Q<Button>("DeleteGame");
            backButton = uiDocument.rootVisualElement.Q<Button>("BackButton");

            RefreshLoadAndDeleteButtons();
            SetActive(false);
        }

        private void OnEnable()
        {
            loadButton.clicked += OnLoadButtonClicked;
            deleteButton.clicked += OnDeleteButtonClicked;
            backButton.clicked += OnBackButtonClicked;
        }

        private void OnDisable()
        {
            loadButton.clicked -= OnLoadButtonClicked;
            deleteButton.clicked -= OnDeleteButtonClicked;
            backButton.clicked -= OnBackButtonClicked;
        }

        public void SetActive(bool active)
        {
            uiDocument.rootVisualElement.SetActive(active);

            if (active)
            {
                OnShow();
            }
            else
            {
                OnHide();
            }
        }

        private void OnShow()
        {
            Game.Instance.StateChanged += OnGameStateChanged;
            SetupButtons();
        }

        private void OnHide()
        {
            Game.Instance.StateChanged -= OnGameStateChanged;
            ClearButtons();
        }

        private void SetupButtons()
        {
            saves.AddRange(Game.Instance.SaveManager.GetSaves()
                .OrderByDescending(x => x.saveWriteTime));

            for (int i = 0; i < saves.Count; i++)
            {
                int idxCopy = i;
                var button = new Button(() => OnGameSaveButtonClicked(idxCopy));
                button.AddToClassList("game-button");
                string gameName = saves[i].saveFileNameExt.Split(".")[0]; // drop extension;
                button.text = gameName;
                if (Game.Instance.CurrentGameName == gameName)
                {
                    button.SetEnabled(false);
                }
                buttons.Add(button);

                scrollView.Add(button);
            }
        }

        private void ClearButtons()
        {
            scrollView.Clear();
            saves.Clear();
            buttons.Clear();
            selectedIndex = -1;
            RefreshLoadAndDeleteButtons();
        }

        private void OnGameStateChanged(Game.State oldState, Game.State newState)
        {
            if (newState == Game.State.Playing)
            {
                SetActive(false);
            }
        }

        private void OnGameSaveButtonClicked(int index)
        {
            if (selectedIndex != -1)
            {
                buttons[selectedIndex].RemoveFromClassList(GAME_BUTTON_SELECTED_CLASS);
            }
            selectedIndex = index;
            buttons[selectedIndex].AddToClassList(GAME_BUTTON_SELECTED_CLASS);
            RefreshLoadAndDeleteButtons();
        }

        private void OnLoadButtonClicked()
        {
            Assert.IsTrue(selectedIndex != -1);

            if (Game.Instance.RequiresSave())
            {
                dialog.Show("Do you want to save your current game?",
                    yesText: "Save current and load selected",
                    onYes: saveName =>
                    {
                        Game.Instance.SaveCurrentGame(saveName);
                        Game.Instance.TryLoadGame(saves[selectedIndex]);
                        Game.Instance.BeginGame();
                    },
                    noText: "Don't save current and load selected",
                    onNo: () =>
                    {
                        Game.Instance.TryLoadGame(saves[selectedIndex]);
                        Game.Instance.BeginGame();
                    },
                    cancelText: "Cancel",
                    showInputField: true
                );
            }
            else
            {
                Game.Instance.TryLoadGame(saves[selectedIndex]);
                Game.Instance.BeginGame();
            }
        }

        private void OnDeleteButtonClicked()
        {
            Assert.IsTrue(selectedIndex != -1);
            dialog.Show($"Are you sure you want to delete {saves[selectedIndex].saveFileNameExt}?",
                onYes: _ =>
                {
                    Game.Instance.TryDeleteGame(saves[selectedIndex]);
                    ClearButtons();
                    SetupButtons();
                });
        }

        private void OnBackButtonClicked()
        {
            SetActive(false);
        }

        private void RefreshLoadAndDeleteButtons()
        {
            loadButton.SetEnabled(selectedIndex != -1);
            deleteButton.SetEnabled(selectedIndex != -1);
        }
    }
}
