using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace NS.Core.GameModes.Common
{
    // District closing V1
    // A district is closed when:
    // 1. It is not possible to add more tiles to it
    // 2. All edges that make up a district have a pair, i.e., there is no edge touching an edge of different type
    // Other
    // - one tile cannot have more than 1 consecutive edge of the same type, i.e., AABBAA is not possible
    //
    // Recursive alg:
    // select random tile
    // keep a set of visited tiles
    // check all green edges
    // if matching:
    //      ask "are you a good tile"? (recursive call) -> idk let me check
    //      does not visit already visited cells, asks other edges
    //      one edge returns false -> it's not a good tile
    //      well then, if my neighbor is bad then I'm bad too, return false
    //
    // Downsides: It's damn hard to close a district in regular gameplay.

    public class DistrictFinder
    {
        public readonly struct EdgeLocator
        {
            public readonly Vector2Int cell;
            public readonly int edgeIndex;

            public EdgeLocator(Vector2Int cell, int edgeIndex)
            {
                this.edgeIndex = edgeIndex;
                this.cell = cell;
            }
        }

        public readonly struct Result
        {
            public readonly EdgeType districtType;
            // TODO : technically we can deduce cells from edge locators, but leaving this redundancy for now,
            // as I am not adding district edges to DFS search at the moment, and it should still work like before.
            public readonly Vector2Int[] districtCells;
            public readonly EdgeLocator[] districtEdges;
            public readonly Vector2Int startingCell;

            public Result(Vector2Int startingCell, EdgeType districtType, Vector2Int[] districtCells,
                EdgeLocator[] districtEdges)
            {
                this.districtType = districtType;
                this.districtCells = districtCells;
                this.districtEdges = districtEdges;
                this.startingCell = startingCell;
            }
        }

        public readonly struct Settings
        {
            public readonly bool requireMatchingEdges;

            public Settings(bool requireMatchingEdges)
            {
                this.requireMatchingEdges = requireMatchingEdges;
            }

            public static Settings Default => new(false);
        }

        private readonly Settings settings;

        public DistrictFinder(Settings settings)
        {
            this.settings = settings;
        }

        public bool TryFindClosedDistrict(Vector2Int cell, HexGrid grid, out Result result)
        {
            Result[] findResults = TryFindDistrictImpl(cell, grid);

            foreach (Result findResult in findResults)
            {
                Debug.Log($"[DistrictFinder] found district of type: {findResult.districtType}");
            }

            if (findResults.Length == 0)
            {
                result = new Result();
                return false;
            }

            if (findResults.Length > 1)
            {
                // TODO : handle this case, this should reward the player even more (some bonus/joined station?)
                Debug.LogWarning("[DistrictFinder] Only one station can be spawned!");
                // sort by most edges - player would likely want to close the bigger district.
                Array.Sort(findResults, (x, y) => y.districtEdges.Length.CompareTo(x.districtEdges.Length));
            }

            result = findResults[0];
            return true;
        }

        private Result[] TryFindDistrictImpl(Vector2Int cell, HexGrid grid)
        {
            Assert.IsTrue(grid.HasTile(cell));

            grid.TryGetTile(cell, out HexTileData tile);
            HashSet<EdgeType> edgeTypes = new();
            foreach (EdgeType edgeType in tile.edges)
            {
                if (edgeType.CanMakeDistrict())
                {
                    edgeTypes.Add(edgeType);
                }
            }

            List<Result> results = new();

            foreach (EdgeType edgeType in edgeTypes)
            {
                List<Vector2Int> districtCells = new();
                List<EdgeLocator> districtEdges = new();

                bool isDistrict;
                if (settings.requireMatchingEdges)
                {
                    List<Vector2Int> visited = new();
                    isDistrict = DfsDistrictFind(edgeType, cell, grid, visited, districtCells);
                }
                else
                {
                    isDistrict = FindDistrictFloodFill(edgeType, cell, grid, districtCells, districtEdges);
                }

                if (isDistrict)
                {
                    results.Add(new Result(cell, edgeType, districtCells.ToArray(), districtEdges.ToArray()));
                }
            }

            return results.ToArray();
        }

        /// Out results are only valid if method returned true.
        /// <returns>true if found district (root call only)</returns>
        private static bool DfsDistrictFind(EdgeType type, Vector2Int cell, HexGrid grid,
            List<Vector2Int> visited, List<Vector2Int> outDistrictCells)
        {
            Assert.IsTrue(grid.HasTile(cell));

            if (visited.Contains(cell))
            {
                return true; // if a tile was visited we assume it is good
            }

            visited.Add(cell);

            bool oneEdgeGoodType = false;
            for (int edgeIndex = 0; edgeIndex < 6; edgeIndex++)
            {
                grid.TryGetTile(cell, out HexTileData tile);
                EdgeType myEdge = tile.edges[edgeIndex];
                if (myEdge != type)
                {
                    continue;
                }

                oneEdgeGoodType = true;

                Vector2Int neighborCell = cell + HexGridMath.Directions[edgeIndex];
                if (grid.TryGetTile(neighborCell, out HexTileData neighborTile) == false)
                {
                    return false;
                }

                EdgeType neighborEdge = neighborTile.edges[HexGridMath.GetOppositeEdgeIndex(edgeIndex)];
                if (myEdge != neighborEdge)
                {
                    return false;
                }

                bool neighborBad = DfsDistrictFind(type, neighborCell, grid, visited, outDistrictCells) == false;
                if (neighborBad)
                {
                    return false;
                }
            }

            outDistrictCells.Add(cell);

            return oneEdgeGoodType;
        }

        /// Out results are only valid if method returned true.
        /// <returns>true if district a closed district was found starting given cell</returns>
        private static bool FindDistrictFloodFill(EdgeType type, Vector2Int startingCell, HexGrid grid, List<Vector2Int> outDistrictCells, List<EdgeLocator> outDistrictEdges)
        {
            Queue<Vector2Int> toVisit = new();
            HashSet<Vector2Int> visited = new();

            toVisit.Enqueue(startingCell);

            while (toVisit.Count > 0)
            {
                Vector2Int cell = toVisit.Dequeue();

                if (visited.Add(cell) == false)
                {
                    continue;
                }

                bool gotTile = grid.TryGetTile(cell, out HexTileData tile);
                Assert.IsTrue(gotTile);

                bool oneEdgeGoodType = false;
                for (int edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    EdgeType myEdge = tile.edges[edgeIndex];
                    if (myEdge != type)
                    {
                        continue;
                    }

                    oneEdgeGoodType = true;
                    outDistrictEdges.Add(new EdgeLocator(cell, edgeIndex));

                    Vector2Int neighborCell = cell + HexGridMath.Directions[edgeIndex];
                    if (grid.TryGetTile(neighborCell, out HexTileData neighborTile) == false)
                    {
                        // if we found a cell that has an edge of correct type
                        // facing empty space this means the district is not closed
                        return false;
                    }

                    // only visit neighboring tiles if there's an edge connection
                    EdgeType neighborEdge = neighborTile.edges[HexGridMath.GetOppositeEdgeIndex(edgeIndex)];
                    if (myEdge != neighborEdge)
                    {
                        continue;
                    }

                    // Note: will add duplicates, but it's fine because we skip them when dequeuing
                    if (visited.Contains(neighborCell) == false)
                    {
                        toVisit.Enqueue(neighborCell);
                    }
                }

                if (oneEdgeGoodType)
                {
                    outDistrictCells.Add(cell);
                }
            }

            // if we didn't abort up to here than it means the district is closed
            return true;
        }
    }
}
