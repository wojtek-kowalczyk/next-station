using NS.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUIPauseMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GUIDialog dialog;
        [SerializeField] private GUILoadGameMenu loadGameMenu;
        [SerializeField] private GUISettingsMenu settingsMenu;

        private Button resumeButton;
        private Button newGameButton;
        private Button loadGameButton;
        private Button exitButton;
        private Button settingsButton;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            resumeButton = root.Q<Button>("ResumeButton");
            newGameButton = root.Q<Button>("NewGameButton");
            loadGameButton = root.Q<Button>("LoadGameButton");
            exitButton = root.Q<Button>("ExitButton");
            settingsButton = root.Q<Button>("SettingsButton");
        }

        private void OnEnable()
        {
            resumeButton.clicked += OnResumeButtonClicked;
            newGameButton.clicked += OnNewGameButtonClicked;
            loadGameButton.clicked += OnLoadGameButtonClicked;
            exitButton.clicked += OnExitButtonClicked;
            settingsButton.clicked += OnSettingsButtonClicked;

        }

        private void OnDisable()
        {
            resumeButton.clicked -= OnResumeButtonClicked;
            newGameButton.clicked -= OnNewGameButtonClicked;
            loadGameButton.clicked -= OnLoadGameButtonClicked;
            exitButton.clicked -= OnExitButtonClicked;
            settingsButton.clicked -= OnSettingsButtonClicked;
        }

        private void OnResumeButtonClicked()
        {
            Game.Instance.TryUnpause();
        }

        private void OnNewGameButtonClicked()
        {
            GUIUtils.HandleStartNewGame(dialog);
        }

        private void OnLoadGameButtonClicked()
        {
            loadGameMenu.SetActive(true);
        }

        private void OnExitButtonClicked()
        {
            GUIUtils.HandleSaveAndExit(dialog);
        }

        private void OnSettingsButtonClicked()
        {
            settingsMenu.SetActive(true, uiDocument.rootVisualElement);
            uiDocument.rootVisualElement.SetActive(false);
        }
    }
}
