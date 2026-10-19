using System;
using MessagePack;
using UnityEngine;
using UnityEngine.Assertions;
using Utils = NS.Utility.Utils;

namespace NS.Core.Blueprints
{
    [Serializable, MessagePackObject]
    public class StationDataBlueprint
    {
        [SerializeField] private ResourceType wants;
        [SerializeField] private Vector2Int wantsAmount;
        [SerializeField] private ResourceType gives;
        [SerializeField] private Vector2Int givesAmount;

        [Key(0)] public ResourceType Wants => wants;
        [Key(1)] public Vector2Int WantsAmount => wantsAmount;
        [Key(2)] public ResourceType Gives => gives;
        [Key(3)] public Vector2Int GivesAmount => givesAmount;

        public StationDataBlueprint(ResourceType wants, int wantsAmount, ResourceType gives, int givesAmount)
        {
            this.wants = wants;
            this.wantsAmount = new Vector2Int(wantsAmount, wantsAmount);
            this.gives = gives;
            this.givesAmount = new Vector2Int(givesAmount, givesAmount);
        }

        public StationDataBlueprint(ResourceType wants, Vector2Int wantsAmount, ResourceType gives, Vector2Int givesAmount)
        {
            Assert.IsTrue(wantsAmount.x <= wantsAmount.y);
            Assert.IsTrue(givesAmount.x <= givesAmount.y);

            this.wants = wants;
            this.wantsAmount = wantsAmount;
            this.gives = gives;
            this.givesAmount = givesAmount;
        }

        public StationData Generate(Random rand)
        {
            if (wants == ResourceType.None && gives == ResourceType.None)
            {
                throw new InvalidOperationException("either 'wants' or 'gives' can be None, not both at once");
            }

            int numWantsBits = Utils.NumSetBits((uint)wants);
            int numGivesBits = Utils.NumSetBits((uint)gives);

            if (numWantsBits == 1 && numGivesBits == 1 && wants == gives)
            {
                throw new InvalidOperationException("impossible to generate a valid station: " +
                                                    $"wants: {wants}, gives: {gives}");
            }

            int givesBit = 0;
            int wantsBit = 0;

            // if there are more "wants" bits, make sure gives is selected first,
            // because then it's always possible to generate wants.
            if (numWantsBits > numGivesBits)
            {
                if (gives != ResourceType.None)
                {
                    givesBit = Utils.SelectRandomBitFrom((int)gives, ResourceTypeExtensions.RESOURCE_TYPE_COUNT, rand);
                }
                if (wants != ResourceType.None)
                {
                    int allowedWants = (int)wants & ~givesBit;
                    Assert.IsTrue(Utils.NumSetBits((uint)allowedWants) > 0, "cannot generate wants == gives");
                    wantsBit = Utils.SelectRandomBitFrom(allowedWants, ResourceTypeExtensions.RESOURCE_TYPE_COUNT, rand);
                }
            }
            else
            {
                if (wants != ResourceType.None)
                {
                   wantsBit = Utils.SelectRandomBitFrom((int)wants, ResourceTypeExtensions.RESOURCE_TYPE_COUNT, rand);
                }
                if (gives != ResourceType.None)
                {
                    int allowedGives = (int)gives & ~wantsBit;
                    Assert.IsTrue(Utils.NumSetBits((uint)allowedGives) > 0, "cannot generate wants == gives");
                    givesBit = Utils.SelectRandomBitFrom(allowedGives, ResourceTypeExtensions.RESOURCE_TYPE_COUNT, rand);
                }
            }

            return new StationData(
                (ResourceType)wantsBit, rand.NextInt(wantsAmount.x, wantsAmount.y + 1),
                (ResourceType)givesBit, rand.NextInt(givesAmount.x, givesAmount.y + 1)
            );
        }

        protected bool Equals(StationDataBlueprint other)
        {
            return wants == other.wants && wantsAmount.Equals(other.wantsAmount) && gives == other.gives && givesAmount.Equals(other.givesAmount);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StationDataBlueprint)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Should not be hashed");
        }
    }
}
