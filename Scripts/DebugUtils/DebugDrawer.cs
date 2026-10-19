using System;
using NS.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.DebugUtils
{
    public static class DebugDrawer
    {
        private static Color GetEdgeColor(EdgeType edgeType)
        {
            return edgeType switch
            {
                EdgeType.Town => Color.red,
                EdgeType.Mountain => Color.gray4,
                EdgeType.Fields => Color.forestGreen,
                EdgeType.Basic => Color.softYellow,
                EdgeType.Rail => new Color(0.4f, 0.4f, 1.0f),
                _ => throw new ArgumentOutOfRangeException(nameof(edgeType), edgeType, null)
            };
        }

        public static void DrawTile(Vector2Int cell,
            HexGridMath hexGridMath,
            HexTileData tileData = null,
            float duration = 0f,
            Color? railColor = null)
        {
            Vector2 center = hexGridMath.HexToWorld(cell).To2();
            Vector2[] offsets = hexGridMath.GetPolygonCornersOffsets();

            for (int i = 0; i < offsets.Length; i++)
            {
                Vector3 start = (center + offsets[i]).To3();
                Vector3 end = (center + offsets[(i + 1) % offsets.Length]).To3();
                Debug.DrawLine(start, end, new Color(1,1,1,0.35f));

                DrawBiomeEdge(i, 1f, new Color(1, 1, 1, 0.35f), 1);

                if (tileData != null)
                {
                    DrawBiomeEdge(i, 0.75f, GetEdgeColor(tileData.edges[i]), 5);
                    DrawRail(i, railColor ?? GetEdgeColor(tileData.edges[i]), 5);
                }
                if (tileData != null && tileData.IsStation)
                {
                    DrawBiomeEdge(i, 0.1f, Color.yellow, 5);
                }
            }

            return;

            void DrawBiomeEdge(int i, float radius, Color color, int lines)
            {
                if (tileData != null && tileData.edges[i] == EdgeType.Rail)
                {
                    return;
                }

                for (int j = 0; j < lines; j++)
                {
                    Vector3 start = (center + offsets[i] * radius).To3();
                    Vector3 end = (center + offsets[(i + 1) % offsets.Length] * radius).To3();
                    Debug.DrawLine(start, end, color, duration, false);

                    radius -= 0.01f; // make lines appear thicker if drawing more than 1 lines
                }
            }

            void DrawRail(int i, Color color, int lines)
            {
                if (tileData.edges[i] != EdgeType.Rail)
                {
                    return;
                }

                const float step = 0.0025f;
                for (int j = 0; j < lines; j++)
                {
                    Vector3 edgeStart = (center + offsets[i]).To3();
                    Vector3 edgeEnd = (center + offsets[(i + 1) % offsets.Length]).To3();
                    Vector3 lineStart = center.To3();
                    Vector3 lineEnd = (edgeStart + edgeEnd) / 2;
                    Vector3 thicknessOffset = (edgeEnd - edgeStart).normalized * (step * (j - (int)(lines / 2)));
                    Debug.DrawLine(lineStart + thicknessOffset * lines, lineEnd + thicknessOffset * lines, color, duration, false);
                }
            }
        }

        public static void DrawGrid(int centerQ, int centerR, int radius, HexGrid grid)
        {
            using var po = ListPool<Vector2Int>.Get(out var cells);
            grid.Math.GetCellsInRange(new Vector2Int(centerQ, centerR), radius, cells);

            foreach (Vector2Int cell in cells)
            {
                grid.TryGetTile(cell, out HexTileData tile);
                DrawTile(cell, grid.Math, tile);
            }
        }

        public static void DrawHex(Vector2Int cell, Color color, HexGridMath hexGridMath, float duration = 0f, float radius = 1.0f, int thickness = 5)
        {
            Vector2 center = hexGridMath.HexToWorld(cell).To2();
            Vector2[] offsets = hexGridMath.GetPolygonCornersOffsets();

            for (int i = 0; i < offsets.Length; i++)
            {
                DrawEdge(i);
            }

            return;

            void DrawEdge(int i)
            {
                float currentRadius = radius;
                for (int j = 0; j < thickness; j++)
                {
                    Vector3 start = (center + offsets[i] * currentRadius).To3();
                    Vector3 end = (center + offsets[(i + 1) % offsets.Length] * currentRadius).To3();
                    Debug.DrawLine(start, end, color, duration, false);

                    currentRadius -= 0.01f; // make lines appear thicker if drawing more than 1 lines
                }
            }
        }
    }
}
