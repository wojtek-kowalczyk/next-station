using UnityEngine;

namespace NS.Utility
{
    [CreateAssetMenu(fileName = "New ScriptableCurve", menuName = "Game/Scriptable Curve")]
    public class ScriptableCurve : ScriptableObject
    {
        [field: SerializeField]
        public AnimationCurve Curve { get; private set; }
    }
}
