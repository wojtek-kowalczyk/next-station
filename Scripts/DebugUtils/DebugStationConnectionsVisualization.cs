using System.Collections.Generic;
using NS.Core;
using NS.DebugUtils.Observers;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.DebugUtils
{
    public class DebugStationConnectionsVisualization : MonoBehaviour
    {
        private RailGraphBuildObserver searchObserver;

        private void Awake()
        {
            searchObserver = new RailGraphBuildObserver();
        }

        private void OnDestroy()
        {
            searchObserver.Dispose();
            searchObserver = null;
        }

        private void Update()
        {
            HexGrid grid = Game.Instance?.Grid;

            if (grid == null)
            {
                return;
            }

            DrawStations(grid);
            DrawStationConnections(grid);
        }

        private static void DrawStations(HexGrid grid)
        {
            using var po = ListPool<Vector2Int>.Get(out var stationCells);
            grid.GetStationTiles(stationCells);
            foreach (Vector2Int cell in stationCells)
            {
                DebugDrawer.DrawHex(cell, Color.yellow, grid.Math);
            }
        }

        private void DrawStationConnections(HexGrid grid)
        {
            var lastRailGraphs = searchObserver.LastRailGraphs;
            if (lastRailGraphs == null)
            {
                return;
            }

            for (int networkIndex = 0; networkIndex < lastRailGraphs.Count; networkIndex++)
            {
                List<Vector2Int> stations = lastRailGraphs[networkIndex].connectedStations;
                for (int j = 0; j < stations.Count - 1; j++)
                {
                    Vector3 a = grid.Math.HexToWorld(stations[j]);
                    Vector3 b = grid.Math.HexToWorld(stations[j + 1]);
                    Debug.DrawLine(a, b, DebugColors.Get(networkIndex), 0, false);
                }
            }
        }
    }
}
