// #define PATH_FOLLOWER_LOGGING

using System;
using Dreamteck.Splines;
using JetBrains.Annotations;
using NS.Core;
using UnityEngine;

namespace NS.Visuals.PathFollowing
{
    // Actually more of a Path Tracer, but that has another, well known meaning.
    public class PathFollower : MonoBehaviour
    {
        [SerializeField] private float defaultSpeed = 0.35f;
        [SerializeField] private float defaultAcceleration;
        [SerializeField] private SplineTracer splineTracer;

        private TileVisualsController visualsController;

        private float speed;
        private RailGraphBuilder.Path currentPath;
        private int currentCellIndex;
        private Action onEndReachedCallback;
        private bool canMove = true;

        private void Awake()
        {
            visualsController = FindAnyObjectByType<TileVisualsController>();
            if (visualsController == null)
            {
                Debug.LogError("[PathFollower] Visuals controller not found in the scene");
            }
        }

        public bool TryStartFollowing(HexGridMath gridMath, RailGraphBuilder.Path path, Action onEndReached = null)
        {
            if (visualsController == null)
            {
                return false;
            }

            currentPath = path;
            currentCellIndex = 0;

            var directedSpline = GetSpline(gridMath, currentCellIndex);
            if (directedSpline == null)
            {
                return false;
            }

            speed = defaultSpeed;
            canMove = true;
            onEndReachedCallback = onEndReached;
            splineTracer.direction = directedSpline.Value.direction;

            splineTracer.spline = directedSpline.Value.spline;
            splineTracer.RebuildImmediate();
            splineTracer.SetPercent(0.5f); // start in the middle of the spline

            return true;
        }

        private void Update()
        {
            if (splineTracer.spline == null)
            {
                EndJourney();
                return;
            }

            if (canMove == false)
            {
                return;
            }

            speed += defaultAcceleration * Time.deltaTime;

            float distanceToMove = speed * Time.deltaTime;
            float moved = Move(distanceToMove);

            bool isLastSegment = currentCellIndex == currentPath.cells.Count - 1;

            float percentLeft = GetPercentLeft(isLastSegment);
            if (percentLeft <= 0)
            {
                OnSegmentEndReached(isLastSegment, distanceToMove, moved);
            }
        }

        private float GetPercentLeft(bool isLastSegment)
        {
            // last segment goes until half of the spline
            return splineTracer.direction == Spline.Direction.Forward
                ? (isLastSegment ? 0.5f : 1.0f) - (float)splineTracer.GetPercent()
                : (float)splineTracer.GetPercent() - (isLastSegment ? 0.5f : 0.0f);
        }

        private float Move(float distanceToMove)
        {
            if (distanceToMove <= 0)
            {
                return 0;
            }

            double newPercent = splineTracer.Travel(
                splineTracer.GetPercent(), distanceToMove, splineTracer.direction, out float moved);
            splineTracer.SetPercent(newPercent);

            return moved;
        }

        private void OnSegmentEndReached(bool isLastSegment, float distanceToMove, float moved)
        {
            if (isLastSegment)
            {
                EndJourney();
                return;
            }

            // advance index
            if (currentCellIndex < currentPath.cells.Count - 1)
            {
                currentCellIndex += 1;
            }

            // pick next spline
            var nextDirectedSpline = GetSpline(Game.Instance.Grid.Math, currentCellIndex);
            if (nextDirectedSpline == null)
            {
                Debug.LogError("[PathFollower] no next spline");
                EndJourney();
                return;
            }

            // jump to new spline
            splineTracer.direction = nextDirectedSpline.Value.direction;
            splineTracer.spline = nextDirectedSpline.Value.spline;
            splineTracer.RebuildImmediate();

            float overshoot = Mathf.Max(0, distanceToMove - moved);
            double percent = overshoot / splineTracer.spline.CalculateLength();

            splineTracer.SetPercent(splineTracer.direction == Spline.Direction.Backward ? 1 - percent : percent);
        }

        private void EndJourney()
        {
            onEndReachedCallback?.Invoke();
            onEndReachedCallback = null;
            canMove = false;
        }

        /// Returns the spline that the follower should be on taken when on cell with given index
        [CanBeNull]
        private DirectedSpline? GetSpline(HexGridMath gridMath, int cellIndex)
        {
            Vector2Int currentCell = currentPath.cells[cellIndex];

            Vector2Int? previousCell = null;
            Vector2Int? nextCell = null;

            if (cellIndex > 0)
                previousCell = currentPath.cells[cellIndex - 1];
            if (cellIndex < currentPath.cells.Count - 1)
                nextCell = currentPath.cells[cellIndex + 1];

            if (visualsController.TryGetTileVisuals(currentCell, out HexTileVisuals tileVisuals) == false)
            {
                Debug.LogError($"[PathFollower] Failed to get tile visuals for cell {currentCell}");
                return null;
            }

            // note: always passing currentCell first to EdgeBetweenCells,
            // because we're selecting a spline from current cell, so we need indices from its POV.
            DirectedSpline? spline;
            if (previousCell != null)
            {
                if (nextCell != null)
                {
                    int fromEdgeIndex = gridMath.EdgeBetweenCells(currentCell, previousCell.Value);
                    int toEdgeIndex = gridMath.EdgeBetweenCells(currentCell, nextCell.Value);
                    DebugLog($"spline from edge {fromEdgeIndex} to edge {toEdgeIndex}");
                    spline = tileVisuals.GetSplineBetweenEdges(fromEdgeIndex, toEdgeIndex);
                }
                else
                {
                    // if the spline ends at a TXTXTX it will look awkward, but that should never happen
                    int edgeIndex = gridMath.EdgeBetweenCells(currentCell, previousCell.Value);
                    DebugLog($"path ending, coming from edge {edgeIndex}");
                    spline = tileVisuals.GetSplineBeginningAtEdge(edgeIndex); // note: edge number starts at 1
                }
            }
            else
            {
                if (nextCell != null)
                {
                    int edgeIndex = gridMath.EdgeBetweenCells(currentCell, nextCell.Value);
                    DebugLog($"path starting, going to edge {edgeIndex}");
                    spline = tileVisuals.GetSplineEndingAtEdge(edgeIndex);
                }
                else
                {
                    Debug.LogError("[PathFollower] Failed to get next spline: path has only one cell");
                    spline = null;
                }
            }

            return spline;
        }

        // ReSharper disable once UnusedParameter.Local
        private static void DebugLog(string message)
        {
#if PATH_FOLLOWER_LOGGING
            Debug.Log("[PathFollower] " + message);
#endif
        }
    }
}
