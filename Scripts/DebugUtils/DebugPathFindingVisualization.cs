using NS.Core;
using UnityEngine;

namespace NS.DebugUtils
{
    public class DebugPathFindingVisualization : MonoBehaviour
    {
        private RailGraphBuilder.Path? pathToVisualize;

        public void VisualizePath(RailGraphBuilder.Path path)
        {
            pathToVisualize = path;
        }

        public void Clear()
        {
            pathToVisualize = null;
        }

        private void Update()
        {
            if (pathToVisualize.HasValue == false)
            {
                return;
            }

            foreach (Vector2Int cell in pathToVisualize.Value.cells)
            {
                DebugDrawer.DrawHex(cell, Color.white, Game.Instance.Grid.Math, 0, 0.5f);
            }
        }
    }
}
