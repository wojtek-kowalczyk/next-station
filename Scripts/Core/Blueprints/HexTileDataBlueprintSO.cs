using UnityEngine;

namespace NS.Core.Blueprints
{
    [CreateAssetMenu(fileName = "new Tile Data Blueprint", menuName = "Game/Tile Data Blueprint")]
    public class HexTileDataBlueprintSO : ScriptableObject
    {
        [SerializeField]
        private HexTileDataBlueprint blueprint;

        [SerializeField]
        private StationDataBlueprintSO stationDataBlueprint;

        public HexTileDataBlueprint Blueprint => new(blueprint.TileTypes, blueprint.EdgeTypes, stationDataBlueprint?.Blueprint);

        private void OnValidate()
        {
            var err = blueprint.Validate();
            if (err != HexTileDataBlueprint.TileBlueprintSetupError.None)
            {
                Debug.LogError(err);
            }
        }
    }
}
