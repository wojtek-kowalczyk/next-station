using System;
using System.Collections.Generic;

namespace NS.Core
{
    [Flags]
    public enum ResourceType
    {
        None = 0,

        People = 1 << 0,
        Wood = 1 << 1,
        Steel = 1 << 2,
        Gold = 1 << 3,

        All = ~0,
    }

    public static class ResourceTypeExtensions
    {
        // We need these sorts of fields, because when passing All flag it has all bits set, and
        // when we select random bit we need to truncate to only valid bits. Ugly, but I don't see another solution.
        public const int RESOURCE_TYPE_COUNT = 4;

        public static IEnumerable<ResourceType> ResourceTypes()
        {
            yield return ResourceType.People;
            yield return ResourceType.Wood;
            yield return ResourceType.Steel;
            yield return ResourceType.Gold;
        }
    }
}
