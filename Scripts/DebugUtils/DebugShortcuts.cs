using System.Collections.Generic;
using MessagePack;
using NS.Core.GameModes.Basic;
using NS.Visuals;
using UnityEngine;
using Random = NS.Core.Random;

namespace NS.DebugUtils
{
    public class DebugShortcuts : MonoBehaviour
    {
        private bool showHelpText;
        private DebugAstarTestingMode debugAstarTesting = new();

        private Dictionary<string, string> noHelpTextLines = new()
        {
            { "F1", "show available debug shortcuts" },
        };

        private Dictionary<string, string> helpTextLines = new()
        {
            { "F1", "hide this text" },
            { "F2", "toggle debug overlay" },
            { "F3", "toggle tile decorations" },
            { "F4", "visualise last station search" },
            { "F5", "toggle visualization of station connections" },
            { "F6", "toggle visualization last of station generation" },
            { "F7", "add a small amount of tiles" },
            { "F8", "give station to place" },
            { "F9", "test station placement candidate cells under cursor" },
            { "F10", "serialize and print GameModel" },
            { "F11", "toggle astar testing mode" },
            { "C", "enable tile placement cheats" },
        };

        private void Update()
        {
            if (DebugManager.Instance == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                showHelpText = !showHelpText;
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                debugAstarTesting.Toggle();
            }

            if (debugAstarTesting.IsEnabled)
            {
                debugAstarTesting.Update();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleDebugOverlay();
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ToggleDecorations();
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                VisualizeLastSearch();
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ToggleVisualizationOfStationConnections();
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                ToggleVisualizationOfStationGeneration();
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
                Game.Instance.TileStack.AddTiles(3);
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Game.Instance.GameMode.StationSpawner.AwardStation(Game.Instance.Model, null, new Random());
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                GenerateCandidateCellsUnderCursor();
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                byte[] bytes = MessagePackSerializer.Serialize(Game.Instance.Model);
                string json = MessagePackSerializer.ConvertToJson(bytes);
                Debug.Log(json);
            }
        }

        private static void GenerateCandidateCellsUnderCursor()
        {
            var stationSpawner = FindAnyObjectByType<StationSpawner>();
            if (stationSpawner == null)
            {
                Debug.LogError("Failed to find station spawner");
                return;
            }

            // generation raises an event which is handled by a debug utility that draws these cells
            var hoveredCell = GameplayHelpers.GetHoveredCell(Game.Instance.Grid, Camera.main);
            stationSpawner.GenerateCandidateCells(Game.Instance.Grid, hoveredCell);
        }

        private void ToggleVisualizationOfStationGeneration()
        {
            var visualization = FindAnyObjectByType<DebugStationGenerationVisualization>();
            if (visualization == null)
            {
                Debug.LogError($"cannot find {nameof(DebugStationGenerationVisualization)} in scene");
                return;
            }

            visualization.ToggleVisualizationOfLastGeneration();
        }

        private void ToggleVisualizationOfStationConnections()
        {
            var visualization = FindAnyObjectByType<DebugStationConnectionsVisualization>();
            if (visualization == null)
            {
                Debug.LogError($"cannot find {nameof(DebugStationConnectionsVisualization)} in scene");
                return;
            }

            visualization.enabled = !visualization.enabled;
        }

        private static void VisualizeLastSearch()
        {
            var visualization = FindAnyObjectByType<DebugRailGraphsVisualization>();
            if (visualization == null)
            {
                Debug.LogError($"cannot find {nameof(DebugRailGraphsVisualization)} in scene");
                return;
            }

            visualization.TriggerVisualizationOfLastSearch();
        }

        private static void ToggleDecorations()
        {
            var debugManager = DebugManager.Instance;
            debugManager.State.decorationsVisible = debugManager.State.decorationsVisible == false;

            var allVisuals = FindObjectsByType<HexTileVisuals>(FindObjectsSortMode.None);
            foreach (HexTileVisuals visuals in allVisuals)
            {
                if (visuals.gameObject.layer == LayerMask.NameToLayer("HexStack"))
                {
                    continue;
                }

                visuals.SetDecorationsVisible(debugManager.State.decorationsVisible);
            }
        }

        private static void ToggleDebugOverlay()
        {
            var debugManager = DebugManager.Instance;
            debugManager.State.overlayVisible = debugManager.State.overlayVisible == false;
        }

        private void OnGUI()
        {
            DrawLines(showHelpText? helpTextLines : noHelpTextLines);

            if (debugAstarTesting.IsEnabled)
            {
                var pos = new Rect(new Vector2(600, 600), new Vector2(400, 100));
                GUIStyle textStyle = GUI.skin.label;
                textStyle.fontSize = 32;
                GUI.Label(pos, "AStar testing enabled!", textStyle);
            }

            return;

            static void DrawLines(Dictionary<string, string> keysWithDescriptions)
            {
                GUIStyle textStyle = GUI.skin.label;
                textStyle.fontSize = 32;

                int lines = keysWithDescriptions.Count;
                float lineHeight = textStyle.lineHeight;

                float sw = Screen.width;
                float sh = Screen.height;
                float w = sw;
                float h = lineHeight * lines;

                var pos = new Rect(new Vector2(0, sh - h), new Vector2(w, h));
                foreach ((string key, string desc) in keysWithDescriptions)
                {
                    GUI.Label(pos, $"Press {key} to {desc}", textStyle);
                    pos = new Rect(pos.x, pos.y + lineHeight, pos.width, pos.height);
                }
            }
        }
    }
}
