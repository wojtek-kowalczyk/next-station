using System;
using System.Collections.Generic;
using NS.Core.Blueprints;
using NS.Core.GameModes.Common;
using NS.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace NS.Core.GameModes.Basic
{
    public class DistrictCloser : MonoBehaviour
    {
        [Serializable]
        private class DistrictSizeResourceAmountPair
        {
            public int numEdgesInDistrict;
            public int amountResourcesToGive;
        }

        [SerializeField] private List<DistrictSizeResourceAmountPair> resourceAmounts;
        [SerializeField] private int minEdgesToCloseDistrict = 8;
        [SerializeField] private int minTilesToCloseDistrict = 3;

        public bool IsEligibleForStationPlacement(DistrictFinder.Result findResult)
        {
            return findResult.districtCells.Length >= minTilesToCloseDistrict &&
                   findResult.districtEdges.Length >= minEdgesToCloseDistrict;
        }

        public bool TrySpawnStationOnDistrictClose(Vector2Int userPlacedCell, HexGrid grid,
            DistrictFinder.Result findResult, Random rand)
        {
            // Station placement on district close:
            // 1. try to promote last placed tile to station
            // 2. if not possible, try to find any tile that belongs to this district that
            //      can be promoted to a station
            // 3. if no station can be placed for district spawn, add bonus points and don't spawn station.
            //
            // promotion:
            // take all edges other than the closed district type and convert them into rail, if that
            // edge is not facing another tile (since that would spawn blocked rail).

            if (IsEligibleForStationPlacement(findResult) == false)
            {
                return false;
            }

            if (TryTransformAndReplaceTile(userPlacedCell))
            {
                return true;
            }

            using var po = ListPool<Vector2Int>.Get(out var candidates);
            GetNonFullyEnclosedCells(findResult.districtCells, grid, candidates);

            if (candidates.Count == 0)
            {
                return false;
            }

            candidates.Shuffle(rand);
            foreach (Vector2Int candidateCell in candidates)
            {
                if (TryTransformAndReplaceTile(candidateCell))
                {
                    return true;
                }
            }

            return false;

            bool TryTransformAndReplaceTile(Vector2Int cell)
            {
                // retrieve original placement rotation and forward it to the replacement tile.
                // we need the placement rotation to be preserved for visuals to work correctly.
                if (TryTransformIntoStation(cell, grid, findResult.districtType,
                        out EdgeType[] edges))
                {
                    ReplaceTileWithStation(cell, findResult.districtType, edges,
                        findResult.districtEdges.Length);
                    return true;
                }

                return false;
            }
        }

        private static void GetNonFullyEnclosedCells(IEnumerable<Vector2Int> cells, HexGrid grid,
            List<Vector2Int> result)
        {
            foreach (Vector2Int cell in cells)
            {
                if (grid.IsCellFullyEnclosed(cell) == false)
                {
                    result.Add(cell);
                }
            }
        }

        private void ReplaceTileWithStation(Vector2Int cell, EdgeType closedDistrictType, EdgeType[] stationEdgeTiles, int numEdgesInDistrict)
        {
            var stationBp = GetStationBlueprint(closedDistrictType, numEdgesInDistrict);
            var stationData = stationBp.Generate(new Random());
            // HACK : we're cheating here a little bit.
            // put given rotation in tile data, but pass tile placement data with zero rotation, so that
            // the tile ends up in a grid as is. This is because the edges are already fitting the
            // neighbors, so no rotation should occur, but we still need the original tile's rotation
            // to select decorations correctly later
            var tileData = new HexTileData(stationEdgeTiles, 0, 0, stationData);
            var tpd = new TilePlacementData(tileData, cell, 0, false);
            Game.Instance.ReplaceTile(tpd);
        }

        private StationDataBlueprint GetStationBlueprint(EdgeType districtType, int numEdgesInDistrict)
        {
            ResourceType resource;
            switch (districtType)
            {
                case EdgeType.Mountain:
                    resource = ResourceType.Steel;
                    break;
                case EdgeType.Fields:
                    resource = ResourceType.Wood;
                    break;
                case EdgeType.Town:
                    resource = ResourceType.People;
                    break;

                case EdgeType.Rail:
                case EdgeType.Basic:
                default:
                    throw new ArgumentOutOfRangeException(nameof(districtType), districtType, null);
            }

            int amount = 1;
            foreach (DistrictSizeResourceAmountPair p in resourceAmounts)
            {
                if (p.numEdgesInDistrict <= numEdgesInDistrict)
                {
                    amount = Mathf.Max(p.amountResourcesToGive, amount);
                }
            }

            return new StationDataBlueprint(wants: ResourceType.None, 0, gives: resource, amount);
        }

        private static bool TryTransformIntoStation(Vector2Int cell, HexGrid grid, EdgeType districtType,
            out EdgeType[] newTileEdges)
        {
            bool didTransform = false;

            bool ok = grid.TryGetTile(cell, out HexTileData tile);
            Assert.IsTrue(ok);

            newTileEdges = new EdgeType[6];

            for (int i = 0; i < newTileEdges.Length; i++)
            {
                newTileEdges[i] = tile.edges[i];

                if (newTileEdges[i] == districtType)
                {
                    continue;
                }

                // TODO: temporarily disabled, because we don't have decorations that would match
                // all the possibilities that this would create (like 1,2,4 rails)
                //
                // Vector2Int neighborPos = cell + HexGridMath.Directions[i];
                // if (grid.TryGetTile(neighborPos, out HexTileData neighborTile))
                // {
                //     if (neighborTile.Edges[HexGridMath.GetOppositeEdgeIndex(i)] != EdgeType.Rail)
                //     {
                //         continue;
                //     }
                // }

                newTileEdges[i] = EdgeType.Rail;
                didTransform = true;
            }

            if (grid.IsCellFullyEnclosed(cell))
            {
                // this would create a station that can never be connected
                didTransform = false;
            }

            return didTransform;
        }

        private void OnValidate()
        {
            int last = -1;
            foreach (DistrictSizeResourceAmountPair p in resourceAmounts)
            {
                if (last < p.numEdgesInDistrict)
                {
                    last = p.numEdgesInDistrict;
                }
                else
                {
                    Debug.LogWarning($"{nameof(DistrictCloser)} thresholds must be increasing");
                    break;
                }
            }
        }
    }
}
