using System;
using NS.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI
{
    public class GUIDialog : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        public static GUIDialog CurrentlyVisibleDialog { get; private set; }

        private Button noButton;
        private Button yesButton;
        private Button cancelButton;
        private Label infoText;
        private TextField inputField;

        private Action onNoAction;
        private Action onCancelAction;
        private Action<string> onYesAction;

        private void Awake()
        {
            noButton = uiDocument.rootVisualElement.Q<Button>("NoButton");
            yesButton = uiDocument.rootVisualElement.Q<Button>("YesButton");
            cancelButton = uiDocument.rootVisualElement.Q<Button>("CancelButton");
            infoText = uiDocument.rootVisualElement.Q<Label>("InfoText");
            inputField = uiDocument.rootVisualElement.Q<TextField>("InputField");

            yesButton.clicked += OnYesButtonClicked;
            noButton.clicked += OnNoButtonClicked;
            cancelButton.clicked += OnCancelButtonClicked;

            SetActive(false);
        }

        private void OnDestroy()
        {
            yesButton.clicked -= OnYesButtonClicked;
            noButton.clicked -= OnNoButtonClicked;
            cancelButton.clicked -= OnCancelButtonClicked;
        }

        public void Show(string promptText,
            Action<string> onYes,
            Action onNo = null,
            Action onCancel = null,
            string noText = "No",
            string yesText = "Yes",
            string cancelText = null,
            bool showInputField = false)
        {
            infoText.text = promptText;
            onYesAction = onYes;
            onNoAction = onNo;
            onCancelAction = onCancel;
            noButton.text = noText;
            yesButton.text = yesText;
            cancelButton.SetActive(cancelText != null);
            if (cancelText != null)
            {
                cancelButton.text = cancelText;
            }
            inputField.SetActive(showInputField);
            if (showInputField)
            {
                inputField.value = string.IsNullOrEmpty(Game.Instance.CurrentGameName) == false
                    ? Game.Instance.CurrentGameName
                    : DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
            }

            SetActive(true);
        }

        private void OnYesButtonClicked()
        {
            string inputText = inputField.value;
            onYesAction.Invoke(inputText);
            SetActive(false);
        }

        private void OnNoButtonClicked()
        {
            onNoAction?.Invoke();
            SetActive(false);
        }

        private void OnCancelButtonClicked()
        {
            onCancelAction?.Invoke();
            SetActive(false);
        }

        private void SetActive(bool active)
        {
            uiDocument.rootVisualElement.SetActive (active);
            CurrentlyVisibleDialog = active ? this : null;
        }

        public void VisualiseNeedsActionFirst()
        {
            // TODO : flash like windows dialog that you need to make a decision here before anything can happen
            Debug.Log("Complete the dialog first!");
        }
    }
}
