using System;
using MessagePack;
using NUnit.Framework;
using Random = NS.Core.Random;

namespace Tests
{
    public class RandomTests
    {
        private static void TestLimitsImpl(int seed)
        {
            Random random = new(seed);

            const int numIterations = 10_000;

            for (int i = 0; i < numIterations; i++)
            {
                uint value = random.NextUint(10, 20);
                Assert.IsTrue(value >= 10 && value < 20, $"value {value} out of range for {i} (seed={seed})");
            }

            for (int i = 0; i < numIterations; i++)
            {
                float value = random.NextFloat();
                Assert.IsTrue(value >= 0 && value < 1, $"value {value} out of range for {i} (seed={seed})");
            }

            for (int i = 0; i < numIterations; i++)
            {
                float value = random.NextFloat(5.0f, 5.5f);
                // Note: upper limit is inclusive
                Assert.IsTrue(value >= 5.0f && value <= 5.5f, $"value {value} out of range for {i} (seed={seed})");
            }

            for (int i = 0; i < numIterations; i++)
            {
                float value = random.NextInt(5, 9);
                Assert.IsTrue(value >= 5 && value < 9, $"value {value} out of range for {i} (seed={seed})");
            }
        }

        [Test]
        public void TestLimits()
        {
            int seed = (int)DateTime.Now.Ticks;
            TestLimitsImpl(seed);
        }

        [Test]
        public void TestEquality()
        {
            Random random1 = new(123);
            Random random2 = new(123);
            Random random3 = new(0);

            Assert.IsTrue(random1.Equals(random2));
            Assert.IsTrue(random1.Equals(random3) == false);

            random1.NextUint();
            Assert.IsTrue(random1.Equals(random2) == false);
            random2.NextUint();
            Assert.IsTrue(random1.Equals(random2));

            random1.NextUint(10, 50);
            Assert.IsTrue(random1.Equals(random2) == false);
            random2.NextUint(60, 80);
            Assert.IsTrue(random1.Equals(random2));

            random1.NextFloat();
            Assert.IsTrue(random1.Equals(random2) == false);
            random2.NextFloat();
            Assert.IsTrue(random1.Equals(random2));

            random1.NextFloat(10, 50);
            Assert.IsTrue(random1.Equals(random2) == false);
            random2.NextFloat(60, 80);
            Assert.IsTrue(random1.Equals(random2));
        }

        [Test]
        public void TestSerialization()
        {
            Random random1 = new(123);
            TestUtils.TestSerialization(random1);
        }

        [Test]
        public void TestSerializationSequence()
        {
            Random random1 = new(123);
            random1.NextUint();
            byte[] snapshot = MessagePackSerializer.Serialize(random1);
            random1.NextUint();
            random1.NextUint();
            random1.NextUint();

            {
                Random random2 = new(123);
                random2.NextUint();

                var deserialized = MessagePackSerializer.Deserialize<Random>(snapshot);

                for (int i = 0; i < 100; i++)
                {
                    Assert.IsTrue(random2.NextUint() == deserialized.NextUint());
                }
            }

            {
                var deserialized = MessagePackSerializer.Deserialize<Random>(snapshot);
                deserialized.NextUint();
                deserialized.NextUint();
                deserialized.NextUint();
                for (int i = 0; i < 100; i++)
                {
                    Assert.IsTrue(random1.NextUint() == deserialized.NextUint());
                }
            }
        }
    }
}
