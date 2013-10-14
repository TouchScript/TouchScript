using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{

    [CustomEditor(typeof(LongPressGesture))]
    public class LongPressGestureEditor : GestureEditor
    {

        public const string TEXT_MAXTOUCHES = "Limit maximum number of simultaneous touch points.";
        public const string TEXT_TIMETOPRESS = "Total time in seconds required to hold touches for gesture to be recognized.";
        public const string TEXT_DISTANCELIMIT = "Gesture fails if fingers move more than <Value> cm.";

        private SerializedProperty maxTouches, timeToPress, distanceLimit;
        private bool useTouchesLimit, useDistanceLimit;

        protected override void OnEnable()
        {
            base.OnEnable();

            maxTouches = serializedObject.FindProperty("maxTouches");
            timeToPress = serializedObject.FindProperty("timeToPress");
            distanceLimit = serializedObject.FindProperty("distanceLimit");

            useTouchesLimit = maxTouches.intValue != int.MaxValue;
            useDistanceLimit = !float.IsInfinity(distanceLimit.floatValue);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUIUtility.LookLikeInspector();

            EditorGUILayout.PropertyField(timeToPress, new GUIContent("Time to Press (sec)", TEXT_TIMETOPRESS));

            var newToucheslimit = GUILayout.Toggle(useTouchesLimit, new GUIContent("Limit Number of Touch Points", TEXT_MAXTOUCHES));
            if (newToucheslimit)
            {
                if (newToucheslimit != useTouchesLimit) maxTouches.intValue = 1;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxTouches, new GUIContent("Value", TEXT_MAXTOUCHES));
                EditorGUI.indentLevel--;
            }
            else
            {
                maxTouches.intValue = int.MaxValue;
            }
            useTouchesLimit = newToucheslimit;

            var newDistanceLimit = GUILayout.Toggle(useDistanceLimit, new GUIContent("Limit Movement", TEXT_DISTANCELIMIT));
            if (newDistanceLimit)
            {
                if (newDistanceLimit != useDistanceLimit) distanceLimit.floatValue = 0;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(distanceLimit, new GUIContent("Value (cm)", TEXT_DISTANCELIMIT));
                EditorGUI.indentLevel--;
            }
            else
            {
                distanceLimit.floatValue = float.PositiveInfinity;
            }
            useDistanceLimit = newDistanceLimit;

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}
