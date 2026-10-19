using UnityEngine;

namespace NS.Core.Blueprints
{
    [CreateAssetMenu(fileName = "new Station Data Blueprint", menuName = "Game/Station Data Blueprint")]
    public class StationDataBlueprintSO : ScriptableObject
    {
        [SerializeField]
        private StationDataBlueprint blueprint;

        public StationDataBlueprint Blueprint => blueprint;
    }
}
