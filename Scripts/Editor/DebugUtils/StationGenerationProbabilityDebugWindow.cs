using System;
using System.Collections.Generic;
using NS.Utility;
using UnityEditor;
using UnityEngine;
using Random = NS.Core.Random;

namespace NS.Editor.DebugUtils
{
    public class StationGenerationProbabilityDebugWindow : EditorWindow
    {
        private ScriptableCurve curve;
        private int iterations = 10_00;

        private List<string> lastResultStrings;
        private Vector2 scrollPosition;

        [MenuItem("NS/Station generation probability debug window")]
        public static void ShowExample()
        {
            var window = GetWindow<StationGenerationProbabilityDebugWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            curve = (ScriptableCurve)EditorGUILayout.ObjectField(curve, typeof(ScriptableCurve), false);
            iterations = EditorGUILayout.IntField(iterations);

            if (GUILayout.Button("Run Test"))
            {
                RunTest();
            }

            if (lastResultStrings == null)
            {
                GUILayout.Label("No results yet");
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (string lastResultString in lastResultStrings)
            {
                GUILayout.Label(lastResultString);
            }
            EditorGUILayout.EndScrollView();
        }

        private struct Sample : IEquatable<Sample>
        {
            public int moves; // how many moves it took to place a station
            public bool Equals(Sample other) => moves == other.moves;
            public override bool Equals(object obj) => obj is Sample other && Equals(other);
            public override int GetHashCode() => moves;
            public override string ToString() => $"{nameof(moves)}: {moves}";
        }

        private void RunTest()
        {
            // var rand = new Random(15234);
            var rand = new Random((int)DateTime.Now.Ticks);

            var samples = new List<Sample>();

            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    int tilesSinceLastStation = j + 1;

                    float roll = rand.NextFloat();
                    float prob = curve.Curve.Evaluate(tilesSinceLastStation);
                    if (roll < prob)
                    {
                        samples.Add(new Sample() { moves = tilesSinceLastStation });
                        break;
                    }
                }
            }

            samples.Sort((x, y) => x.moves.CompareTo(y.moves));

            Dictionary<Sample, int> buckets = new();
            foreach (Sample sample in samples)
            {
                buckets.TryAdd(sample, 0);
                buckets[sample] += 1;
            }

            // 15% of time station was generated on move X... etc.
            lastResultStrings ??= new();
            lastResultStrings.Clear();
            lastResultStrings.Add("Test results:");
            EditorUtils.FormatBuckets(buckets, iterations, lastResultStrings);
        }
    }
}
