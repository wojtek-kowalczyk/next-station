using NS.Core.Blueprints;
using NS.Visuals;
using UnityEngine;

namespace NS.Config
{
    [CreateAssetMenu(fileName = "gameConfig", menuName = "Game/Config/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private TileStackBlueprint dummyTileStackBlueprint;
        [SerializeField] private TileDecorationConfig decorationsConfig;

        public bool IsInitialized { get; private set; }

        public TileStackBlueprint DummyTileStackBlueprint => dummyTileStackBlueprint;
        public TileDecorationConfig Decorations => decorationsConfig;

        public void Initialize()
        {
            decorationsConfig.Initialize();

            IsInitialized = true;
        }
    }
}
