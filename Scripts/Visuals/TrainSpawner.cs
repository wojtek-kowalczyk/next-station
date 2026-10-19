using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NS.Core;
using NS.Core.GameModes.Basic;
using NS.Utility;
using NS.Visuals.PathFollowing;
using UnityEngine;

namespace NS.Visuals
{
    public class TrainSpawner : MonoBehaviour
    {
        [SerializeField] private TrainManager trainManager;

        private IEnumerator Start()
        {
            var wait = new WaitForSeconds(3.0f);

            while (true)
            {
                yield return wait;

                try
                {
                    DoUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // ReSharper disable once IteratorNeverReturns
        }

        private void DoUpdate()
        {
            HexGrid grid = Game.Instance.Model.Grid;

            List<RailNetwork> allNetworks = BasicGameMode.FindRailNetworks(grid);
            List<RailNetwork> eligibleNetworks = allNetworks.Where(x => x.StationCount > 1).ToList();
            int numEligibleStations = eligibleNetworks.Sum(railNetwork => railNetwork.StationCount);

            if (numEligibleStations == 0)
            {
                return;
            }

            int maxTrains = numEligibleStations - 1;
            if (trainManager.TrainsAlive < maxTrains)
            {
                SpawnTrain(eligibleNetworks, grid);
            }
        }

        private void SpawnTrain(List<RailNetwork> networks, HexGrid grid)
        {
            var network = networks.RandomElement();
            if (network.StationCount < 2)
            {
                Debug.LogWarning("[TrainSpawner] Network has less than 2 stations. This should not happen here.");
                return;
            }

            var samplePool = new List<Vector2Int>(network.StationCells);
            samplePool.Shuffle();
            Vector2Int from = samplePool[0];
            Vector2Int to = samplePool[1];
            RailGraphBuilder.Path? path = RailGraphBuilder.FindPath(grid, from, to);

            if (path.HasValue == false)
            {
                Debug.LogWarning($"[TrainSpawner] Failed to get path between {from} and {to}. This should not happen here");
                return;
            }

            trainManager.StartFollowing(grid.Math, path.Value);
        }
    }
}
