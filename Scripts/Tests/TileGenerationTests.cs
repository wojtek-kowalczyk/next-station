using System;
using NS.Core;
using NS.Core.Blueprints;
using NUnit.Framework;
using UnityEngine;
using Random = NS.Core.Random;

public class TileGenerationTests
{
    private readonly StationDataBlueprint dummyStationBp = new(
        ResourceType.Wood | ResourceType.People, new Vector2Int(1, 1),
        ResourceType.Gold, new Vector2Int(1, 3));

    [Test]
    public void TestTileGen()
    {
        var bp = new HexTileDataBlueprint(
            new[] { new HexTileDataBlueprint.TileTypeWithWeight() { type = TileType.AABBCC, weight = 1 } },
            new[] { EdgeType.Mountain, EdgeType.Town, EdgeType.Basic, });

        for (int i = 0; i < 10_000; i++)
        {
            HexTileData t = bp.Generate(new Random(i));

            Assert.IsNotNull(t);

            // AA
            Assert.IsTrue(t.Edge0 == t.Edge1 && t.Edge0 != EdgeType.Fields);
            // BB
            Assert.IsTrue(t.Edge2 == t.Edge3 && t.Edge2 != EdgeType.Fields);
            Assert.IsTrue(t.Edge2 != t.Edge0);
            // CC
            Assert.IsTrue(t.Edge4 == t.Edge5 && t.Edge4 != EdgeType.Fields);
            Assert.IsTrue(t.Edge4 != t.Edge0);
            Assert.IsTrue(t.Edge4 != t.Edge2);
        }
    }

    [Test]
    public void StationGenerationTests()
    {
        var bp = new HexTileDataBlueprint(new[] { new HexTileDataBlueprint.TileTypeWithWeight() { type = TileType.TTTTTT, weight = 1 } },
            new EdgeType[] { }, dummyStationBp);

        var rand = new Random(55);

        for (int i = 0; i < 10_000; i++)
        {
            HexTileData tile = bp.Generate(rand);
            Assert.IsNotNull(tile);
            Assert.IsTrue(tile.IsStation);

            StationData stationData = tile.stationData;
            Assert.IsTrue(stationData.IsEmpty == false);
            Assert.IsTrue(stationData.wantsResource == ResourceType.Wood || stationData.wantsResource == ResourceType.People);
            Assert.IsTrue(stationData.wantsResourceAmount == 1);
            Assert.IsTrue(stationData.givesResource == ResourceType.Gold);
            Assert.IsTrue(stationData.givesResourceAmount >= 1 && stationData.givesResourceAmount <= 3);
        }
    }

    [Test]
    public void TestStationGivesWantsTheSame()
    {
        var bp = new StationDataBlueprint(
            ResourceType.All, new Vector2Int(1, 1),
            ResourceType.All, new Vector2Int(1, 1));

        var rand = new Random(123);

        for (int i = 0; i < 10_000; i++)
        {
            StationData stationData = bp.Generate(rand);
            Assert.IsTrue(stationData.IsEmpty == false);
            Assert.IsTrue(stationData.wantsResource != stationData.givesResource);
        }
    }

    [Test]
    public void TestInvalidBlueprintDetected()
    {
        var rand = new Random(123);
        var amount = new Vector2Int(1, 1);

        Assert.Throws<InvalidOperationException>(() =>
        {
            var bp = new StationDataBlueprint(
                ResourceType.Gold, amount,
                ResourceType.Gold, amount);
            bp.Generate(rand);
        });

        Assert.DoesNotThrow(() =>
        {
            var bp = new StationDataBlueprint(
                ResourceType.Gold | ResourceType.People, amount,
                ResourceType.Gold, amount);
            bp.Generate(rand);
        });

        Assert.DoesNotThrow(() =>
        {
            var bp = new StationDataBlueprint(
                ResourceType.Gold, amount,
                ResourceType.Gold | ResourceType.People, amount);
            bp.Generate(rand);
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            var bp = new StationDataBlueprint(
                ResourceType.None, amount,
                ResourceType.None, amount);
            bp.Generate(rand);
        });
    }

    [Test]
    public void TestNoWantsStations()
    {
        var rand = new Random(9867);
        var amount = new Vector2Int(1, 1);

        {
            var bp = new StationDataBlueprint(
                ResourceType.None, amount,
                ResourceType.Gold, amount);
            var output = bp.Generate(rand);
            Assert.IsTrue(output.wantsResource == ResourceType.None);
            Assert.IsTrue(output.givesResource == ResourceType.Gold);
        }

        for (int i = 0; i < 1000; i++)
        {
            var bp = new StationDataBlueprint(
                ResourceType.None, amount,
                ResourceType.People | ResourceType.Wood | ResourceType.Gold | ResourceType.Steel, amount);
            var output = bp.Generate(rand);
            Assert.IsTrue(output.wantsResource == ResourceType.None);
            Assert.IsTrue(output.givesResource == ResourceType.Gold ||
                          output.givesResource == ResourceType.Wood ||
                          output.givesResource == ResourceType.Steel ||
                          output.givesResource == ResourceType.People);
        }
    }

    [Test]
    public void TestNoGivesStations()
    {
        var rand = new Random(2223);
        var amount = new Vector2Int(1, 1);

        {
            var bp = new StationDataBlueprint(
                ResourceType.Gold, amount,
                ResourceType.None, amount);
            var output = bp.Generate(rand);
            Assert.IsTrue(output.givesResource == ResourceType.None);
            Assert.IsTrue(output.wantsResource == ResourceType.Gold);
        }

        for (int i = 0; i < 1000; i++)
        {
            var bp = new StationDataBlueprint(
                ResourceType.People | ResourceType.Wood | ResourceType.Gold | ResourceType.Steel, amount,
                ResourceType.None, amount);
            var output = bp.Generate(rand);
            Assert.IsTrue(output.givesResource == ResourceType.None);
            Assert.IsTrue(output.wantsResource == ResourceType.Gold ||
                          output.wantsResource == ResourceType.Wood ||
                          output.wantsResource == ResourceType.Steel ||
                          output.wantsResource == ResourceType.People);
        }
    }
}
