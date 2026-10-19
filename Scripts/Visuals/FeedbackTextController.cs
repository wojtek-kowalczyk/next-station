using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.Visuals
{
    public class FeedbackTextController : MonoBehaviour
    {
        private class LiveEntry
        {
            public float timeLeft;
            public FeedbackText uiObject;
        }

        private class PendingEntry
        {
            public Vector3 worldAnchorPosition;
            public string text;
            public float timeLeft;
        }

        [SerializeField] private FeedbackText prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private float timeToLive = 3.0f;

        private readonly List<LiveEntry> liveEntries = new();
        private readonly List<PendingEntry> pendingEntries = new();
        private ObjectPool<FeedbackText> pool;

        private void Awake()
        {
            pool = new ObjectPool<FeedbackText>(
                createFunc: () => Instantiate(prefab, parent),
                actionOnGet: obj => obj.gameObject.SetActive(true),
                actionOnRelease: obj => obj.gameObject.SetActive(false),
                actionOnDestroy: Destroy);
        }

        public void PutText(string text, Vector3 worldPos, float delay = 0)
        {
            pendingEntries.Add(new PendingEntry()
            {
                worldAnchorPosition = worldPos,
                text = text,
                timeLeft = delay,
            });
        }

        private void Update()
        {
            ProcessLiveEntries();
            ProcessPendingEntries();
        }

        private void ProcessPendingEntries()
        {
            using var po = ListPool<PendingEntry>.Get(out var toRemove);

            foreach (PendingEntry entry in pendingEntries)
            {
                entry.timeLeft -= Time.deltaTime;
                if (entry.timeLeft > 0)
                {
                    continue;
                }

                FeedbackText uiObj = pool.Get();
                uiObj.SetText(entry.text);
                uiObj.SetAnchor(entry.worldAnchorPosition);

                liveEntries.Add(new LiveEntry()
                {
                    timeLeft = timeToLive,
                    uiObject = uiObj
                });

                toRemove.Add(entry);
            }

            foreach (PendingEntry entry in toRemove)
            {
                pendingEntries.Remove(entry);
            }
        }

        private void ProcessLiveEntries()
        {
            using var po = ListPool<LiveEntry>.Get(out var toDestroy);

            foreach (LiveEntry entry in liveEntries)
            {
                entry.timeLeft -= Time.deltaTime;
                if (entry.timeLeft <= 0)
                {
                    toDestroy.Add(entry);
                }
            }

            foreach (LiveEntry entry in toDestroy)
            {
                liveEntries.Remove(entry);
                pool.Release(entry.uiObject);
            }
        }
    }
}
