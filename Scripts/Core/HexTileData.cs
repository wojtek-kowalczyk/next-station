using System;
using System.Linq;
using MessagePack;
using UnityEngine.Assertions;

namespace NS.Core
{
    // TODO : consider struct, this should probably be immutable
    [MessagePackObject]
    public class HexTileData
    {
        // Note: sadly, we must store placement rotation inside a tile to be able to undo rotation
        // for decoration selection, because otherwise it's inconsistent with preview tile.

        [Key(0)] public readonly EdgeType[] edges = new EdgeType[6];
        [Key(1)] public readonly int decorationsSeed;
        [Key(2)] public readonly int placementRotation;
        [Key(3)] public readonly StationData stationData;

        [IgnoreMember]
        public bool IsStation => stationData.IsEmpty == false;

        // TODO: this should be extracted to an edge set struct
        // Right, going clockwise
        [IgnoreMember] public EdgeType Edge0 => edges[0]; // E
        [IgnoreMember] public EdgeType Edge1 => edges[1]; // SE
        [IgnoreMember] public EdgeType Edge2 => edges[2]; // SW
        [IgnoreMember] public EdgeType Edge3 => edges[3]; // W
        [IgnoreMember] public EdgeType Edge4 => edges[4]; // NW
        [IgnoreMember] public EdgeType Edge5 => edges[5]; // NE

        public HexTileData()
        {
        }

        public HexTileData(EdgeType[] sixTileEdges, int decorationsSeed, int placementRotation, StationData stationData)
        {
            Assert.IsTrue(sixTileEdges.Length == 6 && sixTileEdges.Length == edges.Length);

            Array.Copy(sixTileEdges, edges, sixTileEdges.Length);
            this.decorationsSeed = decorationsSeed;
            this.stationData = stationData;
            this.placementRotation = placementRotation;
        }

        public HexTileData WithStation(StationData newStationData)
        {
            return new HexTileData(edges, decorationsSeed, placementRotation, newStationData);
        }

        private bool Equals(HexTileData other)
        {
            // Note: placementRotation and decorationSeed do not matter; they're only used for visuals

            if (stationData.IsEmpty ^ other.stationData.IsEmpty)
            {
                return false;
            }

            bool edgesMatch = edges.SequenceEqual(other.edges);

            if (stationData.IsEmpty == false)
            {
                return edgesMatch && stationData.Equals(other.stationData);
            }

            return edgesMatch;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HexTileData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Edge0, Edge1, Edge2, Edge3, Edge4, Edge5, IsStation);
        }

        public override string ToString()
        {
            return $"Edges: {Edge0}, {Edge1}, {Edge2}, {Edge3}, {Edge4}, {Edge5} | Station? {IsStation} | " +
                   $"seed: {decorationsSeed} | rotation: {placementRotation}";
        }

        public HexTileData Rotated(int stepsClockwise)
        {
            // clockwise means I go forward one step, and my place is taken by a previous element.
            // rotating an array by 1 clockwise is equivalent to rotating by 5 counterclockwise.
            // we use ccw steps, because the resulting indexing is simpler (no negative numbers modulo).
            int stepsCcw = (6 - (stepsClockwise % 6)) % 6;

            EdgeType[] rotatedEdges = {
                edges[(0 + stepsCcw) % 6],
                edges[(1 + stepsCcw) % 6],
                edges[(2 + stepsCcw) % 6],
                edges[(3 + stepsCcw) % 6],
                edges[(4 + stepsCcw) % 6],
                edges[(5 + stepsCcw) % 6],
            };

            int newPlacementRotation = (placementRotation + stepsClockwise) % 6;
            return new HexTileData(rotatedEdges, decorationsSeed, newPlacementRotation, stationData);
        }
    }
}
