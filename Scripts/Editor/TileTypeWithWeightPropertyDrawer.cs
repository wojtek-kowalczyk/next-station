using NS.Core.Blueprints;
using UnityEditor;
using UnityEngine;

namespace NS.Editor
{
    [CustomPropertyDrawer(typeof(HexTileDataBlueprint.TileTypeWithWeight))]
    public class TileTypeWithWeightPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            const int typeRectWidth = 200;
            var typeRect = new Rect(position.x, position.y, typeRectWidth, position.height);
            var weightRect = new Rect(position.x + typeRectWidth, position.y, position.width - typeRectWidth, position.height);

            // pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
