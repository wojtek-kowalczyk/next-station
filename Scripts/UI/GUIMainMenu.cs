using NS.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUIMainMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GUIDialog dialog;
        [SerializeField] private GUILoadGameMenu loadGameMenu;
        [SerializeField] private GUISettingsMenu settingsMenu;

        private Button continueButton;
        private Button newGameButton;
        private Button loadGameButton;
        private Button exitButton;
        private Button settingsButton;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            continueButton = root.Q<Button>("ContinueButton");
            newGameButton = root.Q<Button>("NewGameButton");
            loadGameButton = root.Q<Button>("LoadGameButton");
            exitButton = root.Q<Button>("ExitButton");
            settingsButton = root.Q<Button>("SettingsButton");

            var versionLabel = root.Q<Label>("VersionText");
            versionLabel.text = $"version {Application.version}";

            settingsMenu.SetActive(false, null);
        }

        private void OnEnable()
        {
            continueButton.clicked += OnContinueButtonClicked;
            newGameButton.clicked += OnNewGameButtonClicked;
            loadGameButton.clicked += OnLoadGameButtonClicked;
            exitButton.clicked += OnExitButtonClicked;
            settingsButton.clicked += OnSettingsButtonClicked;

            continueButton.SetEnabled(Game.Instance?.HasLastPlayedGameToContinue() ?? false);
        }

        private void OnDisable()
        {
            continueButton.clicked -= OnContinueButtonClicked;
            newGameButton.clicked -= OnNewGameButtonClicked;
            loadGameButton.clicked -= OnLoadGameButtonClicked;
            exitButton.clicked -= OnExitButtonClicked;
            settingsButton.clicked -= OnSettingsButtonClicked;
        }

        private void OnContinueButtonClicked()
        {
            // Note: just begin, because game would have already loaded last played save, if there was any
            Game.Instance.BeginGame();
        }

        private void OnNewGameButtonClicked()
        {
            Game.Instance.SetupAndBeginNewGame();
        }

        private void OnLoadGameButtonClicked()
        {
            loadGameMenu.SetActive(true);
        }

        private void OnExitButtonClicked()
        {
            dialog.Show("Are you sure you want to quit to desktop?",
                _ => Game.Instance.Quit()
            );
        }

        private void OnSettingsButtonClicked()
        {
            settingsMenu.SetActive(true, uiDocument.rootVisualElement);
            uiDocument.rootVisualElement.SetActive(false);
        }
    }
}
