using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Tags))]
    internal sealed class TagsDrawer : PropertyDrawer
    {

        private string newTag = "";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                var tagsProp = property.FindPropertyRelative("tagList");
                return (tagsProp.arraySize + 2) * EditorGUIUtility.singleLineHeight;
            } else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;
            var tagsProp = property.FindPropertyRelative("tagList");
            var size = tagsProp.arraySize;

            if (size > 0)
            {
                var tags = " (";
                for (var i = 0; i < size; i++)
                {
                    tags += tagsProp.GetArrayElementAtIndex(i).stringValue;
                    if (i < size - 1) tags += ", ";
                    else tags += ")";
                }
                label.text += tags;
            }
            label = EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                rect.x += EditorGUI.indentLevel*18;

                var btnRect = rect;
                btnRect.width = 30;
                rect.width = Mathf.Min(rect.width - 30, 140);
                btnRect.x += rect.width;

                var removeId = -1;
                for (var i = 0; i < size; i++)
                {
                    rect.y += EditorGUIUtility.singleLineHeight;
                    btnRect.y += EditorGUIUtility.singleLineHeight;
                    GUI.Label(rect, tagsProp.GetArrayElementAtIndex(i).stringValue);
                    if (GUI.Button(btnRect, "-"))
                    {
                        removeId = i;
                    }
                }

                rect.y += EditorGUIUtility.singleLineHeight;
                btnRect.y += EditorGUIUtility.singleLineHeight;
                newTag = GUI.TextField(rect, newTag);
                if (newTag == "") GUI.enabled = false;
                if (GUI.Button(btnRect, "+"))
                {
                    tagsProp.InsertArrayElementAtIndex(size);
                    tagsProp.GetArrayElementAtIndex(size).stringValue = newTag;
                    newTag = "";
                }
                GUI.enabled = true;

                if (removeId > -1)
                {
                    tagsProp.DeleteArrayElementAtIndex(removeId);
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
