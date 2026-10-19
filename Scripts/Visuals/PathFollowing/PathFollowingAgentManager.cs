using System;
using NS.Core;
using NS.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.Visuals.PathFollowing
{
    public class PathFollowingAgentManager<TAgent> : MonoBehaviour
        where TAgent : PathFollowingAgent
    {
        [SerializeField] private TAgent[] prefabs;
        [SerializeField] private Transform agentsParent;

        private ObjectPool<TAgent> pool;

        protected int AgentsAlive { get; private set; }

        public void Awake()
        {
            pool = new ObjectPool<TAgent>(
                createFunc: () => Instantiate(prefabs.RandomElement(), agentsParent),
                actionOnGet: agent => agent.gameObject.SetActive(true),
                actionOnRelease: agent => agent.gameObject.SetActive(false),
                actionOnDestroy: agent => Destroy(agent.gameObject)
            );
        }

        public void StartFollowing(HexGridMath gridMath, RailGraphBuilder.Path path, Action onArrive = null)
        {
            TAgent agent = pool.Get();
            bool success = agent.TryStartFollowing(gridMath, path, onArrive, onReadyToDie: () =>
            {
                pool.Release(agent);
                AgentsAlive--;
            });

            if (success)
            {
                AgentsAlive++;
            }
            else
            {
                Debug.LogWarning($"[{typeof(TAgent).Name}] Failed to start agent following for path: {path}");
                pool.Release(agent);
            }
        }
    }
}
