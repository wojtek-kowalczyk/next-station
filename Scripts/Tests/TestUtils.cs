using MessagePack;
using NS.Core;
using NS.Core.Blueprints;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tests
{
    public static class TestUtils
    {
        public static readonly EdgeType[] BasicEdges =
            { EdgeType.Basic, EdgeType.Basic, EdgeType.Basic, EdgeType.Basic, EdgeType.Basic, EdgeType.Basic };

        public static readonly EdgeType[] FieldsEdges =
            { EdgeType.Fields, EdgeType.Fields, EdgeType.Fields, EdgeType.Fields, EdgeType.Fields, EdgeType.Fields };

        public static readonly EdgeType[] MixedEdges =
            { EdgeType.Fields, EdgeType.Mountain, EdgeType.Rail, EdgeType.Town, EdgeType.Basic, EdgeType.Basic };

        public static readonly StationData StationWantsWood = new(ResourceType.Wood, 1, ResourceType.None, 0);
        public static readonly StationData StationWantsSteel = new(ResourceType.Steel, 1, ResourceType.None, 0);
        public static readonly StationData StationGivesWood = new(ResourceType.None, 0, ResourceType.Wood, 1);
        public static readonly StationData StationGivesSteel = new(ResourceType.None, 0, ResourceType.Steel, 1);
        public static readonly StationData StationWantsWoodGivesSteel = new(ResourceType.Wood, 1, ResourceType.Steel, 1);

        public static readonly HexTileData TileA = new(MixedEdges, 125, 4, default);
        public static readonly HexTileData TileB = new(FieldsEdges, 0, 2, StationWantsWood);

        public static readonly TilePlacementData PlacementDataA = new(TileA, new Vector2Int(0, 0), 0, true);
        public static readonly TilePlacementData PlacementDataB = new(TileB, new Vector2Int(1, 2), 5, false);

        public static readonly StationDataBlueprint StationDataBlueprintA = new(
            ResourceType.Wood, new Vector2Int(1, 3),
                ResourceType.Steel | ResourceType.People, new Vector2Int(3, 5));

        public static readonly StationDataBlueprint StationDataBlueprintB = new(
            ResourceType.Wood, new Vector2Int(1, 2),
            ResourceType.Steel | ResourceType.People, new Vector2Int(3, 5));

        public static void TestSerialization<T>(T obj)
        {
            byte[] bytes = MessagePackSerializer.Serialize(obj);
            string json = MessagePackSerializer.ConvertToJson(bytes); // for debugger inspection
            var deserialized = MessagePackSerializer.Deserialize<T>(bytes);
            Assert.IsTrue(deserialized != null);
            Assert.IsTrue(obj.Equals(deserialized));
        }
    }
}
