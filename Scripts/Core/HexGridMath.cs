using System.Collections.Generic;
using MessagePack;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.Core
{
    [MessagePackObject]
    public class HexGridMath
    {
        /// Offsets to neighboring directions. Start at the right, go clockwise.
        /// https://www.redblobgames.com/grids/hexagons/#coordinates-axial
        public static readonly Vector2Int[] Directions =
        {
            //   q,  r
            new(+1,  0),
            new( 0, +1),
            new(-1, +1),
            new(-1,  0),
            new( 0, -1),
            new(+1, -1),
        };

        public const int INVALID_EDGE = -1;

        [Key(0)]
        public readonly float hexRadius;

        public HexGridMath(float hexRadius)
        {
            this.hexRadius = hexRadius;
        }

        public Vector3 HexToWorld(Vector2Int pos)
        {
            return HexToWorld(pos.x, pos.y);
        }

        public Vector3 HexToWorld(int q, int r)
        {
            // https://www.redblobgames.com/grids/hexagons/#pixel-to-hex
            // Note: flip Y, because the blog uses pixel coordinates, which are Y down

            float x = hexRadius * Mathf.Sqrt(3f) * (q + r / 2f);
            float z = hexRadius * 1.5f * r * -1f;
            return new Vector3(x, 0, z);
        }

        public Vector2Int WorldToHex(Vector3 position)
        {
            // https://www.redblobgames.com/grids/hexagons/#pixel-to-hex
            // Note: flip Y, because the blog uses pixel coordinates, which are Y down

            float x = position.x / hexRadius;
            float y = position.z / hexRadius * -1f;
            float q = Mathf.Sqrt(3f) / 3f * x - 1f / 3f * y;
            float r = 2f / 3f * y;

            return AxialRound(q, r);
        }

        private static Vector2Int AxialRound(float x, float y)
        {
            int xGrid = Mathf.RoundToInt(x);
            int yGrid = Mathf.RoundToInt(y);

            // remainder
            x -= xGrid;
            y -= yGrid;

            if (Mathf.Abs(x) >= Mathf.Abs(y))
            {
                return new Vector2Int(xGrid + Mathf.RoundToInt(x + 0.5f * y), yGrid);
            }
            else
            {
                return new Vector2Int(xGrid, yGrid + Mathf.RoundToInt(y + 0.5f * x));
            }
        }

        public Vector2[] GetPolygonCorners(int q, int r)
        {
            var corners = new Vector2[6];

            Vector3 center = HexToWorld(q, r);
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = GetHexCornerOffset(i);
                corners[i] = new Vector2(center.x + offset.x, center.z + offset.y);
            }

            return corners;
        }

        public Vector2[] GetPolygonCornersOffsets()
        {
            var corners = new Vector2[6];

            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = GetHexCornerOffset(i);
                corners[i] = new Vector2(offset.x, offset.y);
            }

            return corners;
        }

        public Vector2 GetHexCornerOffset(int corner)
        {
            const float startAngle = 0.5f; // pointy top, 30deg

            float angle = 2.0f * Mathf.PI * (startAngle - corner) / 6f;

            return new Vector2(hexRadius * Mathf.Cos(angle), hexRadius * Mathf.Sin(angle));
        }

        public static int GetOppositeEdgeIndex(int edgeIndex)
        {
            return (edgeIndex + 3) % 6;
        }

        public int GetCellsInRange(Vector2Int center, int radius, List<Vector2Int> results)
        {
            // https://www.redblobgames.com/grids/hexagons/#distances

            int written = 0;

            for (int q = -radius; q <= radius; q++)
            {
                for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
                {
                    results.Add(center + new Vector2Int(q, r));
                    written++;
                }
            }

            return written;
        }

        public int GetCellsInRange(Vector2Int center, int innerIncl, int outerIncl, List<Vector2Int> results)
        {
            // TODO : probably can be optimized by using some smart math instead

            using var po = ListPool<Vector2Int>.Get(out var tempList);

            GetCellsInRange(center, outerIncl, tempList);
            var outerSet = new HashSet<Vector2Int>(tempList);

            tempList.Clear();
            GetCellsInRange(center, innerIncl - 1, tempList); // -1 because inclusive

            outerSet.ExceptWith(tempList);

            results.AddRange(outerSet);

            return outerSet.Count;
        }

        /// Returns the world space center point of the edge at given index
        public Vector3 EdgeToWorld(Vector2Int cell, int edgeIndex)
        {
            Vector3 p1 = HexToWorld(cell);
            Vector3 p2 = p1 + GetHexCornerOffset(edgeIndex).To3();
            Vector3 p3 = p1 + GetHexCornerOffset((edgeIndex + 1) % 6).To3();
            return (p2 + p3) / 2.0f;
        }

        /// Returns the world space center point of the equilateral triangle at given edge index
        public Vector3 EdgeTriangleToWorld(Vector2Int cell, int edgeIndex)
        {
            Vector3 p1 = HexToWorld(cell); // center of hex, one of triangle vertices
            Vector3 p2 = p1 + GetHexCornerOffset(edgeIndex).To3();
            Vector3 p3 = p1 + GetHexCornerOffset((edgeIndex + 1) % 6).To3();
            return (p1 + p2 + p3) / 3.0f;
        }

        /// Returns the index (in from cell domain) of the edge that lies between 2 cells
        /// if they are neighbors, and INVALID_EDGE otherwise.
        public int EdgeBetweenCells(Vector2Int from, Vector2Int to)
        {
            Vector2Int direction = to - from;

            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int dir = Directions[i];
                if (direction == dir)
                {
                    return i;
                }
            }

            return INVALID_EDGE;
        }
    }
}
