using System;
using System.Collections.Generic;
using System.Linq;
using NS.Audio;
using NS.Core.Blueprints;
using NS.Core.GameModes.Common;
using NS.Core.Model;
using NS.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace NS.Core.GameModes.Basic
{
    public class BasicGameMode : MonoBehaviour
    {
        [Serializable]
        public class PointAwards
        {
            public int forMatchingEdge = 5;
            public int forPerfectPlacement = 20;
            public int forEachEdgeInDistrict = 3;
        }

        [Serializable]
        private class TileAwards
        {
            public int forStationConnection = 10;
        }

        [SerializeField] private HexTileDataBlueprintSO startingTileBp;
        [SerializeField] private HexTileDataBlueprintSO firstStationTileBp;
        [SerializeField] private StationSpawner stationSpawner;
        [SerializeField] private DistrictCloser districtCloser;
        [SerializeField] private bool railsMustFit;
        [SerializeField] private float delayBetweenPointPopups = 0.1f;
        [SerializeField] private float delayBeforePerfectPlacementText = 1.0f;
        [SerializeField] private PointAwards pointAwards;
        [SerializeField] private TileAwards tileAwards;

        public delegate void DistrictClosedHandler(DistrictFinder.Result findResult, bool gotReward);
        public event DistrictClosedHandler DistrictClosed; // keep in mind many may fire in one frame

        public event Action<RailNetwork.BalancingResult> RailNetworkBalanced;

        private readonly DistrictFinder districtFinder = new(DistrictFinder.Settings.Default);

        private readonly HashSet<Vector2Int> validPlacementPositions = new();

        public PointAwards PointAwardValues => pointAwards;
        public StationSpawner StationSpawner => stationSpawner;

        public void PlaceStartingTiles(GameModel model)
        {
            Assert.IsTrue(model.Grid != null && model.Grid.TileCount == 0);

            HexTileData startingTile = startingTileBp.Blueprint.Generate(model.Rand);
            var startingTilePlacementData = new TilePlacementData(startingTile, new Vector2Int(0, 0), 0, false);
            Game.Instance.PlaceTile(startingTilePlacementData);

            var networks = FindRailNetworks(model.Grid);
            var stationDataBlueprint = stationSpawner.GetStationDataBlueprint(RailNetwork.GetSupplyInfo(networks), model.Rand);
            var stationTile = firstStationTileBp.Blueprint.Generate(model.Rand, stationDataBlueprint);
            Vector2Int[] positions = GetValidPlacementPositions(model.Grid, startingTilePlacementData.cell, stationTile).ToArray();
            Vector2Int position = positions.RandomElement(model.Rand);
            Game.Instance.PlaceTile(new TilePlacementData(stationTile, position, model.Rand.NextInt(0, 6), false));
        }

        public IReadOnlyCollection<Vector2Int> GetValidPlacementPositions(HexGrid grid, Vector2Int lastPlacementCell, HexTileData rotatedTile)
        {
            if (rotatedTile == null)
            {
                validPlacementPositions.Clear();
                return validPlacementPositions;
            }

            RefreshValidPlacementPositionsIfNeeded(grid, lastPlacementCell, rotatedTile);

            return validPlacementPositions;
        }

        public IReadOnlyCollection<Vector2Int> GetValidPlacementPositionsForCurrentGameState()
        {
            return GetValidPlacementPositions(
                Game.Instance.Model.Grid,
                Game.Instance.Model.LastPlacementCell,
                Game.Instance.TileStack.PeekTile()?.Rotated(Game.Instance.CurrentTileRotation));
        }

        public bool CanPlaceTile(GameModel model, TilePlacementData placementData)
        {
            if (model.Grid.TileCount == 0)
            {
                // first tile can be placed anywhere
                return true;
            }

            // TODO : "baking" rotation into a tile is ugly.
            // Rotated tile is not type safe. The semantics are vague, revisit it.
            HexTileData rotatedTile = placementData.nonRotatedTile.Rotated(placementData.rotation);

            return GetValidPlacementPositions(model.Grid, model.LastPlacementCell, rotatedTile).Contains(placementData.cell);
        }

        // Note: pass in a rotated tile into this method to make sure rotation causes a refresh
        private void RefreshValidPlacementPositionsIfNeeded(HexGrid grid, Vector2Int lastPlacementCell, HexTileData rotatedTile)
        {
            Assert.IsNotNull(grid);

            if (rotatedTile.IsStation)
            {
                RefreshForStation();
            }
            else
            {
                RefreshForRegularTile();
            }

            return;

            void RefreshForStation()
            {
                using var po = ListPool<Vector2Int>.Get(out var cells);
                stationSpawner.GenerateCandidateCells(grid, lastPlacementCell, cells);

                if (cells.Count > 0)
                {
                    validPlacementPositions.Clear();
                    foreach (Vector2Int cell in cells)
                    {
                        validPlacementPositions.Add(cell);
                    }
                }
                else
                {
                    // In the rare case where you don't get any candidate cells for station placement
                    // we generate like for a regular tile. This is most unfortunate, but it is better
                    // than softlocking the user by not allowing them to place the station.
                    // This should never happen, this is just a failsafe.
                    Debug.LogError("[BasicGameMode] Failed to generate any candidate cells for station.");
                    RefreshForRegularTile();
                }
            }

            void RefreshForRegularTile()
            {
                validPlacementPositions.Clear();
                using var po = HashSetPool<Vector2Int>.Get(out var cells);
                grid.GetEdgeCells(cells);
                foreach (Vector2Int cell in cells)
                {
                    if (railsMustFit == false || RailsFit(grid, rotatedTile, cell))
                    {
                        validPlacementPositions.Add(cell);
                    }
                }
            }
        }

        private static bool RailsFit(HexGrid grid, HexTileData tile, Vector2Int pos)
        {
            for (int i = 0; i < 6; i++)
            {
                if (grid.TryGetTile(pos + HexGridMath.Directions[i], out HexTileData otherTile) == false)
                {
                    continue;
                }

                EdgeType edge1 = tile.edges[i];
                EdgeType otherEdge1 = otherTile.edges[HexGridMath.GetOppositeEdgeIndex(i)];
                if (edge1 == EdgeType.Rail ^ otherEdge1 == EdgeType.Rail)
                {
                    return false;
                }
            }

            return true;
        }

        public void UpdateGameModel(GameModel model, TilePlacementData placementData)
        {
            HexTileData rotatedTile = placementData.nonRotatedTile.Rotated(placementData.rotation);

            // cache parts of previous model
            int oldPointsValue = model.Points;
            List<RailNetwork> oldNetworks = FindRailNetworks(model.Grid);

            // put the tile in the grid
            model.Grid.AddTile(rotatedTile, placementData.cell);
            model.LastPlacementCell = placementData.cell;

            AwardPointsForTilePlacement(model, rotatedTile, placementData.cell);

            // handle district closing
            if (districtFinder.TryFindClosedDistrict(placementData.cell, model.Grid, out DistrictFinder.Result result))
            {
                var random = new Random(model.Rand.NextInt());
                OnDistrictClosed(model, placementData, result, random);
            }

            // handle station connections
            List<RailNetwork> newNetworks = FindRailNetworks(model.Grid);
            bool connectionHappened = DidConnectionHappen(oldNetworks, newNetworks);
            if (connectionHappened)
            {
                AwardTiles(model, placementData.cell);
                List<RailNetwork.BalancingResult> results = BalanceRailNetworks(newNetworks);
                foreach (RailNetwork.BalancingResult balancingResult in results)
                {
                    if (balancingResult.IsEmpty == false)
                    {
                        RailNetworkBalanced?.Invoke(balancingResult);
                    }
                }

                AudioManager.Instance?.TryPlaySoundEffect(Sounds.STATION_CONNECT);
            }

            // handle station spawner level/thresholds
            if (oldPointsValue < model.Points)
            {
                var random = new Random(model.Rand.NextInt());
                stationSpawner.OnPointsAdded(model, oldPointsValue, model.Points, newNetworks, random);
            }
        }

        private void OnDistrictClosed(GameModel model, TilePlacementData placementData, DistrictFinder.Result result, Random rand)
        {
            int points = result.districtEdges.Length * pointAwards.forEachEdgeInDistrict;
            AwardPoints(model, points);

            bool wasStationPlaced = false;
            if (districtCloser.IsEligibleForStationPlacement(result))
            {
                wasStationPlaced = districtCloser.TrySpawnStationOnDistrictClose(placementData.cell, model.Grid, result, rand);
                if (wasStationPlaced == false)
                {
                    Vector3 cellWorldPos = model.Grid.Math.HexToWorld(placementData.cell);
                    Game.Instance.FeedbackText?.PutText("Failed to create station", cellWorldPos);
                }
                else
                {
                    AudioManager.Instance?.TryPlaySoundEffect(Sounds.STATION_SPAWN);
                }
            }

            DistrictClosed?.Invoke(result, wasStationPlaced);
        }

        private static List<RailNetwork.BalancingResult> BalanceRailNetworks(IReadOnlyList<RailNetwork> networks)
        {
            List<RailNetwork.BalancingResult> results = new();

            RailNetwork.SupplyInfo supplyBefore = RailNetwork.GetSupplyInfo(networks);

            foreach (RailNetwork railNetwork in networks)
            {
                results.Add(railNetwork.Balance());
            }

            RailNetwork.SupplyInfo supplyAfter = RailNetwork.GetSupplyInfo(networks);
            if (supplyBefore.Equals(supplyAfter) == false)
            {
                Debug.LogWarning("[BasicGameMode] Balancing changed supply values!\n" +
                                 $"Supply before balancing: {supplyBefore}\n" +
                                 $"Supply after balancing: {supplyAfter}");
            }

            return results;
        }

        private void AwardTiles(GameModel model, Vector2Int forPlacementInCell)
        {
            int amount = tileAwards.forStationConnection;
            Game.Instance.FeedbackText?.PutText("Stations connected!", model.Grid.Math.HexToWorld(forPlacementInCell));
            Game.Instance.FeedbackText?.PutText($"+{amount} Tiles", model.Grid.Math.HexToWorld(forPlacementInCell), 1.5f);
            model.TileStack.AddTiles(amount);
        }

        private static bool DidConnectionHappen(IReadOnlyList<RailNetwork> oldNetworks, IReadOnlyList<RailNetwork> newNetworks)
        {
            if (oldNetworks.Count >= newNetworks.Count)
            {
                return Utils.DoListsContainEqualElements(oldNetworks, newNetworks) == false;
            }

            return false;
        }

        public static List<RailNetwork> FindRailNetworks(HexGrid grid)
        {
            List<RailGraphBuilder.RailGraph> railGraphs = RailGraphBuilder.BuildRailGraphs(grid);

            List<RailNetwork> networks = new();

            foreach (var graph in railGraphs)
            {
                var network = new RailNetwork(graph.connectedStations, grid);
                networks.Add(network);
            }

            return networks;
        }

        private void AwardPointsForTilePlacement(GameModel model, HexTileData tile, Vector2Int cell)
        {
            Assert.IsTrue(model.Grid.TryGetTile(cell, out HexTileData placed) && placed.Equals(tile));

            int numMatchedEdges = 0;
            for (int i = 0; i < HexGridMath.Directions.Length; i++)
            {
                if (model.Grid.TryGetTile(cell + HexGridMath.Directions[i], out HexTileData neighbor))
                {
                    bool edgeMatches = tile.edges[i] != EdgeType.Basic &&
                                       tile.edges[i] == neighbor.edges[HexGridMath.GetOppositeEdgeIndex(i)];
                    if (edgeMatches)
                    {
                        Vector3 edgeWorldPos = model.Grid.Math.EdgeToWorld(cell, i);
                        AwardPoints(model, pointAwards.forMatchingEdge, edgeWorldPos, numMatchedEdges * delayBetweenPointPopups);
                        numMatchedEdges++;
                    }
                }
            }

            if (numMatchedEdges == 6)
            {
                Vector3 pos = model.Grid.Math.HexToWorld(cell);
                AwardPoints(model, pointAwards.forPerfectPlacement, pos);
                Game.Instance.FeedbackText?.PutText($"PERFECT!", pos, delayBeforePerfectPlacementText);
            }
        }

        private static void AwardPoints(GameModel model, int amount)
        {
            Assert.IsTrue(amount > 0);
            model.Points += amount;
        }

        // pass in the position of where the thing that added points happened. Will show feedback text there.
        private static void AwardPoints(GameModel model, int amount, Vector3 worldPos, float delay = 0)
        {
            AwardPoints(model, amount);
            Game.Instance.FeedbackText?.PutText($"+{amount}", worldPos, delay);
        }
    }
}
