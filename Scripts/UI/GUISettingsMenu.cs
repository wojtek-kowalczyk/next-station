using NS.Audio;
using NS.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUISettingsMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Toggle muteToggle;

        private Toggle stationPlacementToggle;
        private Toggle tileRotationToggle;

        private Button backButton;
        private VisualElement cameFrom;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;

            masterSlider = root.Q<Slider>("AudioMasterSlider");
            masterSlider.value = AudioManager.Instance.GetVolume01(AudioManager.Bus.Master);
            masterSlider.RegisterValueChangedCallback(x => AudioManager.Instance?.SetVolume01(AudioManager.Bus.Master, x.newValue));

            musicSlider = root.Q<Slider>("AudioMusicSlider");
            musicSlider.value = AudioManager.Instance.GetVolume01(AudioManager.Bus.Music);
            musicSlider.RegisterValueChangedCallback(x => AudioManager.Instance?.SetVolume01(AudioManager.Bus.Music, x.newValue));

            sfxSlider = root.Q<Slider>("AudioSfxSlider");
            sfxSlider.value = AudioManager.Instance.GetVolume01(AudioManager.Bus.Sfx);
            sfxSlider.RegisterValueChangedCallback(x => AudioManager.Instance?.SetVolume01(AudioManager.Bus.Sfx, x.newValue));

            muteToggle = root.Q<Toggle>("MuteToggle");
            muteToggle.value = AudioManager.Instance.IsMuted();
            muteToggle.RegisterValueChangedCallback(x => AudioManager.Instance?.SetIsMuted(x.newValue));

            stationPlacementToggle = root.Q<Toggle>("StationPlacementToggle");
            stationPlacementToggle.value =
                GameplaySettingsManager.Instance?.Get(GameplaySettingsManager.Setting.StationPlacementAllAroundIsland) ?? true;
            stationPlacementToggle.RegisterValueChangedCallback(x =>
                GameplaySettingsManager.Instance?.Set(GameplaySettingsManager.Setting.StationPlacementAllAroundIsland, x.newValue));

            tileRotationToggle = root.Q<Toggle>("TileRotationToggle");
            tileRotationToggle.value =
                GameplaySettingsManager.Instance?.Get(GameplaySettingsManager.Setting.ScrollToRotateTile) ?? true;
            tileRotationToggle.RegisterValueChangedCallback(x =>
                GameplaySettingsManager.Instance?.Set(GameplaySettingsManager.Setting.ScrollToRotateTile, x.newValue));

            backButton = root.Q<Button>("BackButton");
            backButton.clicked += OnBackButtonClicked;

            Game.Instance.StateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            Game.Instance.StateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(Game.State oldState, Game.State newState)
        {
            if (oldState == Game.State.Paused)
            {
                SetActive(false, null);
            }
        }

        public void SetActive(bool active, VisualElement elementToShowOnHide)
        {
            uiDocument.rootVisualElement.SetActive(active);
            cameFrom = elementToShowOnHide;
        }

        private void OnBackButtonClicked()
        {
            cameFrom?.SetActive(true);
            SetActive(false, null);
        }
    }
}
