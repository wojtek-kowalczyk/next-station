using System.Linq;
using NS.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.Visuals
{
    public class VisibleWhenGameState : MonoBehaviour
    {
        [SerializeField] private Game.State[] visibleStates;

        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string visualElementName;
        [SerializeField] private GameObject gameObjectToSetActive;
        [SerializeField] private Canvas canvas;

        private void OnEnable()
        {
            Game.Instance.StateChanged += OnGameStateChanged;
            Refresh(Game.Instance.CurrentState);
        }

        private void OnDisable()
        {
            Game.Instance.StateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(Game.State oldState, Game.State newState)
        {
            Refresh(newState);
        }

        private void Refresh(Game.State newState)
        {
            bool shouldBeVisible = visibleStates.Contains(newState);

            if (uiDocument != null)
            {
                VisualElement ve = string.IsNullOrEmpty(visualElementName)
                    ? uiDocument.rootVisualElement
                    : uiDocument.rootVisualElement.Q(visualElementName);
                ve.SetActive(shouldBeVisible);
            }

            if (gameObjectToSetActive != null)
            {
                gameObjectToSetActive.SetActive(shouldBeVisible);
            }

            if (canvas != null)
            {
                canvas.enabled = shouldBeVisible;
            }
        }
    }
}
