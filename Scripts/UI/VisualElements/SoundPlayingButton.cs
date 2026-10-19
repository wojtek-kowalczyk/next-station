using NS.Audio;
using UnityEngine.UIElements;

namespace NS.UI.VisualElements
{
    [UxmlElement]
    public partial class ButtonWithSounds : Button
    {
        public ButtonWithSounds()
        {
            clicked += OnClicked;
            RegisterCallback<MouseOverEvent>(OnHover);
        }

        private void OnClicked()
        {
            AudioManager.Instance?.TryPlaySoundEffect(Sounds.BUTTON_CLICK);
        }

        private void OnHover(MouseOverEvent _)
        {
            AudioManager.Instance?.TryPlaySoundEffect(Sounds.BUTTON_HOVER);
        }
    }
}
