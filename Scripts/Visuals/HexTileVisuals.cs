using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using NS.Core;
using NS.Visuals.PathFollowing;
using UnityEngine;
using UnityEngine.Assertions;
using Quaternion = UnityEngine.Quaternion;
using Random = NS.Core.Random;

namespace NS.Visuals
{
    public class HexTileVisuals : MonoBehaviour
    {
        [SerializeField] private Vector3 appearAnimationStartScale = Vector3.one * 0.9f;
        [SerializeField] private Vector3 appearAnimationMidScale = Vector3.one * 0.8f;
        [SerializeField] private Vector3 appearAnimationFinishScale = Vector3.one;
        [SerializeField] private float startToMidDuration = 0.12f;
        [SerializeField] private float midToFinishDuration = 0.18f;

        private HexTileData lastSetTileData;

        private readonly List<GameObject> spawnedObjects = new();
        private readonly List<ConnectedEdgesController> connectedEdgesControllers = new();
        private readonly List<SplineDescriptor> rotatedSplineDescriptors = new();

        private Tween appearAnimation;

        /// Pass in grid and cell only when tile is placed in a grid at this cell
        public void SetupForTile(HexTileData tileData, HexGrid grid = null, Vector2Int cell = default)
        {
            Assert.IsNotNull(tileData);

            if (Equals(lastSetTileData, tileData))
            {
                return;
            }

            lastSetTileData = tileData;

            ClearPreviousDecorationsIfAny();

            SpawnDecorations(tileData);
        }

        private void SpawnDecorations(HexTileData tile)
        {
            var rand = new Random(tile.decorationsSeed);
            var config = Game.Instance.Config.Decorations;

            // tile is rotated - in the grid the logical tiles are rotated - and that's correct.
            // Visuals need to be drawn for rotation zero to match preview tile, which is always drawn
            // from rotation zero, so we need to unrotate the tile, but then rotate the decorations.
            HexTileData unrotatedTile = tile.Rotated(-tile.placementRotation);

            if (tile.IsStation)
            {
                TileDecorationConfig.StationDecorationSelection selection = config.GetStationDecoration(unrotatedTile, rand);
                SpawnDecoration(selection.prefab, -selection.rotation + tile.placementRotation);
                if (selection.isGenericStation == false)
                {
                    return;
                }
            }

            SpawnRegularDecorations();
            SpawnRailDecorations();

            return;

            void SpawnRegularDecorations()
            {
                var decorations = config.GetDecorationPrefabs(unrotatedTile, rand);

                foreach ((GameObject prefab, int rotation) in decorations)
                {
                    if (prefab == null)
                    {
                        continue;
                    }

                    // Note: decorations are drawn for rotation zero, so we need to add tile's placement rotation
                    SpawnDecoration(prefab, rotation + tile.placementRotation);
                }
            }

            void SpawnRailDecorations()
            {
                // Tile might be arbitrarily rotated and keys in config are only
                // 1 of these 6 possible rotations, so try all of them.
                for (int rotation = 0; rotation < 6; rotation++)
                {
                    HexTileData rotatedTile = tile.Rotated(rotation);
                    GameObject railsPrefab = config.GetRailsPrefab(rotatedTile, rand);
                    if (railsPrefab == null)
                    {
                        continue;
                    }

                    // `-rotation` because the loop rotated the tile `rotation` steps forward to find decoration.
                    // Since decorations are setup for rotation 0, that means the tile must have been rotated
                    // `rotation` steps in other direction, so we spawn the decoration to match the tile's rotation.
                    // Note: `-rotation` is the same as `6 - rotation` when rotating tiles.
                    SpawnDecoration(railsPrefab, -rotation);

                    break;
                }
            }
        }

