using NS.Core;
using UnityEditor;
using UnityEngine;

namespace NS.Editor
{
    [CustomPropertyDrawer(typeof(EdgeMask))]
    public class EdgeMaskPropertyDrawer : PropertyDrawer
    {
        private static readonly string[] DisplayOptions = { "1", "2", "3", "4", "5", "6" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            byte currentMask = ((EdgeMask)property.boxedValue).Mask;
            int newMask = EditorGUI.MaskField(position, label, currentMask, DisplayOptions);
            if (newMask != currentMask)
            {
                var edgeMask = new EdgeMask();
                edgeMask.Mask = (byte)newMask;
                property.boxedValue = edgeMask;
            }

            EditorGUI.EndProperty();
        }
    }
}
