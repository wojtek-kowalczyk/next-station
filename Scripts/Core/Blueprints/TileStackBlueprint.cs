using System.Linq;
using UnityEngine;

namespace NS.Core.Blueprints
{
    [CreateAssetMenu(fileName = "new Tile Stack Blueprint", menuName = "Game/Tile Stack Blueprint")]
    public class TileStackBlueprint : ScriptableObject
    {
        [SerializeField]
        private int count = 20;

        [Tooltip("First couple of tiles will use this blueprint")]
        [SerializeField]
        private HexTileDataBlueprintSO initialBlueprint;

        [SerializeField]
        private HexTileDataBlueprintSO[] tileDataBlueprints;

        public TileStack Generate(int seed)
        {
            return new TileStack(
                count,
                tileDataBlueprints.Select(x => x.Blueprint),
                initialBlueprint.Blueprint,
                seed);
        }
    }
}
