/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(TapGesture), true)]
    public class TapGestureEditor : GestureEditor
    {
        public const string TEXT_TIMELIMIT = "Gesture fails if it is being pressed for more than <Value> seconds.";
        public const string TEXT_DISTANCELIMIT = "Gesture fails if fingers move more than <Value> cm.";

        private SerializedProperty distanceLimit;
        private SerializedProperty timeLimit;
        private bool useDistanceLimit;
        private bool useTimeLimit;

        protected override void OnEnable()
        {
            base.OnEnable();

            timeLimit = serializedObject.FindProperty("timeLimit");
            distanceLimit = serializedObject.FindProperty("distanceLimit");

            useTimeLimit = !float.IsInfinity(timeLimit.floatValue);
            useDistanceLimit = !float.IsInfinity(distanceLimit.floatValue);

            shouldDrawCombineTouchPoints = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            bool newTimelimit = GUILayout.Toggle(useTimeLimit, new GUIContent("Limit Press Time", TEXT_TIMELIMIT));
            if (newTimelimit)
            {
                if (newTimelimit != useTimeLimit) timeLimit.floatValue = 0;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(timeLimit, new GUIContent("Value (sec)", TEXT_TIMELIMIT));
                EditorGUI.indentLevel--;
            } else
            {
                timeLimit.floatValue = float.PositiveInfinity;
            }
            useTimeLimit = newTimelimit;

            bool newDistanceLimit = GUILayout.Toggle(useDistanceLimit,
                new GUIContent("Limit Movement", TEXT_DISTANCELIMIT));
            if (newDistanceLimit)
            {
                if (newDistanceLimit != useDistanceLimit) distanceLimit.floatValue = 0;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(distanceLimit, new GUIContent("Value (cm)", TEXT_DISTANCELIMIT));
                EditorGUI.indentLevel--;
            } else
            {
                distanceLimit.floatValue = float.PositiveInfinity;
            }
            useDistanceLimit = newDistanceLimit;

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}