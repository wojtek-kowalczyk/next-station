using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;
using UnityEngine.Assertions;
using Random = NS.Core.Random;


namespace NS.Utility
{
    public static class Utils
    {
        public static int NumSetBits(uint i)
        {
            // source: https://stackoverflow.com/questions/109023/count-the-number-of-set-bits-in-a-32-bit-integer
            i = i - ((i >> 1) & 0x55555555); // add pairs of bits
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333); // quads
            i = (i + (i >> 4)) & 0x0F0F0F0F; // groups of 8
            i *= 0x01010101; // horizontal sum of bytes
            return (int) i >> 24; // return just that top byte (after truncating to 32-bit even when int is wider than uint32_t)
        }

        public static int SelectRandomBitFrom(int bitmask, int bitCount, Random rand)
        {
            Assert.IsTrue(bitmask != 0, "No bits set in bitmask");

            // set only those bits that are in the original bitmask and their bit index is less than bitCount.
            // This is useful if you have enum flags with All = ~0 member, and only a few actual bits used.
            int actualBitmask = 0;
            for (int i = 0; i < bitCount; i++)
            {
                actualBitmask |= (bitmask & (1 << i));
            }

            Assert.IsTrue(actualBitmask != 0, "No bits set after masking with bitCount");

            int numSetBits = NumSetBits((uint)actualBitmask);
            // e.g., value 3 means 3rd set bit
            int bitNumberToSelect = rand.NextInt(1, Mathf.Min(numSetBits, bitCount) + 1); // min incl, max excl
            int selectedBit = -1;
            for (int i = 0; i < bitCount; i++)
            {
                int bit = 1 << i;
                if ((actualBitmask & bit) != 0)
                {
                    bitNumberToSelect--;
                    if (bitNumberToSelect <= 0)
                    {
                        selectedBit = bit;
                        break;
                    }
                }
            }

            Assert.IsTrue(selectedBit != -1);

            return selectedBit;
        }

        /// <returns>sampled index in weights array.</returns>
        public static int WeightedSample(IReadOnlyList<float> weights, Random rand)
        {
            Assert.IsTrue(weights.Count > 0);
            Assert.IsTrue(weights.All(w => w >= 0));
            Assert.IsTrue(Mathf.Approximately(weights.Sum(), 0) == false);

            float totalWeight = weights.Sum();
            float r = rand.NextFloat() * totalWeight;
            for (int i = 0; i < weights.Count; i++)
            {
                float weight = weights[i];

                r -= weight;
                if (r <= 0f)
                {
                    return i;
                }
            }

            Assert.IsTrue(false, "unreachable");
            return -1;
        }

        public static bool DoListsContainEqualElements<T>(IReadOnlyList<T> first, IReadOnlyList<T> second)
        {
            // We can't simply compare at indices because lists may be in different orders, hence O(n^2)

            var tempList = new List<T>(second);

            foreach (T item in first)
            {
                int index = tempList.IndexOf(item);
                if (index != -1)
                {
                    tempList.RemoveAt(index);
                }
            }

            return tempList.Count <= 0;
        }

        public static string ToBase64<T>(T obj)
        {
            return Convert.ToBase64String(MessagePackSerializer.Serialize(obj));
        }
        
        public static T FromBase64<T>(string base64)
        {
            return MessagePackSerializer.Deserialize<T>(Convert.FromBase64String(base64));
        }

        public static void ReportCriticalFailure(string info, bool quitApplication)
        {
            string userProfileDir = Environment.GetEnvironmentVariable("USERPROFILE");
            string logFileLocation = $@"{userProfileDir}\AppData\LocalLow\" +
                                     $@"{Application.companyName}\{Application.productName}\Player.log";

            MessageBoxHelper.Show("OK", "Yikes!",
                $"{info}. Please send the log file to the developers.\n\n" +
                "When you click \"OK\" the log file location will open. Please send the \"Player.log\" file.\n\n" +
                "We apologize for the inconvenience and thank you for submitting the report.\n\n" +
                $"If the log file location didn't open, navigate to:\n{logFileLocation}");

            if (File.Exists(logFileLocation))
            {
                Process.Start("explorer.exe", "/select, " + logFileLocation);
            }

            if (quitApplication)
            {
                Application.Quit();
            }
        }
    }
}
