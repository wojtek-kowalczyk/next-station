using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NS.Core;
using UnityEngine;
using NS.Utility;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Random = NS.Core.Random;

namespace NS.Visuals
{
    [CreateAssetMenu(fileName = "tileDecorationConfig", menuName = "Game/Config/TileDecorationConfig")]
    public class TileDecorationConfig : ScriptableObject
    {
        public readonly struct Record : IEquatable<Record>
        {
            public readonly EdgeType edge;
            public readonly int count;
            public readonly int lastOccupiedIndex;

            public Record(EdgeType edge, int count, int lastOccupiedIndex = 0)
            {
                this.edge = edge;
                this.count = count;
                this.lastOccupiedIndex = lastOccupiedIndex;
            }

            public bool Equals(Record other)
            {
                return edge == other.edge && count == other.count;
            }

            public override bool Equals(object obj)
            {
                return obj is Record other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine((int)edge, count);
            }

            public override string ToString()
            {
                return $"({edge}, {count})";
            }
        }

        [Serializable]
        public struct RailMapping
        {
            public EdgeMask edges;
            public GameObject[] prefabs;
        }

        [Serializable]
        public struct StationMapping
        {
            public EdgeMask railMask;
            public EdgeType otherEdgesType; // note: single color for all edges, otherise use generic station model
            public GameObject[] prefabs;
        }

        [Header("General Settings")]
        [SerializeField] private float baseRotation = 60; // simpler to factor this in than to rotate all the models.
        [SerializeField] private float baseYOffset = -0.26f; // account for how previous models were made

        [Header("Mountains")]
        [SerializeField] private List<GameObject> mountain2 = new();
        [SerializeField] private List<GameObject> mountain3 = new();
        [SerializeField] private List<GameObject> mountain4 = new();
        [SerializeField] private List<GameObject> mountain6 = new();

        [Header("Town")]
        [SerializeField] private List<GameObject> town2 = new();
        [SerializeField] private List<GameObject> town3 = new();
        [SerializeField] private List<GameObject> town4 = new();
        [SerializeField] private List<GameObject> town6 = new();

        [Header("Fields")]
        [SerializeField] private List<GameObject> fields2 = new();
        [SerializeField] private List<GameObject> fields3 = new();
        [SerializeField] private List<GameObject> fields4 = new();
        [SerializeField] private List<GameObject> fields6 = new();

        [Header("Basic")]
        [SerializeField] private List<GameObject> basic2 = new();
        [SerializeField] private List<GameObject> basic3 = new();
        [SerializeField] private List<GameObject> basic4 = new();
        [SerializeField] private List<GameObject> basic6 = new();

        [Header("Rail")]
        [SerializeField] private List<RailMapping> rails = new();

        [Header("Stations")]
        [SerializeField] private List<StationMapping> stations = new();
        [SerializeField] private GameObject genericStationPrefab;
        [SerializeField] private GameObject station6;

        // maps edge type with how many of that edge to possible models for such configuration
        private readonly Dictionary<Record, List<GameObject>> modelDict = new();
        // maps tile type to possible rail models. not all tile types have rails
        private readonly Dictionary<EdgeMask, GameObject[]> railsDict = new();

        public float BaseRotation => baseRotation;
        public float BaseYOffset => baseYOffset;

        public void Initialize()
        {
            modelDict.Add(new Record(EdgeType.Mountain, 2), mountain2);
            modelDict.Add(new Record(EdgeType.Mountain, 3), mountain3);
            modelDict.Add(new Record(EdgeType.Mountain, 4), mountain4);
            modelDict.Add(new Record(EdgeType.Mountain, 6), mountain6);

            modelDict.Add(new Record(EdgeType.Town, 2), town2);
            modelDict.Add(new Record(EdgeType.Town, 3), town3);
            modelDict.Add(new Record(EdgeType.Town, 4), town4);
            modelDict.Add(new Record(EdgeType.Town, 6), town6);

            modelDict.Add(new Record(EdgeType.Fields, 2), fields2);
            modelDict.Add(new Record(EdgeType.Fields, 3), fields3);
            modelDict.Add(new Record(EdgeType.Fields, 4), fields4);
            modelDict.Add(new Record(EdgeType.Fields, 6), fields6);

            modelDict.Add(new Record(EdgeType.Basic, 2), basic2);
            modelDict.Add(new Record(EdgeType.Basic, 3), basic3);
            modelDict.Add(new Record(EdgeType.Basic, 4), basic4);
            modelDict.Add(new Record(EdgeType.Basic, 6), basic6);

            // only here because we can't serialize dictionaries in the inspector (easily)
            foreach (RailMapping railMapping in rails)
            {
                railsDict.Add(railMapping.edges, railMapping.prefabs);
            }
        }

