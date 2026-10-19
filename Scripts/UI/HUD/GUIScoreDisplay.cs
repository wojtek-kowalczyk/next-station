using NS.Core.Model;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

namespace NS.UI.HUD
{
    public class GUIScoreDisplay : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private float punchTweenDuration = 0.66f;
        [SerializeField] private float punchTweenScale = 2.0f;
        [SerializeField] private int vibrato = 10;
        [SerializeField] private float elasticity = 1.0f;

        private Label label;
        private Tween textPunchTween;
        private int lastSet;

        private void OnEnable()
        {
            Game.Instance.ModelChanged += OnGameModelChanged;

            label = document.rootVisualElement.Q<Label>("ScoreText");
            label.text = Game.Instance.Model.Points.ToString();
        }

        private void OnDisable()
        {
            Game.Instance.ModelChanged -= OnGameModelChanged;
        }

        private void OnGameModelChanged(GameModel oldModel, GameModel newModel, Game.ModelChangeReason reason)
        {
            if (lastSet == newModel.Points)
            {
                return;
            }

            label.text = newModel.Points.ToString();
            lastSet = newModel.Points;

            Vector3 from = Vector3.one;
            Vector3 to = Vector3.one * punchTweenScale;

            textPunchTween.Kill();
            label.style.scale = from;
            textPunchTween = DOTween.Punch(
                () => label.style.scale.value.value,
                value => label.style.scale = value,
                to, punchTweenDuration, vibrato, elasticity);
        }
    }
}
