using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace NS.Visuals.PathFollowing
{
    public class Train : PathFollowingAgent
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private Transform visualsTransform;
        [SerializeField] private Vector3 disappearScale = new(0.01f, 0.01f, 0.01f);
        [SerializeField] private float scaleTweenDuration = 1.0f;
        [SerializeField] private Ease scaleInEase = Ease.OutBack;
        [SerializeField] private Ease scaleOutEase = Ease.InBack;

        private Tween scaleInTween;
        private Tween scaleOutTween;
        private Vector3 defaultScale;

        private void Awake()
        {
            defaultScale = visualsTransform.localScale;
        }

        protected override void OnFollowingStarted()
        {
            scaleInTween.Kill();
            scaleOutTween.Kill();

            visualsTransform.localScale = disappearScale;
            scaleInTween = visualsTransform.DOScale(defaultScale, scaleTweenDuration).SetEase(scaleInEase);

            particles.Play();
        }

        protected override void OnFollowingEnded()
        {
            particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);

            InvokeOnArrived();

            scaleOutTween.Kill(complete: false);
            visualsTransform.localScale = defaultScale;
            scaleOutTween = visualsTransform
                .DOScale(disappearScale, scaleTweenDuration).SetEase(scaleOutEase)
                .OnComplete(() =>
                {
                    StartCoroutine(WaitForParticlesAndComplete());
                });
        }

        private IEnumerator WaitForParticlesAndComplete()
        {
            visualsTransform.gameObject.SetActive(false);
            yield return new WaitUntil(() => particles.particleCount == 0);
            visualsTransform.gameObject.SetActive(true);
            InvokeOnReadyToDie();
        }
    }
}
