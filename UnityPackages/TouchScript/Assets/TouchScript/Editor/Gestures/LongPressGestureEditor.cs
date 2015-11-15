/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(LongPressGesture), true)]
    internal sealed class LongPressGestureEditor : GestureEditor
    {
        private static readonly GUIContent TIME_TO_PRESS = new GUIContent("Time to Press (sec)", "Limit maximum number of simultaneous touch points.");
        private static readonly GUIContent DISTANCE_LIMIT = new GUIContent("Limit Movement (cm)", "Gesture fails if fingers move more than <Value> cm.");

        private SerializedProperty distanceLimit;
        private SerializedProperty timeToPress;

        protected override void OnEnable()
        {
            base.OnEnable();

            timeToPress = serializedObject.FindProperty("timeToPress");
            distanceLimit = serializedObject.FindProperty("distanceLimit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(timeToPress, TIME_TO_PRESS);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        protected override void drawAdvanced()
        {
            EditorGUILayout.PropertyField(distanceLimit, DISTANCE_LIMIT);

            base.drawAdvanced();
        }
    }
}
