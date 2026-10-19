using NS.Core;
using NS.Core.Blueprints;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Tests
{
    public class HexTileDataBlueprintTests
    {
        [Test]
        public void EqualBlueprints_AreEqual()
        {
            var tileTypes = new[]
            {
                new HexTileDataBlueprint.TileTypeWithWeight
                {
                    type = TileType.AABBCC,
                    weight = 1.0f
                }
            };

            var edgeTypes = new[]
            {
                EdgeType.Fields,
                EdgeType.Mountain,
                EdgeType.Town,
            };

            var station = new StationDataBlueprint(
                ResourceType.Wood,
                2,
                ResourceType.Steel,
                3);

            var a = new HexTileDataBlueprint(tileTypes, edgeTypes, station);
            var b = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                },
                new StationDataBlueprint(
                    ResourceType.Wood,
                    2,
                    ResourceType.Steel,
                    3));

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void DifferentTileType_AreNotEqual()
        {
            var a = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            var b = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                });

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentTileTypeWeight_AreNotEqual()
        {
            var a = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            var b = new HexTileDataBlueprint(
                new[]
                {
                    new HexTileDataBlueprint.TileTypeWithWeight
                    {
                        type = TileType.AABBCC,
                        weight = 2.0f
                    }
                },
                new[]
                {
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentEdgeType_AreNotEqual()
        {
            var a = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            var b = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Rail,
                });

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentStationDataBlueprint_AreNotEqual()
        {
            var a = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                },
                new StationDataBlueprint(
                    ResourceType.Wood,
                    2,
                    ResourceType.Steel,
                    3));

            var b = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                },
                new StationDataBlueprint(
                    ResourceType.People,
                    2,
                    ResourceType.Steel,
                    3));

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void NullStationDataBlueprints_AreEqual()
        {
            var a = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            var b = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void SameReference_IsEqual()
        {
            var blueprint = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            Assert.IsTrue(blueprint.Equals(blueprint));
        }

        [Test]
        public void Null_IsNotEqual()
        {
            var blueprint = new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                });

            Assert.IsFalse(blueprint.Equals(null));
        }

        [Test]
        public void TestSerialization()
        {
            TestUtils.TestSerialization(new HexTileDataBlueprint(
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
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                }));

            TestUtils.TestSerialization(new HexTileDataBlueprint(
                new[]
                {
                    new HexTileDataBlueprint.TileTypeWithWeight
                    {
                        type = TileType.AABBCC,
                        weight = 1.0f
                    },
                    new HexTileDataBlueprint.TileTypeWithWeight
                    {
                        type = TileType.TAATBB,
                        weight = 2.0f
                    },
                    new HexTileDataBlueprint.TileTypeWithWeight
                    {
                        type = TileType.TTTTAA,
                        weight = 5.0f
                    }
                },
                new[]
                {
                    EdgeType.Fields,
                    EdgeType.Mountain,
                    EdgeType.Town,
                }));
        }
    }
}