        public readonly struct StationDecorationSelection
        {
            public readonly GameObject prefab;
            public readonly int rotation;
            public readonly bool isGenericStation;

            public StationDecorationSelection(GameObject prefab, int rotation, bool isGenericStation)
            {
                this.prefab = prefab;
                this.rotation = rotation;
                this.isGenericStation = isGenericStation;
            }
        }

        public StationDecorationSelection GetStationDecoration(HexTileData tileData, Random rand)
        {
            Assert.IsTrue(tileData.placementRotation == 0);

            // although the placement rotation is 0, the tile can actually have edges on arbitrary positions.

            using var po = HashSetPool<EdgeType>.Get(out var nonRailEdgeTypes);
            foreach (EdgeType edgeType in tileData.edges)
            {
                if (edgeType != EdgeType.Rail)
                {
                    nonRailEdgeTypes.Add(edgeType);
                }
            }

            if (nonRailEdgeTypes.Count == 0)
            {
                return new StationDecorationSelection(station6, 0, false);
            }

            if (nonRailEdgeTypes.Count != 1)
            {
                return new StationDecorationSelection(genericStationPrefab, 0,true);
            }

            EdgeType otherEdgesColor = nonRailEdgeTypes.First();

            // select a subset to not iterate the whole collection 6 times
            List<StationMapping> mappingsForColor = stations
                .Where(mapping => mapping.otherEdgesType == otherEdgesColor).ToList();

            for (int rotation = 0; rotation < 6; rotation++)
            {
                // try all six rotations because config is set up for one rotation only
                EdgeMask railMask = EdgeMask.FromTile(tileData.Rotated(rotation), EdgeType.Rail);

                StationMapping match = mappingsForColor.Find(x => x.railMask.Equals(railMask));
                if (match.prefabs != null && match.prefabs.Length > 0)
                {
                    return new StationDecorationSelection(match.prefabs.RandomElement(rand), rotation, false);
                }
            }

            return new StationDecorationSelection(genericStationPrefab, 0, true);
        }

        /// <returns>
        /// Decorations prefabs with their desired rotation for a given tile.
        /// Empty array if decorations could not be found for this tile.
        /// Note: returned array may contain (null, 0) entries
        /// </returns>
        public (GameObject prefab, int rotation)[] GetDecorationPrefabs(HexTileData tileData, Random rand)
        {
            Assert.IsTrue(tileData.placementRotation == 0);

            List<Record> records = new();

            int index = -1;
            int currCount = 0;
            EdgeType? lastEdge = null;
            foreach (EdgeType edge in tileData.edges)
            {
                if (lastEdge.HasValue && edge != lastEdge)
                {
                    if (lastEdge != EdgeType.Rail) // do not record rail, they are handled separately
                    {
                        records.Add(new Record(lastEdge.Value, currCount, index));
                    }

                    currCount = 0;
                }

                lastEdge = edge;
                currCount++;
                index++;
            }

            if (lastEdge != EdgeType.Rail)
            {
                records.Add(new Record(lastEdge!.Value, currCount, index));
            }

            // The config is set up for tiles with zero rotation, because that was simpler than to permute manually.
            // This has a consequence, that the tileData we receive here, must have rotation 0, which
            // is rather inconvenient for the calling code, which can pass arbitrary rotation.
            // Since we don't know that rotation, we cannot "unrotate" a tile to be at zero rotation.
            // We can, however, observe that we don't need zero rotation! We just need to find the right records.
            // So if first and last records are of the same type, that means it was "cut" by the invisible line
            // of starting record gathering at the right edge (unless it's TAATAA, but we ignore this tile altogether).
            // A simple merging of first and last record should do the trick.

            if (records.Count > 1)
            {
                // Note: this will not work for TAATAA tile, but it's disallowed anyway
                Record first = records[0];
                Record last = records[^1];
                if (first.edge == last.edge)
                {
                    records[0] = new Record(first.edge, first.count + last.count, first.lastOccupiedIndex);
                    records.Remove(last);
                }
            }

            var results = new (GameObject, int)[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                Record record = records[i];

                // skip this case, this is allowed.
                if (record.edge == EdgeType.Basic && record.count == 1)
                {
                    results[i] = default; // TODO : UGLY!
                    continue;
                }

                if (modelDict.TryGetValue(record, out List<GameObject> possibleObjects) == false)
                {
                    Debug.LogError($"[TileDecorationConfig] No decorations for {record}");
                    continue;
                }

                results[i] = (possibleObjects.RandomElement(rand), record.lastOccupiedIndex + 1 - record.count);
            }

            return results;
        }

        [CanBeNull]
        public GameObject GetRailsPrefab(HexTileData tileData, Random rand)
        {
            EdgeMask edgeMask = EdgeMask.FromTile(tileData, EdgeType.Rail);

            if (edgeMask.Mask == 0)
            {
                // no rails on this tile
                return null;
            }

            if (railsDict.TryGetValue(edgeMask, out GameObject[] possibleDecorations))
            {
                return possibleDecorations.RandomElement(rand);
            }

            return null;
        }
    }
}
