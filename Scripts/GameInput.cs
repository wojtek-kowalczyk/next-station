using NS.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NS
{
    public class GameInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference pauseToggleAction;

        private void OnEnable()
        {
            pauseToggleAction.action.performed += OnPauseToggleActionPerformed;
        }
        private void OnDisable()
        {
            pauseToggleAction.action.performed -= OnPauseToggleActionPerformed;
        }

        private void OnPauseToggleActionPerformed(InputAction.CallbackContext obj)
        {
            if (GUIDialog.CurrentlyVisibleDialog != null)
            {
                GUIDialog.CurrentlyVisibleDialog.VisualiseNeedsActionFirst();
                return;
            }

            if (Game.Instance.IsPaused)
            {
                Game.Instance.TryUnpause();
            }
            else
            {
                Game.Instance.TryPause();
            }
        }
    }
}