        private void SpawnDecoration(GameObject prefab, int rotation)
        {
            float baseRotation = Game.Instance.Config.Decorations.BaseRotation;
            GameObject go = Instantiate(prefab, transform);
            go.transform.localRotation = Quaternion.Euler(0, rotation * 60 + baseRotation, 0);
            spawnedObjects.Add(go);

            if (go.TryGetComponent(out ConnectedEdgesController connectedEdgesController))
            {
                connectedEdgesControllers.Add(connectedEdgesController);
            }

            var splineDescriptorComponents = go.GetComponentsInChildren<SplineDescriptorComponent>();
            if (splineDescriptorComponents.Length > 0)
            {
                Assert.IsTrue(splineDescriptorComponents.Length == 1);
                foreach (SplineDescriptor unrotated in splineDescriptorComponents[0].Descriptors)
                {
                    // Note: `edge - 1` because it's not an index but the edge number in assets
                    // This way it's easier to verify in the inspector, because assets use a convention
                    // for numbering edges that start at 1 (the reason eludes me, but it's too late to change that).
                    // + 6 because we want positive rotation.
                    SplineDescriptor rotated = new(
                        unrotated.spline,
                        (unrotated.fromEdge - 1 + rotation + 6) % 6,
                        (unrotated.toEdge - 1 + rotation + 6) % 6);
                    rotatedSplineDescriptors.Add(rotated);
                }
            }
        }

        /// Note that there might be many splines ending at one edge (e.g., TXTXTX tile has 2)
        public DirectedSpline? GetSplineEndingAtEdge(int edgeIndex)
        {
            SplineComputer spline = rotatedSplineDescriptors.Find(x => x.toEdge == edgeIndex).spline;
            if (spline != null)
                return new DirectedSpline(spline, Spline.Direction.Forward);

            spline = rotatedSplineDescriptors.Find(x => x.fromEdge == edgeIndex).spline;
            if (spline != null)
                return new DirectedSpline(spline, Spline.Direction.Backward);

            return null;
        }

        /// Note that there might be many splines beginning at one edge (e.g., TXTXTX tile has 2)
        public DirectedSpline? GetSplineBeginningAtEdge(int edgeIndex)
        {
            SplineComputer spline = rotatedSplineDescriptors.Find(x => x.fromEdge == edgeIndex).spline;
            if (spline != null)
                return new DirectedSpline(spline, Spline.Direction.Forward);

            spline = rotatedSplineDescriptors.Find(x => x.toEdge == edgeIndex).spline;
            if (spline != null)
                return new DirectedSpline(spline, Spline.Direction.Backward);

            return null;
        }

        public DirectedSpline? GetSplineBetweenEdges(int fromEdgeIndex, int toEdgeIndex)
        {
            var spline = rotatedSplineDescriptors.Find(
                x => x.fromEdge == fromEdgeIndex && x.toEdge == toEdgeIndex).spline;
            if (spline != null)
                return new DirectedSpline(spline, Spline.Direction.Forward);

            spline = rotatedSplineDescriptors.Find(
                x => x.toEdge == fromEdgeIndex && x.fromEdge == toEdgeIndex).spline;
            if (spline != null)
                return new DirectedSpline(spline, Spline.Direction.Backward);

            return null;
        }

        private void ClearPreviousDecorationsIfAny()
        {
            foreach (GameObject spawnedObject in spawnedObjects)
            {
                Destroy(spawnedObject);
            }

            spawnedObjects.Clear();
            connectedEdgesControllers.Clear();
        }

        public void SetDecorationsVisible(bool visible)
        {
            foreach (GameObject spawnedObject in spawnedObjects)
            {
                spawnedObject.SetActive(visible);
            }
        }

        public HexTileData GetTile()
        {
            return lastSetTileData;
        }

        /// <param name="grid">grid this tile visuals' data lives in</param>
        /// <param name="cell">cell this tile visuals' data is at </param>
        public void TryRefreshConnectedEdges(HexGrid grid, Vector2Int cell)
        {
            foreach (ConnectedEdgesController connectedEdgesController in connectedEdgesControllers)
            {
                connectedEdgesController.Refresh(grid, cell);
            }
        }

        public void PlayAppearAnimation()
        {
            appearAnimation.Kill();
            transform.localScale = appearAnimationStartScale;
            Sequence s = DOTween.Sequence();
            s.Append(transform.DOScale(appearAnimationMidScale, startToMidDuration));
            s.Append(transform.DOScale(appearAnimationFinishScale, midToFinishDuration));
            appearAnimation = s;
        }
    }
}
