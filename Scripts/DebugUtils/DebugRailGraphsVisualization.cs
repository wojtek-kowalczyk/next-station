using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NS.Core;
using NS.DebugUtils.Observers;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.DebugUtils
{
    public class DebugRailGraphsVisualization : MonoBehaviour
    {
        [SerializeField] private float timeInterval = 0.075f;
        [SerializeField] private float timeWaitAfter = 1.0f;

        private Coroutine visualizationCoroutine;
        private RailGraphBuildObserver searchObserver;

        private void OnEnable()
        {
            searchObserver = new RailGraphBuildObserver();
        }

        private void OnDisable()
        {
            searchObserver.Dispose();
            searchObserver = null;
        }

        public void TriggerVisualizationOfLastSearch()
        {
            var lastRailGraphs = searchObserver.LastRailGraphs;
            if (lastRailGraphs == null)
            {
                Debug.LogWarning("cannot visualize, no search was recorded yet");
                return;
            }

            RailGraphBuilder.RailGraph graph = lastRailGraphs.First();
            if (graph.visitedCellsInOrder == null)
            {
                Debug.LogWarning("last search has no debug data to visualize");
                return;
            }

            if (visualizationCoroutine != null)
            {
                StopCoroutine(visualizationCoroutine);
            }

            visualizationCoroutine = StartCoroutine(VisualizeBfsDebugData(Game.Instance.Grid, lastRailGraphs));
        }

        private IEnumerator VisualizeBfsDebugData(HexGrid grid, List<RailGraphBuilder.RailGraph> railGraphs)
        {
            float timeToDrawWhole = timeInterval * railGraphs.Sum(x => x.visitedCellsInOrder.Count);

            int drawnTiles = 0;
            for (int i = 0; i < railGraphs.Count; i++)
            {
                List<Vector2Int> visitedCells = railGraphs[i].visitedCellsInOrder;

                foreach (Vector2Int cell in visitedCells)
                {
                    grid.TryGetTile(cell, out HexTileData tile);
                    Assert.IsNotNull(tile);
                    float duration = timeToDrawWhole - drawnTiles * timeInterval + timeWaitAfter;
                    DebugDrawer.DrawTile(cell, Game.Instance.Grid.Math, tile, duration, DebugColors.Get(i));

                    drawnTiles++;

                    yield return new WaitForSeconds(timeInterval);
                }
            }
        }
    }
}
