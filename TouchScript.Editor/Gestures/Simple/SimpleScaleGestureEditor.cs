/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleScaleGesture), true)]
    internal sealed class SimpleScaleGestureEditor : TwoPointTransform2DGestureBaseEditor
    {
        private static readonly GUIContent SCALING_THRESHOLD = new GUIContent("Scaling Threshold (cm)", "Minimum distance in cm touch points must move for the gesture to begin.");

        private SerializedProperty scalingThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            scalingThreshold = serializedObject.FindProperty("scalingThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(scalingThreshold, SCALING_THRESHOLD);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}