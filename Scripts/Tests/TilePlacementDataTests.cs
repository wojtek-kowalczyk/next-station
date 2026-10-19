using NS.Core;
using NUnit.Framework;
using UnityEngine;

public class TilePlacementDataTests
{
    private static TilePlacementData CreateDefault() =>
        new TilePlacementData(
            nonRotatedTile: null,
            cell: new Vector2Int(1, 2),
            rotation: 3,
            wasPlacedByPlayer: true);

    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = CreateDefault();
        var b = CreateDefault();

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(b.Equals(a));
    }

    [Test]
    public void Equals_SameReference_ReturnsTrue()
    {
        var data = CreateDefault();

        Assert.IsTrue(data.Equals(data));
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        var data = CreateDefault();

        Assert.IsTrue(data.Equals(null) == false);
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var data = CreateDefault();

        Assert.IsTrue(data.Equals(new object()) == false);
    }

    [Test]
    public void Equals_DifferentTile_ReturnsFalse()
    {
        var a = new TilePlacementData(
            nonRotatedTile: null,
            cell: new Vector2Int(1, 2),
            rotation: 3,
            wasPlacedByPlayer: true);

        var b = new TilePlacementData(
            nonRotatedTile: new HexTileData(),
            cell: new Vector2Int(1, 2),
            rotation: 3,
            wasPlacedByPlayer: true);

        Assert.IsTrue(a.Equals(b) == false);
    }

    [Test]
    public void Equals_DifferentCell_ReturnsFalse()
    {
        var a = CreateDefault();

        var b = new TilePlacementData(
            nonRotatedTile: null,
            cell: new Vector2Int(2, 2),
            rotation: 3,
            wasPlacedByPlayer: true);

        Assert.IsTrue(a.Equals(b) == false);
    }

    [Test]
    public void Equals_DifferentRotation_ReturnsFalse()
    {
        var a = CreateDefault();

        var b = new TilePlacementData(
            nonRotatedTile: null,
            cell: new Vector2Int(1, 2),
            rotation: 4,
            wasPlacedByPlayer: true);

        Assert.IsTrue(a.Equals(b) == false);
    }

    [Test]
    public void Equals_DifferentWasPlacedByPlayer_ReturnsFalse()
    {
        var a = CreateDefault();

        var b = new TilePlacementData(
            nonRotatedTile: null,
            cell: new Vector2Int(1, 2),
            rotation: 3,
            wasPlacedByPlayer: false);

        Assert.IsTrue(a.Equals(b) == false);
    }

    [Test]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        var a = CreateDefault();
        var b = CreateDefault();

        Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
    }
}
