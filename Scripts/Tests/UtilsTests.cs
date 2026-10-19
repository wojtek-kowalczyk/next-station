using System;
using NS.Utility;
using NUnit.Framework;
using Random = NS.Core.Random;

namespace Tests
{
    public class UtilsTests
    {
        [Test]
        public void SetCountSetBits()
        {
            for (int i = 0; i < 32; i++)
            {
                uint bits = (uint)1 << i;
                Assert.IsTrue(Utils.NumSetBits(bits) == 1);
            }

            var rand = new Random(2137);
            for (int i = 0; i < 1000; i++)
            {
                uint bits = rand.NextUint(0, 256);
                int count = Utils.NumSetBits(bits);
                int manualCount = 0;
                for (int b = 0; b < 8; b++)
                {
                    if ((bits & ((uint)1 << b)) != 0)
                    {
                        manualCount++;
                    }
                }

                Assert.IsTrue(count == manualCount, $"Failed for {Convert.ToString(bits, toBase: 2).PadLeft(8, '0')} ({bits})");
            }
        }

        [Test]
        public void TestGetRandomBit()
        {
            int bitmask = 0b_1101_0110;
            int bitCount = 8;
            var rand = new Random(2137);
            for (int i = 0; i < 1000; i++)
            {
                int selectedBit = Utils.SelectRandomBitFrom(bitmask, bitCount, rand);
                Assert.IsTrue((selectedBit & bitmask) != 0,
                    $"Selected bit {Convert.ToString(selectedBit, toBase: 2).PadLeft(8, '0')} " +
                    $"not in bitmask {Convert.ToString(bitmask, toBase: 2).PadLeft(8, '0')}");
                Assert.IsTrue(selectedBit < 0b0001_0000_0000); // make sure we didnt select outisde of range
            }

            for (int i = 0; i < 1000; i++)
            {
                int randomBit = Utils.SelectRandomBitFrom(bitmask, 2, rand);
                Assert.IsTrue(randomBit < 0b11); // make sure we didnt select outisde of range
            }
        }

        [Test]
        public void TestGetRandomBitShortBitmask()
        {
            int bitmask = 0b0001_1100;
            int bitCount = 32;
            var rand = new Random(2137);
            for (int i = 0; i < 1000; i++)
            {
                int selectedBit = Utils.SelectRandomBitFrom(bitmask, bitCount, rand);
                Assert.IsTrue((selectedBit & bitmask) != 0,
                    $"Selected bit {Convert.ToString(selectedBit, toBase: 2).PadLeft(8, '0')} " +
                    $"not in bitmask {Convert.ToString(bitmask, toBase: 2).PadLeft(8, '0')}");
                Assert.IsTrue(selectedBit < 0b0010_0000); // make sure we didnt select outisde of range
            }

            for (int i = 0; i < 1000; i++)
            {
                int randomBit = Utils.SelectRandomBitFrom(bitmask, 3, rand);
                Assert.IsTrue(randomBit < 0b111); // make sure we didnt select outisde of range
                Assert.IsTrue(randomBit == 0b100); // given this bitmask this is the only possible outcome
            }
        }

        [Test]
        public void WeightedSamplingTests()
        {
            float[] arr = { 1f, 3f, 3f, 3f, 0f, 15f, 0f, 0f };
            var rand = new Random(1234);

            for (int i = 0; i < 10_000; i++)
            {
                int index = Utils.WeightedSample(arr, rand);
                Assert.IsTrue(index >= 0 && index < arr.Length, "index out of bounds");
                Assert.IsTrue(index != 4, "should never sample zero");
            }
        }
    }
}
