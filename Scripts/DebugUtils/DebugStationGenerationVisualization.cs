using NS.Core.GameModes.Basic;
using NS.DebugUtils.Observers;
using UnityEngine;

namespace NS.DebugUtils
{
    public class DebugStationGenerationVisualization : MonoBehaviour
    {
        [SerializeField] private StationSpawner spawner;

        private StationGenerationObserver observer;
        private bool visualizationEnabled;

        private void OnEnable()
        {
            observer = new StationGenerationObserver(spawner);
        }

        private void OnDisable()
        {
            observer.Dispose();
            observer = null;
        }

        public void ToggleVisualizationOfLastGeneration()
        {
            visualizationEnabled = !visualizationEnabled;
        }

        private void Update()
        {
            if (!visualizationEnabled)
            {
                return;
            }

            foreach (Vector2Int cell in observer.LastGenerationCandidateCells)
            {
                // Note: +1 because if station was placed then it happened after this value was saved // TODO : UGLY
                bool isStale = observer.NumPlacedTilesAtTimeOfGeneration + 1 != Game.Instance.Grid.TileCount;
                Color color = isStale ? Color.orangeRed : Color.yellow;
                DebugDrawer.DrawHex(cell, color, Game.Instance.Grid.Math, radius: 0.5f);
            }
        }
    }
}
