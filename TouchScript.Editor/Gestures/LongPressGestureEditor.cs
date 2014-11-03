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

        private static readonly GUIContent MAX_TOUCHES = new GUIContent("Limit Number of Touch Points", "Limit maximum number of simultaneous touch points.");
        private static readonly GUIContent TIME_TO_PRESS = new GUIContent("Time to Press (sec)", "Limit maximum number of simultaneous touch points.");
        private static readonly GUIContent DISTANCE_LIMIT = new GUIContent("Limit Movement (cm)", "Gesture fails if fingers move more than <Value> cm.");

        private SerializedProperty distanceLimit;
        private SerializedProperty maxTouches;
        private SerializedProperty timeToPress;

        protected override void OnEnable()
        {
            base.OnEnable();

            maxTouches = serializedObject.FindProperty("maxTouches");
            timeToPress = serializedObject.FindProperty("timeToPress");
            distanceLimit = serializedObject.FindProperty("distanceLimit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(timeToPress, TIME_TO_PRESS);
            EditorGUILayout.PropertyField(maxTouches, MAX_TOUCHES);
            EditorGUILayout.PropertyField(distanceLimit, DISTANCE_LIMIT);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
