using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = NS.Core.Random;

namespace NS.Editor.DebugUtils
{
    public class RandomTestsWindow : EditorWindow
    {
        private int minIntIncl = 0;
        private int maxIntExcl = 10;

        private string lastResultString;

        [MenuItem("NS/Random tests")]
        public static void ShowExample()
        {
            var window = GetWindow<RandomTestsWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            minIntIncl = EditorGUILayout.IntField("Min int (inclusive)", minIntIncl);
            maxIntExcl = EditorGUILayout.IntField("Max int (exclusive)", maxIntExcl);

            if (GUILayout.Button("Run Test"))
            {
                RunTest();
            }

            if (lastResultString == null)
            {
                GUILayout.Label("No results yet");
                return;
            }

            GUILayout.Label(lastResultString);
        }

        private void RunTest()
        {
            Random rand = new();

            const int totalSamplesPerBucket = 100_000;

            Dictionary<int /*sample*/, int /*count*/> intSamples = new();
            for (int i = 0; i < totalSamplesPerBucket; i++)
            {
                int sample = rand.NextInt(minIntIncl, maxIntExcl);
                intSamples.TryAdd(sample, 0);
                intSamples[sample] += 1;
            }

            Dictionary<uint /*sample*/, int /*count*/> uintSamples = new();
            for (int i = 0; i < totalSamplesPerBucket; i++)
            {
                uint sample = rand.NextUint((uint)minIntIncl, (uint)maxIntExcl);
                uintSamples.TryAdd(sample, 0);
                uintSamples[sample] += 1;
            }

            StringBuilder sb = new();
            sb.AppendLine("Results:");
            sb.AppendLine();
            AppendSamples(intSamples);
            AppendSamples(uintSamples);
            lastResultString = sb.ToString();

            return;

            void AppendSamples<T>(Dictionary<T, int> samples)
            {
                sb.AppendLine($"{typeof(T).Name}");
                foreach ((T sample, int amount) in samples.OrderBy(x => x.Key))
                    sb.AppendLine($"{sample}\t-- {amount}\t({(float)amount/totalSamplesPerBucket:P})");
            }
        }
    }
}
