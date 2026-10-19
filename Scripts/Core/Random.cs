using System;
using MessagePack;
using UnityEngine.Assertions;

namespace NS.Core
{
    /// <summary>
    /// Custom random number generation with support for MessagePack serialization
    /// </summary>
    [MessagePackObject]
    public class Random
    {
        // ReSharper disable once MemberCanBePrivate.Global -- needs to be public for serialization
        [Key(0)]
        public uint state;

        public Random()
        {
            state = (uint)DateTime.Now.Ticks;
        }

        public Random(uint seed)
        {
            state = seed;
        }

        public Random(int seed)
        {
            state = (uint)seed;
        }

        public int NextInt()
        {
            return (int)NextUint();
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            Assert.IsTrue(minInclusive < maxExclusive);
            uint value = NextUint() % (uint)(checked(maxExclusive - minInclusive));
            return (int)value + minInclusive;
        }

        public uint NextUint()
        {
            state = state * 747796405u + 2891336453u;

            uint shift = (state >> 28) + 4;
            uint word = ((state >> (int)shift) ^ state) * 277803737u;

            return (word >> 22) ^ word;
        }

        public uint NextUint(uint minInclusive, uint maxExclusive)
        {
            Assert.IsTrue(minInclusive < maxExclusive);
            return NextUint() % (maxExclusive - minInclusive) + minInclusive;
        }

        public float NextFloat()
        {
            return (float)NextUint() / uint.MaxValue;
        }

        public float NextFloat(float minInclusive, float maxInclusive)
        {
            Assert.IsTrue(minInclusive <= maxInclusive);
            float f = NextFloat();
            return f * (maxInclusive - minInclusive) + minInclusive;
        }

        private bool Equals(Random other)
        {
            return state == other.state;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Random)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Should not be hashed");
        }
    }
}
