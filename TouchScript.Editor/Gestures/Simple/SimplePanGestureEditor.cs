/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimplePanGesture), true)]
    internal sealed class SimplePanGestureEditor : Transform2DGestureBaseEditor
    {
        private static readonly GUIContent MOVEMENT_THRESHOLD = new GUIContent("Movement Threshold (cm)", "Minimum distance in cm touch points must move for the gesture to begin.");

        private SerializedProperty movementThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            movementThreshold = serializedObject.FindProperty("movementThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(movementThreshold, MOVEMENT_THRESHOLD);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
