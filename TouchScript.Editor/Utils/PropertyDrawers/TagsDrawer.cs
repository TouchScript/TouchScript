using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils.PropertyDrawers
{
    //[CustomPropertyDrawer(typeof(Tags))]
    internal sealed class TagsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            var list = property.FindPropertyRelative("tagList");
            GUI.Label(position, list.arraySize.ToString());
            EditorGUI.EndProperty();
        }
    }
}
