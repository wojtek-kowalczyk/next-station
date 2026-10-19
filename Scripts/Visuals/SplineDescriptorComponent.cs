using UnityEngine;

namespace NS.Visuals
{
    public class SplineDescriptorComponent : MonoBehaviour
    {
        [field: SerializeField]
        public SplineDescriptor[] Descriptors { get; private set; }
    }
}
