/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(FlickGesture))]
    public class FlickGestureEditor : GestureEditor
    {
        public const string TEXT_DIRECTION = "Flick direction.";

        public const string TEXT_MOVEMENTTHRESHOLD = "Minimum distance in cm touch points must move for the gesture to begin.";

        public const string TEXT_FLICKTIME = "Time interval in seconds during which touch points must move by <Minimum Distance> for the gesture to be recognized.";

        public const string TEXT_MINDISTANCE = "Minimum distance in cm touch points must move in <Flick Time> seconds for the gesture to be recognized.";

        private SerializedProperty direction;

        private SerializedProperty flickTime, minDistance, movementThreshold;

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

            EditorGUILayout.PropertyField(direction, new GUIContent("Direction", TEXT_DIRECTION));
            EditorGUILayout.PropertyField(movementThreshold, new GUIContent("Movement Threshold (cm)", TEXT_MOVEMENTTHRESHOLD));
            EditorGUILayout.PropertyField(flickTime, new GUIContent("Flick Time (sec)", TEXT_FLICKTIME));
            EditorGUILayout.PropertyField(minDistance, new GUIContent("Minimum Distance (cm)", TEXT_MINDISTANCE));

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}