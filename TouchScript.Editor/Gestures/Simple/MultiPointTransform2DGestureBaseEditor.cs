/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(MultiPointTransform2DGestureBase), true)]
    internal class MultiPointTransform2DGestureBaseEditor : Transform2DGestureBaseEditor
    {
        private static readonly GUIContent MIN_POINTS_COUNT = new GUIContent("Min Points Count", "Minimum points count for gesture to begin.");
        private static readonly GUIContent MIN_POINTS_DISTANCE = new GUIContent("Min Points Distance (cm)", "Minimum distance between two points (clusters) in cm to consider this gesture started. Used to prevent fake touch points spawned near real ones on cheap multitouch hardware to mess everything up.");

        private SerializedProperty minPointsCount;
        private SerializedProperty minPointsDistance;

        protected bool showMinPointsCount;

        protected override void OnEnable()
        {
            base.OnEnable();

            showMinPointsCount = true;

            minPointsCount = serializedObject.FindProperty("minPointsCount");
            minPointsDistance = serializedObject.FindProperty("minPointsDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 160;

            if(showMinPointsCount) {
                EditorGUILayout.PropertyField(minPointsCount, MIN_POINTS_COUNT);
            }
            EditorGUILayout.PropertyField(minPointsDistance, MIN_POINTS_DISTANCE);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
