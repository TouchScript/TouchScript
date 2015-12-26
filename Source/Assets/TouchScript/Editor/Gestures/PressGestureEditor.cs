/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(PressGesture), true)]
    internal sealed class PressGestureEditor : GestureEditor
    {
        private static readonly GUIContent IGNORE_CHILDREN = new GUIContent("Ignore Children", "If selected this gesture ignores touch points from children.");

        private SerializedProperty ignoreChildren;

        protected override void OnEnable()
        {
            base.OnEnable();

            ignoreChildren = serializedObject.FindProperty("ignoreChildren");
        }

        protected override void drawAdvanced()
        {
            EditorGUILayout.PropertyField(ignoreChildren, IGNORE_CHILDREN);

            base.drawAdvanced();
        }
    }
}
