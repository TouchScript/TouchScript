using TouchScript.Utils.Editor.Attributes;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(ToggleLeftAttribute))]
    public class ToggleLeftDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var boolValue = EditorGUI.ToggleLeft(position, label, property.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = boolValue;
            }
            EditorGUI.EndProperty();
        }
    }

}
