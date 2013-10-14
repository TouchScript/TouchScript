/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleScaleGesture))]
    public class SimpleScaleGestureEditor : TwoPointTransform2DGestureBaseEditor
    {

        public const string TEXT_SCALINGTHRESHOLD = "Minimum distance in cm touch points must move for the gesture to begin.";

        private SerializedProperty scalingThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            scalingThreshold = serializedObject.FindProperty("scalingThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUIUtility.LookLikeInspector();

            EditorGUILayout.PropertyField(scalingThreshold, new GUIContent("Scaling Threshold (cm)", TEXT_SCALINGTHRESHOLD));

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}