using System;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace NS.Core
{
    public partial class RailNetwork
    {
        /// Describes the supply of a set of rail networks, as if there were balanced all together
        public readonly struct SupplyInfo : IEquatable<SupplyInfo>
        {
            private readonly ResourceType[] resources;
            private readonly int[] givesAmounts;

            public SupplyInfo(ResourceType[] resources, int[] givesAmounts)
            {
                Assert.IsNotNull(resources);
                Assert.IsNotNull(givesAmounts);
                this.resources = resources;
                this.givesAmounts = givesAmounts;
            }

            public int Gives(ResourceType type)
            {
                if (resources == null || givesAmounts == null)
                {
                    return 0;
                }

                for (int i = 0; i < givesAmounts.Length; i++)
                {
                    if (resources[i] == type)
                    {
                        return givesAmounts[i];
                    }
                }

                return 0;
            }

            public float GivesProb01(ResourceType type)
            {
                if (givesAmounts == null)
                {
                    return 0;
                }

                int givesAll = givesAmounts.Sum();
                if (givesAll == 0)
                {
                    return 0;
                }

                return Gives(type) / (float)givesAll;
            }

            public bool GivesNothing()
            {
                if (givesAmounts == null)
                {
                    return true;
                }

                return givesAmounts.All(x => x == 0);
            }

            public override string ToString()
            {
                if (GivesNothing())
                {
                    return "Empty";
                }

                StringBuilder sb = new();
                for (int i = 0; i < resources.Length; i++)
                {
                    sb.Append($"{resources[i]}: {givesAmounts[i]}, ");
                }
                sb.Remove(sb.Length - 2, 2);
                return sb.ToString();
            }

            public bool Equals(SupplyInfo other)
            {
                return resources.SequenceEqual(other.resources) && givesAmounts.SequenceEqual(other.givesAmounts);
            }

            public override bool Equals(object obj)
            {
                return obj is SupplyInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(resources, givesAmounts);
            }
        }
    }
}
