using NS.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUITutorial : MonoBehaviour
    {
        [SerializeField] private UIDocument tutorialDialogDocument;
        [SerializeField] private UIDocument tutorialButtonDocument;

        private Button closeTutorialButton;
        private Button showTutorialButton;

        private void Awake()
        {
            closeTutorialButton = tutorialDialogDocument.rootVisualElement.Q<Button>("CloseButton");
            closeTutorialButton.clicked += OnCloseTutorialButtonClicked;

            showTutorialButton = tutorialButtonDocument.rootVisualElement.Q<Button>("OpenTutorialButton");
            showTutorialButton.clicked += OnShowTutorialButtonClicked;

            tutorialDialogDocument.rootVisualElement.SetActive(false);
        }

        private void OnDestroy()
        {
            closeTutorialButton.clicked -= OnCloseTutorialButtonClicked;
            showTutorialButton.clicked -= OnShowTutorialButtonClicked;
        }

        private void OnShowTutorialButtonClicked()
        {
            Game.Instance.ShowTutorial();
        }

        private void OnCloseTutorialButtonClicked()
        {
            Game.Instance.HideTutorial();
        }
    }
}
