/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(FlickGesture), true)]
    internal sealed class FlickGestureEditor : GestureEditor
    {
        private static readonly GUIContent DIRECTION = new GUIContent("Direction", "Flick direction.");
        private static readonly GUIContent MOVEMENT_THRESHOLD = new GUIContent("Movement Threshold (cm)", "Minimum distance in cm touch points must move for the gesture to begin.");
        private static readonly GUIContent FLICK_TIME = new GUIContent("Flick Time (sec)", "Time interval in seconds during which touch points must move by <Minimum Distance> for the gesture to be recognized.");
        private static readonly GUIContent MIN_DISTANCE = new GUIContent("Minimum Distance (cm)", "Minimum distance in cm touch points must move in <Flick Time> seconds for the gesture to be recognized.");

        private SerializedProperty direction;
        private SerializedProperty flickTime;
        private SerializedProperty minDistance;
        private SerializedProperty movementThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            flickTime = serializedObject.FindProperty("flickTime");
            minDistance = serializedObject.FindProperty("minDistance");
            movementThreshold = serializedObject.FindProperty("movementThreshold");
            direction = serializedObject.FindProperty("direction");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 180;
            EditorGUILayout.PropertyField(direction, DIRECTION);
            EditorGUILayout.PropertyField(movementThreshold, MOVEMENT_THRESHOLD);
            EditorGUILayout.PropertyField(flickTime, FLICK_TIME);
            EditorGUILayout.PropertyField(minDistance, MIN_DISTANCE);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
