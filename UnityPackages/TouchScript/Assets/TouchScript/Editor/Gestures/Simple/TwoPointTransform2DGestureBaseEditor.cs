/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(TwoPointTransform2DGestureBase), true)]
    internal class TwoPointTransform2DGestureBaseEditor : Transform2DGestureBaseEditor
    {
        private static readonly GUIContent MIN_POINTS_DISTANCE = new GUIContent("Min Points Distance (cm)", "Minimum distance between two points (clusters) in cm to consider this gesture started. Used to prevent fake touch points spawned near real ones on cheap multitouch hardware to mess everything up.");

        private SerializedProperty minPointsDistance;

        protected override void OnEnable()
        {
            base.OnEnable();

            minPointsDistance = serializedObject.FindProperty("minPointsDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(minPointsDistance, MIN_POINTS_DISTANCE);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
