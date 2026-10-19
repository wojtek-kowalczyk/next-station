using DG.Tweening;
using NS.Utility;
using TMPro;
using UnityEngine;

namespace NS.Visuals
{
    public class FeedbackText : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform transformToAnchor;
        [SerializeField] private Transform transformToAnimate;
        [SerializeField] private TMP_Text text;

        [Header("Settings")]

        [SerializeField] private float scaleMult = 1.5f;
        [SerializeField] private float scaleTime = 0.35f;
        [SerializeField] private Ease scaleEase = Ease.OutQuad;

        [SerializeField] private Vector3 punchRotation;
        [SerializeField] private float punchRotationTime;

        [SerializeField] private float yMoveDelay = 1.0f;
        [SerializeField] private float yMoveDistance = 100.0f;
        [SerializeField] private float yMoveTime = 3.0f;
        [SerializeField] private Ease yMoveEase = Ease.OutQuad;

        [SerializeField] private float fadeDelay = 1.0f;
        [SerializeField] private float fadeTime = 3.0f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        private Tween tween;
        private float originalScale;
        private Vector3 worldAnchorPosition;
        private Camera cam;

        private void Awake()
        {
            cam = Camera.main;
            originalScale = transform.localScale.x;
        }

        public void SetText(string textValue)
        {
            text.text = textValue;
        }

        public void SetAnchor(Vector3 worldPos)
        {
            worldAnchorPosition = worldPos;
            UpdateAnchorTransform();
        }

        private void OnEnable()
        {
            tween.Kill();

            transformToAnimate.localScale = Vector3.one * originalScale * scaleMult;
            transformToAnimate.rotation = Quaternion.identity;
            transformToAnimate.localPosition = Vector3.zero;
            text.alpha = 1;

            var s = DOTween.Sequence();
            Transform t = transformToAnimate;
            s.Insert(0, t.DOPunchRotation(punchRotation, punchRotationTime));
            s.Insert(0, t.DOScale(originalScale, scaleTime).SetEase(scaleEase));
            s.Insert(yMoveDelay, t.DOLocalMoveY(yMoveDistance, yMoveTime).SetEase(yMoveEase));
            s.Insert(fadeDelay, text.DOAlpha(0, fadeTime).SetEase(fadeEase));
            tween = s;
        }

        private void Update()
        {
            UpdateAnchorTransform();
        }

        private void UpdateAnchorTransform()
        {
            if (cam == null)
            {
                transformToAnchor.transform.position = Vector2.zero;
                return;
            }

            transformToAnchor.transform.position = cam.WorldToScreenPoint(worldAnchorPosition);
        }
    }
}
