using NS.Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class HexGridTests
    {
        [Test]
        public void TestEquality()
        {
            HexGrid g1 = new(new HexGridParams() { HexRadius = 1.0f });
            HexGrid g2 = new(new HexGridParams() { HexRadius = 1.0f });
            HexGrid g3 = new(new HexGridParams() { HexRadius = 1.1f });

            Assert.IsTrue(g1.Equals(g2));
            Assert.IsTrue(g1.Equals(g3) == false);

            g1.AddTile(TestUtils.TileA, new Vector2Int(0, 0));

            Assert.IsTrue(g1.Equals(g2) == false);

            g2.AddTile(TestUtils.TileA, new Vector2Int(0, 0));

            Assert.IsTrue(g1.Equals(g2));

            g1.AddTile(TestUtils.TileA, new Vector2Int(0, 1));
            g2.AddTile(TestUtils.TileA, new Vector2Int(0, 2));

            Assert.IsTrue(g1.Equals(g2) == false);

            g1.AddTile(TestUtils.TileA, new Vector2Int(0, 2));
            g2.AddTile(TestUtils.TileA, new Vector2Int(0, 1));

            Assert.IsTrue(g1.Equals(g2));

            g1.AddTile(TestUtils.TileA, new Vector2Int(0, 3));
            g2.AddTile(TestUtils.TileB, new Vector2Int(0, 3));

            Assert.IsTrue(g1.Equals(g2) == false);
        }

        [Test]
        public void TestSerialization()
        {
            HexGrid grid = new(new HexGridParams() {HexRadius = 6.32f});
            grid.AddTile(new HexTileData(TestUtils.FieldsEdges, 15, 3, default), new Vector2Int(5, 5));
            TestUtils.TestSerialization(grid);
        }
    }
}
