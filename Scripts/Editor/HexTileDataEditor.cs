using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS.Core.Blueprints;
using UnityEditor;
using UnityEngine;
using Random = NS.Core.Random;

namespace NS.Editor
{
    [CustomEditor(typeof(HexTileDataBlueprintSO))]
    public class HexTileDataBlueprintSOEditor : UnityEditor.Editor
    {
        private string outputMsg = string.Empty;

        public override void OnInspectorGUI()
        {
            string msg = "edges start at right edge and go clockwise\n\n" +
                         "edge0 = right edge (E)\n" +
                         "edge1 = bottom-right edge (SE)\n" +
                         "edge2 = bottom-left edge (SW)\n" +
                         "edge3 = left edge (W)\n" +
                         "edge4 = top-left edge (NW)\n" +
                         "edge5 = top-right edge (NE)";

            EditorGUILayout.HelpBox(msg, MessageType.Info);

            base.OnInspectorGUI();

            GUILayout.Space(25);
            if (GUILayout.Button("test run"))
            {
                GenerateManyStationsAndPrintOutputStats();
            }
            GUILayout.TextField(outputMsg);
        }

        private void GenerateManyStationsAndPrintOutputStats()
        {
            if (serializedObject.targetObject is not HexTileDataBlueprintSO bpSO)
            {
                outputMsg = "Please select a blueprint";
                return;
            }

            const int numIterations = 10_000;

            HexTileDataBlueprint bp = bpSO.Blueprint;
            Dictionary<TileType, int> buckets = new();
            var rand = new Random();
            var debugInfo = new HexTileDataBlueprint.GenerationDebugInfo();
            for (int i = 0; i < numIterations; i++)
            {
                _ = bp.Generate(rand, debugInfo: debugInfo);
                buckets.TryAdd(debugInfo.tileType, 0);
                buckets[debugInfo.tileType] += 1;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Test run results ({numIterations} generations):");
            var sortedBuckets = buckets
                .OrderBy(kvp => kvp.Key.ToString())
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            EditorUtils.FormatBuckets(sortedBuckets, numIterations, sb);

            int tilesWithRails = sortedBuckets
                .Where(x => x.Key.ToString().StartsWith("T"))
                .Sum(x => x.Value);
            sb.AppendLine();
            sb.AppendLine($"rail percentage: {(float)tilesWithRails/numIterations:P}");

            outputMsg = sb.ToString();
        }
    }
}
