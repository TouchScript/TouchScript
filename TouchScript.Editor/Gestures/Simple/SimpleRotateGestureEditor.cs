/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleRotateGesture))]
    public class SimpleRotateGestureEditor : TwoPointTransform2DGestureBaseEditor
    {

        public const string TEXT_ROTATIONTHRESHOLD = "Minimum rotation in degrees for the gesture to begin.";

        private SerializedProperty rotationThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            rotationThreshold = serializedObject.FindProperty("rotationThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUIUtility.LookLikeInspector();

            EditorGUILayout.PropertyField(rotationThreshold, new GUIContent("Rotation Threshold (deg)", TEXT_ROTATIONTHRESHOLD));

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}