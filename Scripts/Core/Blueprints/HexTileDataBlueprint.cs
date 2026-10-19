using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MessagePack;
using NS.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.Core.Blueprints
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable IdentifierTypo

    /// If you're adding things here remember to update <see cref="TileTypeExtensions"/>
    /// Don't leave holes in this enum, iteration logic breaks.
    [Flags] // TODO : consider not using flags for this, makes leaving holes really ugly
    public enum TileType
    {
        None = 0,

        AABBCC = 1 << 0,
        AAABBB = 1 << 1,
        AAAABB = 1 << 2,
        AAAAAA = 1 << 3,

        TXTXTX = 1 << 4,
        TTAAAA = 1 << 5,
        TTAABB = 1 << 6,
        TAATBB = 1 << 7,
        TAATAA = 1 << 8, // Don't use this. District closing logic fails on it and it isn't that attractive anyway.
        TXTAAA = 1 << 9,

        TTTTTT = 1 << 10,
        TTTAAA = 1 << 11,
        TTTTAA = 1 << 12,
        TAAAAA = 1 << 13,

        All = ~0,
    }

    // ReSharper restore IdentifierTypo
    // ReSharper restore InconsistentNaming

    public static class TileTypeExtensions
    {
        public const int TILE_TYPE_COUNT = 14;

        public static int GetEdgeTypeCount(this TileType tileType)
        {
            Assert.IsTrue(Utils.NumSetBits((uint)tileType) == 1);

            return tileType switch
            {
                TileType.AABBCC => 3,
                TileType.AAABBB => 2,
                TileType.AAAABB => 2,
                TileType.AAAAAA => 1,
                TileType.TXTXTX => 0,
                TileType.TTAAAA => 1,
                TileType.TTAABB => 2,
                TileType.TAATBB => 2,
                TileType.TAATAA => 1,
                TileType.TXTAAA => 1,
                TileType.TTTTTT => 0,
                TileType.TTTAAA => 1,
                TileType.TTTTAA => 1,
                TileType.TAAAAA => 1,
                TileType.All => 3,
                // TileType.None => 0, // should never be called with None
                _ => throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null)
            };
        }
    }

    /// <summary>
    /// Tile data blueprint, used to generate <see cref="HexTileData"/>s
    /// </summary>
    [Serializable, MessagePackObject]
    public class HexTileDataBlueprint
    {
        public enum TileBlueprintSetupError
        {
            None,
            NotEnoughEdges,
            DuplicateEdges,
        }

        [Serializable, MessagePackObject]
        public struct TileTypeWithWeight : IEquatable<TileTypeWithWeight>
        {
            [Key(0)] public TileType type;
            [Key(1)] public float weight;

            public bool Equals(TileTypeWithWeight other)
            {
                return type == other.type && weight.Equals(other.weight);
            }

            public override bool Equals(object obj)
            {
                return obj is TileTypeWithWeight other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine((int)type, weight);
            }
        }

        public class GenerationDebugInfo
        {
            public TileType tileType;
        }

        [SerializeField] private TileTypeWithWeight[] tileTypes;
        [SerializeField] private EdgeType[] edgeTypes;
        // Note: NonSerialized because in the inspector we supply this value via ScriptableObject
        [NonSerialized] private StationDataBlueprint stationDataBlueprint;

        [Key(0)] public TileTypeWithWeight[] TileTypes => tileTypes;
        [Key(1)] public EdgeType[] EdgeTypes => edgeTypes;
        [Key(2)] public StationDataBlueprint StationDataBlueprint => stationDataBlueprint;

        public HexTileDataBlueprint(TileTypeWithWeight[] tileTypes, EdgeType[] edgeTypes, StationDataBlueprint stationDataBlueprint = null)
        {
            this.tileTypes = tileTypes;
            this.edgeTypes = edgeTypes;
            this.stationDataBlueprint = stationDataBlueprint;

            TileBlueprintSetupError error = Validate();
            Assert.IsTrue(error == TileBlueprintSetupError.None, $"error: {error}");
        }

        [CanBeNull]
        public HexTileData Generate(Random rand,
            StationDataBlueprint stationDataBlueprintOverride = null,
            GenerationDebugInfo debugInfo = null)
        {
            Assert.IsTrue(tileTypes.Length > 0);

            var err = Validate();
            if (err != TileBlueprintSetupError.None)
            {
                Debug.LogError($"[HexTileDataBlueprint] invalid blueprint: {err}");
                return null;
            }

            // select tile type
            float[] weights = tileTypes.Select(x => x.weight).ToArray();
            int index = Utils.WeightedSample(weights, rand);
            TileType selectedTileType = tileTypes[index].type;
            if (debugInfo != null) debugInfo.tileType = selectedTileType;
            Assert.IsTrue(selectedTileType != TileType.None && Utils.NumSetBits((uint)selectedTileType) == 1);

            // select edges
            Assert.IsTrue(edgeTypes.Length >= selectedTileType.GetEdgeTypeCount());
            var edges = new EdgeType[edgeTypes.Length];
            Array.Copy(edgeTypes, edges, edgeTypes.Length);
            edges.Shuffle(rand);

            // build edges
            string typeString = selectedTileType.ToString();
            Assert.IsTrue(typeString.Length == 6);
            var tileEdges = new EdgeType[6];
            int edgeIndex = 0;
            foreach (char c in typeString)
            {
                tileEdges[edgeIndex] = c switch
                {
                    'T' => EdgeType.Rail,
                    'X' => EdgeType.Basic,
                    'A' => edges[0],
                    'B' => edges[1],
                    'C' => edges[2],
                    _ => throw new ArgumentOutOfRangeException()
                };

                edgeIndex++;
            }

            StationData stationData = stationDataBlueprintOverride != null
                ? stationDataBlueprintOverride.Generate(rand)
                : StationDataBlueprint?.Generate(rand) ?? default;

            var result = new HexTileData(tileEdges, rand.NextInt(), 0, stationData);

            return result;
        }

        public TileBlueprintSetupError Validate()
        {
            int requiredEdgeTypes = 0;
            foreach (TileTypeWithWeight typeWithWeight in tileTypes)
            {
                if (typeWithWeight.type != TileType.None)
                {
                    requiredEdgeTypes = Mathf.Max(typeWithWeight.type.GetEdgeTypeCount(), requiredEdgeTypes);
                }
            }

            HashSet<EdgeType> edges = new();
            foreach (EdgeType edge in edgeTypes)
            {
                if (edges.Add(edge) == false)
                {
                    return TileBlueprintSetupError.DuplicateEdges;
                }
            }

            return edgeTypes.Length >= requiredEdgeTypes
                ? TileBlueprintSetupError.None
                : TileBlueprintSetupError.NotEnoughEdges;
        }

        protected bool Equals(HexTileDataBlueprint other)
        {
            return tileTypes.SequenceEqual(other.tileTypes) && edgeTypes.SequenceEqual(other.edgeTypes) && Equals(stationDataBlueprint, other.stationDataBlueprint);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HexTileDataBlueprint)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Should not be hashed");
        }
    }
}
