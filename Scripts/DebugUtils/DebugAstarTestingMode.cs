using NS.Core;
using NS.Visuals;
using NS.Visuals.PathFollowing;
using UnityEngine;

namespace NS.DebugUtils
{
    public class DebugAstarTestingMode
    {
        public bool IsEnabled { get; private set; }

        private readonly Vector2Int[] cells = new Vector2Int[2];
        private int lastSetIndex = -1;
        private DebugPathFindingVisualization visualization;
        private TrainManager trainManager;
        private ShockwaveManager shockwaveManager;

        public void Toggle()
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled)
            {
                visualization = Object.FindAnyObjectByType<DebugPathFindingVisualization>();
                trainManager = Object.FindAnyObjectByType<TrainManager>();
                shockwaveManager = Object.FindAnyObjectByType<ShockwaveManager>();

                Debug.Log("select first cell");
                lastSetIndex = -1;
            }
            else if (visualization != null)
            {
                visualization.Clear();
            }
        }

        private void Select(Vector2Int cell)
        {
            lastSetIndex = (lastSetIndex + 1) % cells.Length;
            cells[lastSetIndex] = cell;

            bool secondCellSet = lastSetIndex == 1;
            if (secondCellSet == false)
            {
                return;
            }

            RailGraphBuilder.Path? path = RailGraphBuilder.FindPath(Game.Instance.Grid, cells[0], cells[1]);
            if (path.HasValue)
            {
                visualization.VisualizePath(path.Value);
                trainManager.StartFollowing(Game.Instance.Grid.Math, path.Value);
                shockwaveManager.StartFollowing(Game.Instance.Grid.Math, path.Value);
            }
            else
            {
                visualization.Clear();
            }
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Select(GameplayHelpers.GetHoveredCell(Game.Instance.Grid, Camera.main));
                if (lastSetIndex == 0)
                {
                    Debug.Log("select second cell");
                }
                if (lastSetIndex == 1)
                {
                    Debug.Log("select first cell (new path)");
                }
            }
        }
    }
}
