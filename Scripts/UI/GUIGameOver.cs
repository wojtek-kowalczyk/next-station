using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUIGameOver : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GUIDialog dialog;
        [SerializeField] private GUILoadGameMenu loadGameMenu;

        private Button newGameButton;
        private Button loadGameButton;
        private Button exitButton;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            newGameButton = root.Q<Button>("NewGameButton");
            loadGameButton = root.Q<Button>("LoadGameButton");
            exitButton = root.Q<Button>("ExitButton");
        }

        private void OnEnable()
        {
            newGameButton.clicked += OnNewGameButtonClicked;
            loadGameButton.clicked += OnLoadGameButtonClicked;
            exitButton.clicked += OnExitButtonClicked;
        }

        private void OnDisable()
        {
            newGameButton.clicked -= OnNewGameButtonClicked;
            loadGameButton.clicked -= OnLoadGameButtonClicked;
            exitButton.clicked -= OnExitButtonClicked;
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
    }
}
