using System.Collections.Generic;
using NS.Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class RailNetworkTests
    {
        private readonly EdgeType[] edges = { EdgeType.Basic, EdgeType.Basic, EdgeType.Basic, EdgeType.Basic, EdgeType.Basic, EdgeType.Basic };
        private HexTileData NonStationTile => Tile(default);
        private HexTileData StationTile => Tile(new StationData(ResourceType.Wood, 1, ResourceType.None, 0));
        private HexTileData Tile(StationData stationData) => new(edges, 0, 0, stationData);

        [Test]
        public void TestRailNetworkMustContainStations()
        {
            HexGrid grid = new(HexGridParams.Default);

            grid.AddTile(NonStationTile, new Vector2Int(0, 0));
            grid.AddTile(StationTile, new Vector2Int(0, 1));

            // TODO : this probably should throw meaningful exceptions if we're testing it...
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                RailNetwork _ = new(new List<Vector2Int>()
                {
                    new(0, 0),
                }, grid);
            });

            Assert.DoesNotThrow(() =>
            {
                RailNetwork _ = new(new List<Vector2Int>()
                {
                    new(0, 1),
                }, grid);
            });
        }

        [Test]
        public void TestNetworkEquality()
        {
            HexGrid grid = new(HexGridParams.Default);

            grid.AddTile(StationTile, new Vector2Int(0, 0));
            grid.AddTile(StationTile, new Vector2Int(0, 1));
            grid.AddTile(StationTile, new Vector2Int(0, 2));

            RailNetwork a = new(new List<Vector2Int>()
            {
                new(0, 0),
                new(0, 1),
            }, grid);

            RailNetwork b = new(new List<Vector2Int>()
            {
                new(0, 0),
                new(0, 2),
            }, grid);

            RailNetwork c = new(new List<Vector2Int>()
            {
                new(0, 2),
                new(0, 0),
            }, grid);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(c.Equals(b));
        }

        [Test]
        public void TestSupplyInfo()
        {
            var grid = new HexGrid(HexGridParams.Default);

            {
                var info = RailNetwork.GetSupplyInfo(null);
                AssertGivesNothing(info);
            }

            {
                var info = RailNetwork.GetSupplyInfo(new List<RailNetwork>());
                AssertGivesNothing(info);
            }

            grid.AddTile(Tile(new StationData().WithWants(ResourceType.Wood, 1)), new Vector2Int(0, 1));
            grid.AddTile(Tile(new StationData().WithWants(ResourceType.People, 1)), new Vector2Int(0, 2));
            grid.AddTile(Tile(new StationData().WithGives(ResourceType.Steel, 2)), new Vector2Int(1, 0));
            grid.AddTile(Tile(new StationData().WithGives(ResourceType.Wood, 1)), new Vector2Int(2, 0));

            {
                RailNetwork network = new( new List<Vector2Int>() { new(0, 1), new(0, 2)}, grid);
                var info = RailNetwork.GetSupplyInfo(new[] { network });
                AssertGivesNothing(info);
            }

            {
                RailNetwork network = new( new List<Vector2Int>() { new(1, 0), new(2, 0)}, grid);
                var info = RailNetwork.GetSupplyInfo(new[] { network });
                Assert.IsTrue(info.Gives(ResourceType.People) == 0);
                Assert.IsTrue(info.Gives(ResourceType.Wood) == 1);
                Assert.IsTrue(info.Gives(ResourceType.Steel) == 2);
                Assert.IsTrue(info.Gives(ResourceType.Gold) == 0);
                Assert.IsTrue(info.GivesProb01(ResourceType.People) == 0);
                Assert.IsTrue(Mathf.Approximately(info.GivesProb01(ResourceType.Wood), 1/3f));
                Assert.IsTrue(Mathf.Approximately(info.GivesProb01(ResourceType.Steel), 2/3f));
                Assert.IsTrue(info.GivesProb01(ResourceType.Gold) == 0);
            }

            {
                RailNetwork network = new( new List<Vector2Int>() { new(1, 0), new(2, 0), new(0, 1)}, grid);
                var info = RailNetwork.GetSupplyInfo(new[] { network });
                Assert.IsTrue(info.Gives(ResourceType.People) == 0);
                Assert.IsTrue(info.Gives(ResourceType.Wood) == 0); // note: set and wants cancel out
                Assert.IsTrue(info.Gives(ResourceType.Steel) == 2);
                Assert.IsTrue(info.Gives(ResourceType.Gold) == 0);
                Assert.IsTrue(info.GivesProb01(ResourceType.People) == 0);
                Assert.IsTrue(Mathf.Approximately(info.GivesProb01(ResourceType.Wood), 0));
                Assert.IsTrue(Mathf.Approximately(info.GivesProb01(ResourceType.Steel), 1f));
                Assert.IsTrue(info.GivesProb01(ResourceType.Gold) == 0);
            }

            return;

            void AssertGivesNothing(RailNetwork.SupplyInfo info)
            {
                Assert.IsTrue(info.Gives(ResourceType.People) == 0);
                Assert.IsTrue(info.Gives(ResourceType.Wood) == 0);
                Assert.IsTrue(info.Gives(ResourceType.Steel) == 0);
                Assert.IsTrue(info.Gives(ResourceType.Gold) == 0);
                Assert.IsTrue(info.GivesProb01(ResourceType.People) == 0);
                Assert.IsTrue(info.GivesProb01(ResourceType.Wood) == 0);
                Assert.IsTrue(info.GivesProb01(ResourceType.Steel) == 0);
                Assert.IsTrue(info.GivesProb01(ResourceType.Gold) == 0);
            }
        }

        [Test]
        public void TestBalancingDoesNotChangeSupply()
        {
            var grid = new HexGrid(HexGridParams.Default);
            grid.AddTile(Tile(new StationData().WithWants(ResourceType.Wood, 1)), new Vector2Int(0, 1));
            grid.AddTile(Tile(new StationData().WithGives(ResourceType.Wood, 2)), new Vector2Int(0, 2));

            RailNetwork network = new( new List<Vector2Int>() { new(0, 1), new(0, 2)}, grid);
            var info = RailNetwork.GetSupplyInfo(new[] { network });
            Assert.IsTrue(info.Gives(ResourceType.Wood) == 1);
            network.Balance();
            Assert.IsTrue(info.Gives(ResourceType.Wood) == 1, "Balancing shouldn't change global supply");
        }
    }
}
