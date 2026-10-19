using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.Core
{
    /// a bitmask representing which edges of a hex tile are "active".
    [Serializable]
    public struct EdgeMask : IEquatable<EdgeMask>
    {
        public const byte SIX_LOWEST_BITS_MASK = 0b00111111;

        [SerializeField]
        private byte mask;

        public EdgeMask(byte mask)
        {
            this.mask = mask;
        }

        // passing 0, 2, 3 will set 1st, 3rd, and 4th edges to be active in the mask
        public EdgeMask(params int[] edgeIndices)
        {
            Assert.IsTrue(edgeIndices.Length <= 6);

            mask = 0;

            foreach (int edgeIndex in edgeIndices)
            {
                Set(edgeIndex);
            }

            Assert.IsTrue(mask == (mask & SIX_LOWEST_BITS_MASK));
        }

        public static EdgeMask FromTile(HexTileData tileData, EdgeType type)
        {
            var mask = new EdgeMask((byte)0);
            for (int i = 0; i < tileData.edges.Length; i++)
            {
                EdgeType edge = tileData.edges[i];
                if (edge == type)
                {
                    mask.Set(i);
                }
            }

            return mask;
        }

        public void Set(int edgeIndex)
        {
            Assert.IsTrue(edgeIndex >= 0 && edgeIndex < 6);
            mask |= (byte)(1 << edgeIndex);
        }

        public override string ToString()
        {
            return Convert.ToString(mask, 2).PadLeft(6, '0');
        }

        public bool Equals(EdgeMask other)
        {
            return (mask & SIX_LOWEST_BITS_MASK) == (other.mask & SIX_LOWEST_BITS_MASK);
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeMask other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (mask & SIX_LOWEST_BITS_MASK).GetHashCode();
        }

        // accessors for drawing in a custom property drawer
        public byte Mask
        {
            get => mask;
            set => mask = value;
        }
    }
}
