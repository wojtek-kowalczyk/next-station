using System.Collections.Generic;
using System.Linq;
using NS.Core;
using NS.Core.Model;
using NS.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.Visuals
{
    public class TileVisualsController : MonoBehaviour
    {
        [SerializeField] private Transform tilesParent;
        [SerializeField] private HexTileVisuals visualsPrefab;
        [SerializeField] private GameObject ghostTilePrefab;

        private readonly Dictionary<Vector2Int, GameObject> spawnedGhostTiles = new();
        private readonly Dictionary<Vector2Int, HexTileVisuals> spawnedVisuals = new();
        private ObjectPool<GameObject> ghostTilePool;

        private void Awake()
        {
            ghostTilePool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(ghostTilePrefab, tilesParent),
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go => go.SetActive(false),
                actionOnDestroy: Destroy
            );
        }

        private void OnEnable()
        {
            Game.Instance.ModelChanged += OnGameModelChanged;
        }

        private void OnDisable()
        {
            Game.Instance.ModelChanged -= OnGameModelChanged;
        }

        public bool TryGetTileVisuals(Vector2Int cell, out HexTileVisuals visuals)
        {
            return spawnedVisuals.TryGetValue(cell, out visuals);
        }

        private void OnGameModelChanged(GameModel oldModel, GameModel newModel, Game.ModelChangeReason reason)
        {
            HandleTileVisuals(newModel, reason);
            HandleGhostTiles(newModel);
        }

        private void HandleTileVisuals(GameModel newModel, Game.ModelChangeReason reason)
        {
            var grid = newModel.Grid;

            foreach ((Vector2Int cell, HexTileData tile) in grid.tiles)
            {
                if (spawnedVisuals.ContainsKey(cell))
                {
                    TryReplaceTileVisuals(cell, tile, grid);
                }
                else
                {
                    bool animate = newModel.LastPlacementCell == cell &&
                                   (reason == Game.ModelChangeReason.PlaceTile || reason == Game.ModelChangeReason.Redo);
                    CreateTileVisuals(cell, tile, grid, animate);
                }

                spawnedVisuals[cell].TryRefreshConnectedEdges(grid, cell);
            }

            using var po = ListPool<Vector2Int>.Get(out var toDestroy);
            foreach ((Vector2Int cell, HexTileVisuals _) in spawnedVisuals)
            {
                if (grid.HasTile(cell) == false)
                {
                    toDestroy.Add(cell);
                }
            }

            foreach (Vector2Int cell in toDestroy)
            {
                DestroyTileVisuals(cell);
            }
        }

        private void CreateTileVisuals(Vector2Int cell, HexTileData tileData, HexGrid grid, bool animate)
        {
            HexTileVisuals tileVisuals = Instantiate(visualsPrefab, tilesParent);
            tileVisuals.name = $"Hex({cell.x}, {cell.y})";
            tileVisuals.transform.position = grid.Math.HexToWorld(cell)
                .WithY(Game.Instance.Config.Decorations.BaseYOffset);
            tileVisuals.SetupForTile(tileData, grid, cell);
            if (animate)
            {
                tileVisuals.PlayAppearAnimation();
            }
            spawnedVisuals.Add(cell, tileVisuals);
        }

        private void DestroyTileVisuals(Vector2Int cell)
        {
            var tile = spawnedVisuals[cell];
            Destroy(tile.gameObject);
            spawnedVisuals.Remove(cell);
        }

        private void TryReplaceTileVisuals(Vector2Int cell, HexTileData newTile, HexGrid grid)
        {
            if (spawnedVisuals.TryGetValue(cell, out HexTileVisuals visuals) == false)
            {
                return;
            }

            if (visuals.GetTile().Equals(newTile))
            {
                return;
            }

            DestroyTileVisuals(cell);
            CreateTileVisuals(cell, newTile, grid, false);
        }

        private void HandleGhostTiles(GameModel newModel)
        {
            var grid = newModel.Grid;

            HashSet<Vector2Int> edgeCells = GetCellsForGhostTiles(newModel);

            foreach (Vector2Int cell in edgeCells)
            {
                if (spawnedGhostTiles.ContainsKey(cell) == false)
                {
                    CreateGhostTile(cell, grid);
                }
            }

            using var po2 = ListPool<Vector2Int>.Get(out var toDestroy);
            foreach ((Vector2Int cell, GameObject _) in spawnedGhostTiles)
            {
                if (edgeCells.Contains(cell) == false)
                {
                    toDestroy.Add(cell);
                }
            }

            foreach (Vector2Int cell in toDestroy)
            {
                DestroyGhostTile(cell);
            }
        }

        // TODO : move out of this class
        public static HashSet<Vector2Int> GetCellsForGhostTiles(GameModel model)
        {
            var nextTileToPlace = model.TileStack.PeekTile();
            if (nextTileToPlace?.IsStation ?? false)
            {
                return Game.Instance.GameMode.GetValidPlacementPositions(
                    model.Grid, model.LastPlacementCell, model.TileStack.PeekTile()).ToHashSet();
            }

            // Note: not using GameMode.GetValidPlacementPositions because there exists a setting that
            // requires the rails to fit. In such case, we still show ghost tiles on all edge tiles
            // and, instead, tint the preview tile red if it cannot be placed.
            HashSet<Vector2Int> edgeCells = new();
            model.Grid.GetEdgeCells(edgeCells);
            return edgeCells;
        }

        private void CreateGhostTile(Vector2Int cell, HexGrid grid)
        {
            if (spawnedGhostTiles.ContainsKey(cell))
            {
                return;
            }

            GameObject ghostTile = ghostTilePool.Get();
            Vector3 worldPos = grid.Math.HexToWorld(cell).WithY(Game.Instance.Config.Decorations.BaseYOffset);
            ghostTile.transform.position = worldPos;

            spawnedGhostTiles.Add(cell, ghostTile);
        }

        private void DestroyGhostTile(Vector2Int cell)
        {
            ghostTilePool.Release(spawnedGhostTiles[cell]);
            spawnedGhostTiles.Remove(cell);
        }
    }
}
