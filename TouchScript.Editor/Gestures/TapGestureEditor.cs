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
        public const string TEXT_NUMBEROFTAPSREQUIRED = "Number of taps required for this gesture to be recognized.";
        public const string TEXT_TIMELIMIT = "Gesture fails if in <value> seconds user didn't do the required number of taps.";
        public const string TEXT_DISTANCELIMIT = "Gesture fails if taps are made more than <value> cm away from the first touch position.";

        private enum NumberOfTaps
        {
            One = 1,
            Two = 2,
            Three = 3,
            More = 4
        }

        private SerializedProperty numberOfTapsRequired;
        private SerializedProperty distanceLimit;
        private SerializedProperty timeLimit;
        private bool useDistanceLimit;
        private bool useTimeLimit;

        protected override void OnEnable()
        {
            base.OnEnable();

            numberOfTapsRequired = serializedObject.FindProperty("numberOfTapsRequired");
            timeLimit = serializedObject.FindProperty("timeLimit");
            distanceLimit = serializedObject.FindProperty("distanceLimit");

            useTimeLimit = !float.IsInfinity(timeLimit.floatValue);
            useDistanceLimit = !float.IsInfinity(distanceLimit.floatValue);

            shouldDrawCombineTouchPoints = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            var value = numberOfTapsRequired.intValue;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Number of Taps Required", TEXT_NUMBEROFTAPSREQUIRED), GUILayout.ExpandWidth(true), GUILayout.MinWidth(160));
            if (value <= 3)
            {
                var numberOfTaps = value > 3 ? NumberOfTaps.More : (NumberOfTaps)value;
                value = (int)(NumberOfTaps)EditorGUILayout.EnumPopup(numberOfTaps, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
            } else
            {
                value = EditorGUILayout.IntField(value, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                numberOfTapsRequired.intValue = Mathf.Max(value, 1);
            }

            bool newTimelimit = GUILayout.Toggle(useTimeLimit, new GUIContent("Limit Time", TEXT_TIMELIMIT));
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