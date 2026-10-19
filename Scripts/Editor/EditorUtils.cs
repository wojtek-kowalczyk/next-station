using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NS.Editor
{
    public class EditorUtils
    {
        /// Format buckets of samples for visually representing the probability distribution.
        public static void FormatBuckets<T>(Dictionary<T, int> buckets, int totalIterations, StringBuilder outLines)
        {
            foreach ((T sample, int count) in buckets)
            {
                outLines.AppendLine(FormatLine(sample, (float)count / totalIterations));
            }
        }

        /// <inheritdoc cref="FormatBuckets{T}(Dictionary{T, int}, int, StringBuilder)"/>
        public static void FormatBuckets<T>(Dictionary<T, int> buckets, int totalIterations, List<string> outLines)
        {
            foreach ((T sample, int count) in buckets)
            {
                outLines.Add(FormatLine(sample, (float)count / totalIterations));
            }
        }

        /// <inheritdoc cref="FormatBuckets{T}(Dictionary{T, int}, int, StringBuilder)"/>
        public static void FormatBucketsSorted<T>(Dictionary<T, int> buckets, int totalIterations, List<string> outLines)
        {
            foreach ((T sample, int count) in buckets)
            {
                outLines.Add(FormatLine(sample, (float)count / totalIterations));
            }
        }

        private static string FormatLine<T>(T sample, float share)
        {
            return $"{sample.ToString()}\t-- {share:P}\t{GetVisualRepresentationString(share)}";
        }

        private static string GetVisualRepresentationString(float fillRatio01)
        {
            const float maxWidth = 50;
            int filledCells = Mathf.FloorToInt(fillRatio01 * maxWidth);

            StringBuilder sb = new();
            for (int i = 0; i < filledCells; i++)
            {
                sb.Append('▀');
            }

            return sb.ToString();
        }
    }
}
