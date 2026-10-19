using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace NS.Core
{
    public struct HexGridParams
    {
        public float HexRadius { get; init; }

        public static HexGridParams Default => new() { HexRadius = 1 };
    }

    /// Note: Pointy top
    [MessagePackObject]
    public class HexGrid
    {
        [Key(0)]
        public SerializationData SerializationProxy => new(tiles, Math.hexRadius);

        [IgnoreMember] // TODO : now that this is public we probably can drop the serialization proxy
        public readonly Dictionary<Vector2Int, HexTileData> tiles = new();

        [IgnoreMember]
        public HexGridMath Math { get; }

        [IgnoreMember]
        public int TileCount => tiles.Count;

        public HexGrid(SerializationData serializationData)
        {
            Math = new HexGridMath(serializationData.hexRadius);
            tiles = new Dictionary<Vector2Int, HexTileData>(serializationData.tiles);
        }

        public HexGrid(HexGridParams hexGridParams)
        {
            Math = new HexGridMath(hexGridParams.HexRadius);
        }

        public void AddTile(HexTileData tile, Vector2Int pos)
        {
            Assert.IsTrue(!HasTile(pos));
            tiles.Add(pos, tile);
        }

        public void ReplaceTile(Vector2Int pos, HexTileData tile)
        {
            Assert.IsTrue(HasTile(pos));
            tiles[pos] = tile;
        }

        public bool TryGetTile(Vector2Int pos, out HexTileData hexTile)
        {
            return tiles.TryGetValue(pos, out hexTile);
        }

        public HexTileData GetTile(Vector2Int cell)
        {
            Assert.IsTrue(HasTile(cell));
            return tiles[cell];
        }

        public bool HasTile(Vector2Int cell)
        {
            return tiles.ContainsKey(cell);
        }

        public bool TryRemoveTile(Vector2Int pos)
        {
            return tiles.Remove(pos);
        }

        public int GetNumEdgesMatchingNeighbors(Vector2Int cell)
        {
            bool hasTile = TryGetTile(cell, out HexTileData me);
            Assert.IsTrue(hasTile);

            int numMatchingEdges = 0;

            for (int i = 0; i < HexGridMath.Directions.Length; i++)
            {
                Vector2Int direction = HexGridMath.Directions[i];
                if (TryGetTile(cell + direction, out HexTileData neighbor))
                {
                    bool edgeMatches = me.edges[i] == neighbor.edges[HexGridMath.GetOppositeEdgeIndex(i)];
                    if (edgeMatches)
                    {
                        numMatchingEdges++;
                    }
                }
            }

            Assert.IsTrue(numMatchingEdges >= 0 && numMatchingEdges <= 6);

            return numMatchingEdges;
        }

        public void GetNeighborsWithRailConnection(Vector2Int cell, List<Vector2Int> results)
        {
            TryGetTile(cell, out HexTileData me);

            for (int i = 0; i < HexGridMath.Directions.Length; i++)
            {
                Vector2Int neighborCell = cell + HexGridMath.Directions[i];
                if (TryGetTile(neighborCell, out HexTileData neighborTile) == false)
                {
                    continue;
                }

                if (me.edges[i] == EdgeType.Rail && neighborTile.edges[HexGridMath.GetOppositeEdgeIndex(i)] == EdgeType.Rail)
                {
                    results.Add(neighborCell);
                }
            }
        }

        public void GetStationTiles(List<Vector2Int> results)
        {
            foreach ((Vector2Int cell, HexTileData tile) in tiles)
            {
                if (tile.IsStation)
                {
                    results.Add(cell);
                }
            }
        }

        public bool TryGetStationData(Vector2Int cell, out StationData stationData)
        {
            if (TryGetTile(cell, out HexTileData tileData) && tileData.IsStation)
            {
                stationData = tileData.stationData;
                return true;
            }

            stationData = default;
            return false;
        }

        public bool HasStation(Vector2Int cell)
        {
            return TryGetStationData(cell, out _);
        }

        /// Edge cell is an EMPTY cell that is touching an existing cell
        public void GetEdgeCells(HashSet<Vector2Int> results)
        {
            foreach ((Vector2Int pos, HexTileData tileData) in tiles)
            {
                foreach (Vector2Int dir in HexGridMath.Directions)
                {
                    Vector2Int candidate = pos + dir;

                    if (!HasTile(candidate))
                    {
                        results.Add(candidate);
                    }
                }
            }
        }

        public bool IsCellFullyEnclosed(Vector2Int cell)
        {
            foreach (Vector2Int dir in HexGridMath.Directions)
            {
                Vector2Int neighbor = cell + dir;
                if (HasTile(neighbor) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasCellAnyNeighbor(Vector2Int cell)
        {
            return IsAnyCellWithin(cell, 1);
        }

        public bool IsAnyCellWithin(Vector2Int cell, int radius)
        {
            using var po = ListPool<Vector2Int>.Get(out var cells);
            Math.GetCellsInRange(cell, radius, cells);

            foreach (Vector2Int c in cells)
            {
                if (HasTile(c))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAnyStationWithin(Vector2Int cell, int radius)
        {
            using var po = ListPool<Vector2Int>.Get(out var cells);
            Math.GetCellsInRange(cell, radius, cells);

            foreach (Vector2Int c in cells)
            {
                if (TryGetTile(c, out HexTileData tile) && tile.IsStation)
                {
                    return true;
                }
            }

            return false;
        }

        private bool Equals(HexGrid other)
        {
            if (Mathf.Approximately(Math.hexRadius, other.Math.hexRadius) == false)
            {
                return false;
            }

            if (TileCount != other.TileCount)
            {
                return false;
            }

            foreach ((Vector2Int key, HexTileData value) in tiles)
            {
                if (other.tiles.TryGetValue(key, out HexTileData otherValue) == false)
                {
                    return false;
                }

                if (otherValue.Equals(value) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HexGrid)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Should not be hashed");
        }

        public Vector3 GetTilesCenterWorld()
        {
            Vector3 sum = Vector3.zero;

            foreach ((Vector2Int cell, HexTileData _) in tiles)
            {
                Vector3 worldPos = Math.HexToWorld(cell);
                Assert.IsTrue(worldPos.y == 0);
                sum += worldPos;
            }

            return sum / tiles.Count;
        }

        /// Returns all cells around existing cells that are no further than radius from them
        public void Dilate(int radius, HashSet<Vector2Int> results)
        {
            using var po1 = ListPool<Vector2Int>.Get(out var cellsInRange);

            foreach (Vector2Int originalCell in tiles.Keys)
            {
                cellsInRange.Clear();
                Math.GetCellsInRange(originalCell, radius, cellsInRange);
                foreach (Vector2Int cell in cellsInRange)
                {
                    if (tiles.ContainsKey(cell))
                    {
                        continue;
                    }

                    results.Add(cell);
                }
            }
        }

        [MessagePackObject]
        public class SerializationData
        {
            [Key(0)]
            public readonly Dictionary<Vector2Int, HexTileData> tiles;

            [Key(1)]
            public readonly float hexRadius;

            public SerializationData(Dictionary<Vector2Int, HexTileData> tiles, float hexRadius)
            {
                this.tiles = tiles;
                this.hexRadius = hexRadius;
            }
        }
    }
}
