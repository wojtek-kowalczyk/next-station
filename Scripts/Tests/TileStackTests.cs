using NS.Core;
using NS.Core.Blueprints;
using NUnit.Framework;

namespace Tests
{
    public class TileStackTests
    {
        private static HexTileDataBlueprint dummyBp =>
            new(new[] { new HexTileDataBlueprint.TileTypeWithWeight() { type = TileType.AAABBB, weight = 1 } },
                new[] { EdgeType.Basic, EdgeType.Fields });

        [Test]
        public void TestCreation()
        {
            var stack = new TileStack(10, new[] { dummyBp }, null, 123);
            Assert.AreEqual(10, stack.Count);
        }

        [Test]
        public void TestPeek()
        {
            var stack = new TileStack(10, new[] { dummyBp }, null, 123);
            var next = stack.PeekTile();
            Assert.IsNotNull(next);
            Assert.AreEqual(10, stack.Count);

            var nextTiles = stack.PeekTiles();
            Assert.IsTrue(nextTiles.Length > 0);
            Assert.AreEqual(10, stack.Count);
        }

        [Test]
        public void TestGet()
        {
            var stack = new TileStack(10, new[] { dummyBp }, null, 123);

            for (int i = 0; i < 10; i++)
            {
                var next = stack.PopTile();
                Assert.IsNotNull(next);
                Assert.AreEqual(9 - i, stack.Count);
            }

            var next2 = stack.PopTile();
            Assert.IsNull(next2);
        }

        [Test]
        public void TestGetGetsWhatWasPeeked()
        {
            var stack = new TileStack(10, new[] { dummyBp }, null, 123);
            var peeked = stack.PeekTile();
            var gotten = stack.PopTile();
            Assert.IsNotNull(peeked);
            Assert.IsNotNull(gotten);
            Assert.IsTrue(gotten.Equals(peeked));
        }

        [Test]
        public void EqualTileStacks_AreEqual()
        {
            var blueprints = new[]
            {
                CreateBlueprint()
            };

            var a = new TileStack(
                5,
                blueprints,
                CreateBlueprint(),
                123);

            var b = new TileStack(
                5,
                blueprints,
                CreateBlueprint(),
                123);

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void DifferentCount_AreNotEqual()
        {
            var blueprints = new[]
            {
                CreateBlueprint()
            };

            var a = new TileStack(
                5,
                blueprints,
                CreateBlueprint(),
                123);

            var b = new TileStack(
                6,
                blueprints,
                CreateBlueprint(),
                123);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentBlueprints_AreNotEqual()
        {
            var a = new TileStack(
                5,
                new[]
                {
                    CreateBlueprint()
                },
                CreateBlueprint(),
                123);

            var b = new TileStack(
                5,
                new[]
                {
                    CreateDifferentBlueprint()
                },
                CreateBlueprint(),
                123);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentInitialBlueprint_AreNotEqual()
        {
            var blueprints = new[]
            {
                CreateBlueprint()
            };

            var a = new TileStack(
                5,
                blueprints,
                CreateBlueprint(),
                123);

            var b = new TileStack(
                5,
                blueprints,
                CreateDifferentBlueprint(),
                123);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentSeed_AreNotEqual()
        {
            var blueprints = new[]
            {
                CreateBlueprint()
            };

            var a = new TileStack(
                5,
                blueprints,
                CreateBlueprint(),
                123);

            var b = new TileStack(
                5,
                blueprints,
                CreateBlueprint(),
                456);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void SameReference_IsEqual()
        {
            var stack = new TileStack(
                5,
                new[]
                {
                    CreateBlueprint()
                },
                CreateBlueprint(),
                123);

            Assert.IsTrue(stack.Equals(stack));
        }

        [Test]
        public void Null_IsNotEqual()
        {
            var stack = new TileStack(
                5,
                new[]
                {
                    CreateBlueprint()
                },
                CreateBlueprint(),
                123);

            Assert.IsFalse(stack.Equals(null));
        }

        [Test]
        public void TestSerialization()
        {
            TestUtils.TestSerialization(new TileStack(10, new[] { dummyBp }, null, 123));
            TestUtils.TestSerialization(new TileStack(10, new[] { CreateBlueprint() }, null, 123));
            TestUtils.TestSerialization(new TileStack(10, new[] { CreateBlueprint() }, CreateBlueprint(), 123));
            TestUtils.TestSerialization(new TileStack(10, new[] { CreateDifferentBlueprint(), CreateBlueprint() },
                CreateBlueprint(), 123));
            TestUtils.TestSerialization(new TileStack(10, new[] { CreateBlueprint() }, CreateDifferentBlueprint(),
                123));
        }

        private static HexTileDataBlueprint CreateBlueprint()
        {
            return new HexTileDataBlueprint(
                new[]
                {
                    new HexTileDataBlueprint.TileTypeWithWeight
                    {
                        type = TileType.AABBCC,
                        weight = 1.0f
                    }
                },
                new[]
                {
                    EdgeType.Basic,
                    EdgeType.Rail,
                    EdgeType.Mountain
                });
        }

        private static HexTileDataBlueprint CreateDifferentBlueprint()
        {
            return new HexTileDataBlueprint(
                new[]
                {
                    new HexTileDataBlueprint.TileTypeWithWeight
                    {
                        type = TileType.AAABBB,
                        weight = 1.0f
                    }
                },
                new[]
                {
                    EdgeType.Rail,
                    EdgeType.Town
                });
        }
    }
}
