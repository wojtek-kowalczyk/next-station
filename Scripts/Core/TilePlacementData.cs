using System;
using MessagePack;
using UnityEngine;

namespace NS.Core
{
    // TODO : probably should be a struct?
    [Serializable, MessagePackObject]
    public class TilePlacementData
    {
        [Key(0)] public readonly HexTileData nonRotatedTile;
        [Key(1)] public readonly Vector2Int cell;
        [Key(2)] public readonly int rotation;
        [Key(3)] public readonly bool wasPlacedByPlayer;

        public TilePlacementData(HexTileData nonRotatedTile, Vector2Int cell, int rotation, bool wasPlacedByPlayer)
        {
            this.nonRotatedTile = nonRotatedTile;
            this.rotation = rotation;
            this.cell = cell;
            this.wasPlacedByPlayer = wasPlacedByPlayer;
        }

        protected bool Equals(TilePlacementData other)
        {
            return Equals(nonRotatedTile, other.nonRotatedTile) && cell.Equals(other.cell) && rotation == other.rotation &&
                   wasPlacedByPlayer == other.wasPlacedByPlayer;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TilePlacementData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nonRotatedTile, cell, rotation, wasPlacedByPlayer);
        }
    }
}
