using System;
using MessagePack;
using UnityEngine;

namespace NS.Core.Model
{
    [MessagePackObject]
    public class GameModel
    {
        public const int VERSION = 2;

        [Key(0)] public int Version { get; set; } = VERSION;
        [Key(1)] public HexGrid Grid { get; set; } = new(HexGridParams.Default);
        [Key(2)] public TileStack TileStack { get; set; }
        [Key(3)] public Random Rand { get; set; }
        [Key(4)] public int Points { get; set; }
        [Key(5)] public Vector2Int LastPlacementCell { get; set; }
        [Key(6)] public int StartingSeed { get; set; } // TODO : add to equals?

        protected bool Equals(GameModel other)
        {
            return Version == other.Version &&
                Equals(Grid, other.Grid) &&
                Equals(TileStack, other.TileStack) &&
                Equals(Rand, other.Rand) &&
                Points == other.Points &&
                LastPlacementCell.Equals(other.LastPlacementCell);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GameModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Version, Grid, TileStack, Rand, Points, LastPlacementCell);
        }
    }
}
