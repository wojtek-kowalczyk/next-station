using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;

namespace NS.Utility
{
    public static class DOTweenExtensions
    {
        public static TweenerCore<float, float, FloatOptions> DOAlpha(this TMP_Text target, float endValue,
            float duration)
        {
            return DOTween.To(() => target.alpha, v => target.alpha = v, endValue, duration);
        }
    }
}
