using System.Collections.Generic;
using NS.Core;
using NS.Core.Model;
using NS.Visuals;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.UI
{
    public class StationDataTooltipManager : MonoBehaviour
    {
        [SerializeField] private StationDataTooltip template;
        [SerializeField] private PreviewTile previewTile;

        private readonly Dictionary<Vector2Int, StationDataTooltip> spawnedTooltips = new();
        private StationDataTooltip previewTileTooltip;

        private void Awake()
        {
            template.gameObject.SetActive(false);

            previewTileTooltip = Instantiate(template, transform);
            previewTileTooltip.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Game.Instance.ModelChanged += OnGameModelChanged;

            RefreshForModel(Game.Instance.Model, Game.ModelChangeReason.LoadGame);
        }

        private void OnDisable()
        {
            Game.Instance.ModelChanged -= OnGameModelChanged;
        }

        public void AnimateUpdate(Vector2Int stationCell, StationData newData)
        {
            if (spawnedTooltips.TryGetValue(stationCell, out var tooltip))
            {
                tooltip.Refresh(newData, animate: true);
            }
            else
            {
                Debug.LogError($"[StationDataTooltipManager] No tooltip for station at {stationCell}");
            }
        }

        private void OnGameModelChanged(GameModel oldModel, GameModel newModel, Game.ModelChangeReason reason)
        {
            RefreshForModel(newModel, reason);
        }

        private void RefreshForModel(GameModel model, Game.ModelChangeReason modelChangeReason)
        {
            HexGrid grid = model.Grid;

            foreach ((Vector2Int cell, HexTileData tile) in grid.tiles)
            {
                if (tile.IsStation == false)
                {
                    continue;
                }

                if (spawnedTooltips.TryGetValue(cell, out StationDataTooltip existingTooltip))
                {
                    // if we placed a tile there is a visuals sequence that will refresh the tooltip
                    if (modelChangeReason != Game.ModelChangeReason.PlaceTile)
                    {
                        existingTooltip.Refresh(tile.stationData);
                    }
                }
                else
                {
                    CreateStationTooltip(cell, tile.stationData);
                }
            }

            using var po = ListPool<Vector2Int>.Get(out var toDestroy);
            foreach ((Vector2Int cell, StationDataTooltip tooltip) in spawnedTooltips)
            {
                if (grid.HasStation(cell) == false)
                {
                    toDestroy.Add(cell);
                }
            }

            foreach (Vector2Int cell in toDestroy)
            {
                DestroyStationTooltip(cell);
            }
        }

        private void CreateStationTooltip(Vector2Int cell, StationData stationData)
        {
            StationDataTooltip tooltip = Instantiate(template, transform);
            tooltip.SetPosition(Game.Instance.Grid.Math.HexToWorld(cell));
            tooltip.Refresh(stationData);
            tooltip.gameObject.SetActive(true);

            spawnedTooltips.Add(cell, tooltip);
        }

        private void DestroyStationTooltip(Vector2Int cell)
        {
            Destroy(spawnedTooltips[cell].gameObject);
            spawnedTooltips.Remove(cell);
        }

        private void Update()
        {
            foreach ((Vector2Int cell, StationDataTooltip tooltip) in spawnedTooltips)
            {
                tooltip.SetPosition(Game.Instance.Grid.Math.HexToWorld(cell));
            }

            if (previewTile != null && previewTile.StationData.HasValue)
            {
                Vector3 worldPos = previewTile.GetPosition();
                previewTileTooltip.SetPosition(worldPos);
                previewTileTooltip.Refresh(previewTile.StationData.Value, false);
                previewTileTooltip.gameObject.SetActive(previewTile.IsVisible());
            }
            else
            {
                previewTileTooltip.gameObject.SetActive(false);
            }
        }
    }
}
