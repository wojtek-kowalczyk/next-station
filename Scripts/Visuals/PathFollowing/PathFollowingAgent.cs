using System;
using NS.Core;
using UnityEngine;

namespace NS.Visuals.PathFollowing
{
    public class PathFollowingAgent : MonoBehaviour
    {
        [SerializeField] private PathFollower pathFollower;

        private Action onReadyToDieCallback;
        private Action onArriveCallback;

        public bool TryStartFollowing(HexGridMath gridMath, RailGraphBuilder.Path path, Action onArrive, Action onReadyToDie)
        {
            if (pathFollower.TryStartFollowing(gridMath, path, OnFollowingEnded) == false)
            {
                return false;
            }

            if (onReadyToDieCallback != null)
            {
                Debug.LogWarning("[Shockwave] New journey was started before first one completed.");
            }

            onReadyToDieCallback = onReadyToDie;
            onArriveCallback = onArrive;

            OnFollowingStarted();

            return true;
        }

        protected virtual void OnFollowingStarted()
        {
        }

        protected virtual void OnFollowingEnded()
        {
            InvokeOnArrived();
            InvokeOnReadyToDie();
        }

        protected void InvokeOnArrived()
        {
            onArriveCallback?.Invoke();
            onArriveCallback = null;
        }

        protected void InvokeOnReadyToDie()
        {
            onReadyToDieCallback?.Invoke();
            onReadyToDieCallback = null;
        }
    }
}
