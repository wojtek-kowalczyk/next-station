using System;
using MessagePack;

namespace NS.Core
{
    [MessagePackObject]
    public readonly struct StationData : IEquatable<StationData>
    {
        [Key(0)] public readonly ResourceType wantsResource;
        [Key(1)] public readonly int wantsResourceAmount;
        [Key(2)] public readonly ResourceType givesResource;
        [Key(3)] public readonly int givesResourceAmount;

        [IgnoreMember] public bool IsEmpty => this.Equals(default);

        public static bool TryTransferResourcesTo(ref StationData from, ref StationData to)
        {
            if (from.givesResource == to.wantsResource)
            {
                int amount = Math.Min(from.givesResourceAmount, to.wantsResourceAmount);

                from = new StationData(from.wantsResource, from.wantsResourceAmount,
                    from.givesResource, from.givesResourceAmount - amount);
                to = new StationData(to.wantsResource, to.wantsResourceAmount - amount,
                    to.givesResource, to.givesResourceAmount);

                return amount > 0;
            }

            return false;
        }

        public StationData(ResourceType wantsResource, int wantsResourceAmount, ResourceType givesResource, int givesResourceAmount)
        {
            this.wantsResource = wantsResource;
            this.wantsResourceAmount = wantsResourceAmount;
            this.givesResource = givesResource;
            this.givesResourceAmount = givesResourceAmount;
        }

        public bool Equals(StationData other)
        {
            return wantsResource == other.wantsResource && wantsResourceAmount == other.wantsResourceAmount &&
                   givesResource == other.givesResource && givesResourceAmount == other.givesResourceAmount;
        }

        public override bool Equals(object obj)
        {
            return obj is StationData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)wantsResource, wantsResourceAmount, (int)givesResource, givesResourceAmount);
        }

        public StationData WithWants(ResourceType resource, int amount)
        {
            return new StationData(resource, amount, givesResource, givesResourceAmount);
        }

        public StationData WithGives(ResourceType resource, int amount)
        {
            return new StationData(wantsResource, wantsResourceAmount, resource, amount);
        }

        public override string ToString()
        {
            return $"Station: wants {wantsResourceAmount} {wantsResource}, " +
                   $"gives: {givesResourceAmount} {givesResource}";
        }
    }
}
