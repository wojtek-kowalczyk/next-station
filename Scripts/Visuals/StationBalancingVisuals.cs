using System.Collections.Generic;
using NS.Core;
using NS.UI;
using NS.Visuals.PathFollowing;
using UnityEngine;

namespace NS.Visuals
{
    public class StationBalancingVisuals : MonoBehaviour
    {
        [SerializeField] private ShockwaveManager shockwaveManager;
        [SerializeField] private StationDataTooltipManager tooltipManager;

        private readonly List<RailNetwork.BalancingResult.Pair> toVisualize = new();

        private void OnEnable()
        {
            Game.Instance.GameMode.RailNetworkBalanced += OnRailNetworkBalanced;
        }

        private void OnDisable()
        {
            Game.Instance.GameMode.RailNetworkBalanced -= OnRailNetworkBalanced;
        }

        private void OnRailNetworkBalanced(RailNetwork.BalancingResult balancingResult)
        {
            foreach (RailNetwork.BalancingResult.Pair balancedPair in balancingResult.pairs)
            {
                // defer to give visuals a chance to spawn (path needs to get splines from visuals)
                toVisualize.Add(balancedPair);
            }
        }

        private void Update()
        {
            foreach (RailNetwork.BalancingResult.Pair pair in toVisualize)
            {
                HandleBalancingVisuals(pair);
            }

            toVisualize.Clear();
        }

        private void HandleBalancingVisuals(RailNetwork.BalancingResult.Pair balancedPair)
        {
            HexGrid grid = Game.Instance.Grid;
            Vector2Int lastPlacementCell = Game.Instance.Model.LastPlacementCell;

            HandleStation(balancedPair.station1Cell);
            HandleStation(balancedPair.station2Cell);

            return;

            void HandleStation(Vector2Int stationCell)
            {
                if (lastPlacementCell == stationCell)
                {
                    if (Game.Instance.Model.Grid.TryGetStationData(stationCell, out StationData data))
                    {
                        tooltipManager.AnimateUpdate(stationCell, data);
                    }
                    return;
                }

                RailGraphBuilder.Path? path = RailGraphBuilder.FindPath(grid, lastPlacementCell, stationCell);
                if (path.HasValue == false)
                {
                    Debug.LogError($"[StationBalancingVisuals] no path from {lastPlacementCell} to {stationCell}");
                    return;
                }

                shockwaveManager.StartFollowing(grid.Math, path.Value, onArrive: () =>
                {
                    // take station data from the model, because the model might have changed during the journey
                    if (Game.Instance.Model.Grid.TryGetStationData(stationCell, out StationData data))
                    {
                        tooltipManager.AnimateUpdate(stationCell, data);
                    }
                });
            }
        }
    }
}
