using System;
using NS.Core;
using NS.Core.GameModes.Basic;
using NS.Core.Model;
using UnityEngine;

namespace NS.DebugUtils
{
    public class DebugOverlay : MonoBehaviour
    {
        private void Awake()
        {
#if !UNITY_EDITOR
            Destroy(gameObject);
#endif
        }

        private void Update()
        {
            var debugManager = DebugManager.Instance;
            if (debugManager == null || debugManager.State.overlayVisible == false)
            {
                return;
            }

            var game = Game.Instance;

            if (game == null)
            {
                return;
            }

            Vector3 pos = GameplayHelpers.GetMousePositionOnGroundPlane(Camera.main);
            Vector2Int hex = game.Grid.Math.WorldToHex(pos);
            DebugDrawer.DrawGrid(hex.x, hex.y, 10, game.Grid);
            DrawValidPlacementPositions(game);
        }

        private static void DrawValidPlacementPositions(Game game)
        {
            foreach (Vector2Int validPlacementPos in game.GameMode.GetValidPlacementPositionsForCurrentGameState())
            {
                DebugDrawer.DrawHex(validPlacementPos, Color.lightSkyBlue, game.Grid.Math, radius: 0.25f, thickness: 3);
            }
        }

        private void OnGUI()
        {
            // draw stack info
            GUI.skin.label.fontSize = 32;

            // draw hovered tile info
            GUI.skin.label.fontSize = 32;
            if (Game.Instance != null)
            {
                Vector3 worldPos = GameplayHelpers.GetMousePositionOnGroundPlane(Camera.main);
                Vector2Int gridPos = Game.Instance.Grid.Math.WorldToHex(worldPos);

                string hoverText = $"Tile ({gridPos.x}, {gridPos.y})\n";
                if (Game.Instance.Grid.TryGetTile(gridPos, out HexTileData tileData))
                {
                    hoverText += tileData.ToString();
                }

                Vector2 hoverPos = (Input.mousePosition + new Vector3(50, 30, 0));
                hoverPos.y = Screen.height - hoverPos.y;
                GUI.Label(new Rect(hoverPos, new Vector2(5000, 200)), hoverText);
            }

            // draw supply info
            {
                var railNetworks = BasicGameMode.FindRailNetworks(Game.Instance.Model.Grid);
                var supplyInfo = RailNetwork.GetSupplyInfo(railNetworks);
                string text = $"Supply: {supplyInfo.ToString()}";

                var pos = new Rect(new Vector2(0, 0), new Vector2(Screen.width, 50));
                GUIStyle textStyle = GUI.skin.label;
                textStyle.fontSize = 32;
                GUI.Label(pos, text, textStyle);
            }
        }
    }
}
