using TMPro;
using DG.Tweening;
using NS.Utility;
using UnityEngine;

namespace NS.UI.HUD
{
    public class GUIStationPlacementModeIndicator : MonoBehaviour
    {
        private Tween tween1;
        private Tween tween2;
        private TMP_Text textComponent;
        private bool lastIsStation;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            Hide();
        }

        private void Update()
        {
            bool isStation = Game.Instance.TileStack.PeekTile()?.IsStation ?? false;
            if (lastIsStation != isStation)
            {
                lastIsStation = isStation;
                if (isStation)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        private void Show()
        {
            textComponent.alpha = 1;
            textComponent.transform.localScale = Vector3.one;

            tween1 = textComponent.DOAlpha(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            tween2 = textComponent.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }

        private void Hide()
        {
            tween1?.Kill();
            tween2?.Kill();
            tween1 = null;
            tween2 = null;

            textComponent.alpha = 0;
        }
    }
}
