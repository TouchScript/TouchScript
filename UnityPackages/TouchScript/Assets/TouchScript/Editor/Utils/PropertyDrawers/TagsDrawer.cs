using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TouchScript.Editor.Utils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Tags))]
    internal sealed class TagsDrawer : PropertyDrawer
    {
        private ReorderableList list;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (list == null) initList(property, label);

            return list.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (list == null) initList(property, label);

            list.serializedProperty = property.FindPropertyRelative("tagList");
            list.DoList(position);
        }

        private void initList(SerializedProperty property, GUIContent label)
        {
            list = new ReorderableList(property.serializedObject, property.FindPropertyRelative("tagList"), false, true, true, true);
            list.drawHeaderCallback += rect => GUI.Label(rect, label);
            list.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;
                EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none);
            };
        }
    }
}
