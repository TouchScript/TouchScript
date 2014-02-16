/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleRotateGesture), true)]
    public class SimpleRotateGestureEditor : TwoPointTransform2DGestureBaseEditor
    {
        private static readonly GUIContent ROTATION_THRESHOLD = new GUIContent("Rotation Threshold (deg)", "Minimum rotation in degrees for the gesture to begin.");

        private SerializedProperty rotationThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            rotationThreshold = serializedObject.FindProperty("rotationThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(rotationThreshold, ROTATION_THRESHOLD);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}