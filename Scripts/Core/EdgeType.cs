using System.Linq;

namespace NS.Core
{
    public enum EdgeType : byte
    {
        Basic,
        Mountain,
        Fields,
        Town,
        Rail,
    }

    public static class EdgeTypeExtensions
    {
        public static EdgeType[] CanMakeDistrictTypes { get; } = { EdgeType.Mountain, EdgeType.Fields, EdgeType.Town };

        public static bool CanMakeDistrict(this EdgeType edgeType)
        {
            return CanMakeDistrictTypes.Contains(edgeType);
        }
    }
}
