using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NS.Core.Blueprints;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.Core
{
    [MessagePackObject]
    public class TileStack : ITileStack
    {
        private const int NUM_TILES_TO_PRELOAD = 3;

        private readonly Queue<HexTileData> tiles;
        private readonly Queue<HexTileDataBlueprint> blueprints;
        private readonly Random rand;

        private int numTilesLeftToGenerate;

        [IgnoreMember]
        public int Count => tiles.Count + numTilesLeftToGenerate;

        [Key(0)] public SerializationData SerializationProxy => new(this);

        public TileStack(SerializationData serializationData)
        {
            tiles = serializationData.generatedTiles;
            blueprints = serializationData.blueprints;
            rand = serializationData.rand;
            numTilesLeftToGenerate = serializationData.numTilesLeftToGenerate;
        }

        public TileStack(int count, IEnumerable<HexTileDataBlueprint> blueprints, HexTileDataBlueprint initialBlueprint, int seed)
        {
            Assert.IsNotNull(blueprints);

            this.numTilesLeftToGenerate = count;
            this.blueprints = new Queue<HexTileDataBlueprint>(blueprints);
            rand = new Random(seed);

            tiles = new Queue<HexTileData>();
            int numToPreload = Mathf.Min(NUM_TILES_TO_PRELOAD, count);
            for (int i = 0; i < numToPreload ; i++)
            {
                GenerateNextTile(initialBlueprint);
            }
        }

        public HexTileData PeekTile()
        {
            if (Count == 0)
            {
                return null;
            }

            return tiles.Peek();
        }

        public Span<HexTileData> PeekTiles()
        {
            int numToPeek = Mathf.Min(tiles.Count, NUM_TILES_TO_PRELOAD);
            return tiles.ToArray()[..numToPeek];
        }

        public void AddTiles(int count)
        {
            numTilesLeftToGenerate += count;

            // generate up to preload limit if possible
            int numToPreload = Mathf.Min(NUM_TILES_TO_PRELOAD, count);
            for (int i = 0; i < numToPreload - tiles.Count; i++)
            {
                GenerateNextTile();
            }
        }

        public void PushNextTile(HexTileData tile)
        {
            List<HexTileData> list = tiles.ToList();
            list.Insert(0, tile);
            tiles.Clear();
            list.ForEach(x => tiles.Enqueue(x));
        }

        public HexTileData PopTile()
        {
            if (Count == 0)
            {
                return null;
            }

            HexTileData tile = tiles.Dequeue();

            if (numTilesLeftToGenerate > 0)
            {
                GenerateNextTile();
            }

            return tile;
        }

        private void GenerateNextTile(HexTileDataBlueprint blueprintOverride = null)
        {
            Assert.IsTrue(numTilesLeftToGenerate > 0);

            HexTileDataBlueprint bp = blueprintOverride ?? blueprints.Dequeue();
            tiles.Enqueue(bp.Generate(rand));
            if (blueprintOverride == null) // was dequeued, so enqueue it back (circular buffer)
            {
                blueprints.Enqueue(bp);
            }
            numTilesLeftToGenerate--;
        }

        protected bool Equals(TileStack other)
        {
            if (Count != other.Count) return false;
            if (numTilesLeftToGenerate != other.numTilesLeftToGenerate) return false;
            if (rand.Equals(other.rand) == false) return false;
            if (tiles.SequenceEqual(other.tiles) == false) return false;
            if (blueprints.SequenceEqual(other.blueprints) == false) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TileStack)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Should not be hashed");
        }

        [MessagePackObject]
        public class SerializationData
        {
            [Key(0)] public Queue<HexTileData> generatedTiles;
            [Key(1)] public int numTilesLeftToGenerate;
            [Key(2)] public Random rand;
            [Key(3)] public Queue<HexTileDataBlueprint> blueprints;

            public SerializationData()
            {
            }

            public SerializationData(TileStack tileStack)
            {
                generatedTiles = tileStack.tiles;
                numTilesLeftToGenerate = tileStack.numTilesLeftToGenerate;
                rand = tileStack.rand;
                blueprints = tileStack.blueprints;
            }
        }
    }
}
