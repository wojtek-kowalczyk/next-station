using System.Collections;
using UnityEngine;

namespace NS.Visuals.PathFollowing
{
    public class Shockwave : PathFollowingAgent
    {
        [SerializeField] private TrailRenderer trail;

        protected override void OnFollowingStarted()
        {
            base.OnFollowingStarted();

            trail.Clear();
        }

        protected override void OnFollowingEnded()
        {
            InvokeOnArrived();
            StartCoroutine(WaitForTrailAndComplete());
        }

        private IEnumerator WaitForTrailAndComplete()
        {
            var wait = new WaitForSeconds(trail.time);
            yield return wait;
            InvokeOnReadyToDie();
        }
    }
}
