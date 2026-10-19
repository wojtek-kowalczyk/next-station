using NS.Core;
using NUnit.Framework;

namespace Tests
{
    public class HexTileDataTests
    {
        [Test]
        public void TestEquality()
        {
            var tile1 = new HexTileData(TestUtils.BasicEdges, 1, 0, TestUtils.StationGivesWood);
            var tile2 = new HexTileData(TestUtils.BasicEdges, 2, 0, TestUtils.StationGivesWood);
            var tile3 = new HexTileData(TestUtils.FieldsEdges, 2, 0, TestUtils.StationGivesWood);
            var tile4 = new HexTileData(TestUtils.FieldsEdges, 2, 0, TestUtils.StationGivesSteel);
            var tile5 = new HexTileData(TestUtils.FieldsEdges, 2, 0, TestUtils.StationWantsSteel);

            Assert.IsTrue(tile1.Equals(tile2));
            Assert.IsTrue(tile2.Equals(tile1));
            Assert.IsTrue(tile1.Equals(null) == false);
            Assert.IsTrue(tile1.Equals(tile3) == false);
            Assert.IsTrue(tile3.Equals(tile4) == false);
            Assert.IsTrue(tile3.Equals(tile5) == false);
            Assert.IsTrue(tile4.Equals(tile5) == false);

            var tile1NoStation = new HexTileData(TestUtils.BasicEdges, 1, 0, default);

            Assert.IsTrue(tile1.Equals(tile1NoStation) == false);
            Assert.IsTrue(tile1NoStation.Equals(tile1) == false);
        }

        [Test]
        public void TestTileRotation()
        {
            var edges = new[]
            {
                EdgeType.Basic, EdgeType.Town, EdgeType.Mountain, EdgeType.Fields, EdgeType.Mountain, EdgeType.Rail
            };
            var tile = new HexTileData(edges, 123, 0, default);

            HexTileData r1 = tile.Rotated(1);

            Assert.IsTrue(r1.decorationsSeed == tile.decorationsSeed);
            Assert.IsTrue(r1.IsStation == tile.IsStation);

            Assert.IsTrue(r1.Edge0 == EdgeType.Rail && r1.Edge1 == EdgeType.Basic && r1.Edge2 == EdgeType.Town &&
                          r1.Edge3 == EdgeType.Mountain && r1.Edge4 == EdgeType.Fields && r1.Edge5 == EdgeType.Mountain);
            HexTileData r4 = tile.Rotated(4);
            Assert.IsTrue(r4.Edge0 == EdgeType.Mountain && r4.Edge1 == EdgeType.Fields && r4.Edge2 == EdgeType.Mountain &&
                          r4.Edge3 == EdgeType.Rail && r4.Edge4 == EdgeType.Basic && r4.Edge5 == EdgeType.Town);
            Assert.IsTrue(tile.Equals(tile.Rotated(0)));
            Assert.IsTrue(tile.Equals(tile.Rotated(6)));
            Assert.IsTrue(tile.Equals(tile.Rotated(36)));
            Assert.IsTrue(tile.Equals(tile.Rotated(-6)));
            Assert.IsTrue(tile.Equals(tile.Rotated(-36)));
        }

        [Test]
        public void TestUnrotateTile()
        {
            var edges = new[]
            {
                EdgeType.Basic, EdgeType.Town, EdgeType.Mountain, EdgeType.Fields, EdgeType.Mountain, EdgeType.Rail
            };
            var tile = new HexTileData(edges, 123, 0, default);

            var r1 = tile.Rotated(2).Rotated(-2);
            Assert.IsTrue(tile.Equals(r1));
            Assert.IsTrue(tile.IsStation == r1.IsStation);

            var r2 = tile.Rotated(6).Rotated(-6);
            Assert.IsTrue(tile.Equals(r2));

            var r3 = tile.Rotated(-4).Rotated(4);
            Assert.IsTrue(tile.Equals(r3));
        }

        [Test]
        public void TestSerialization()
        {
            TestUtils.TestSerialization(new HexTileData());

            TestUtils.TestSerialization(new HexTileData(TestUtils.BasicEdges, 15, 0, default));

            TestUtils.TestSerialization(new HexTileData(TestUtils.FieldsEdges, 15, 0, TestUtils.StationWantsWoodGivesSteel));
        }

        [Test]
        public void TestSeedAndPlacementRotationNotAffectingEquality()
        {
            var tileA = new HexTileData(TestUtils.FieldsEdges, 0, 0, default);
            var tileB = new HexTileData(TestUtils.FieldsEdges, 0, 0, default);
            var tileC = new HexTileData(TestUtils.FieldsEdges, 1, 0, default);
            var tileD = new HexTileData(TestUtils.FieldsEdges, 0, 1, default);
            var tileE = new HexTileData(TestUtils.FieldsEdges, 1, 1, default);

            Assert.IsTrue(tileA.Equals(tileB));
            Assert.IsTrue(tileA.Equals(tileC));
            Assert.IsTrue(tileA.Equals(tileD));
            Assert.IsTrue(tileA.Equals(tileE));
        }
    }
}
