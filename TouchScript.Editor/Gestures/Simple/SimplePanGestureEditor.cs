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
        private static readonly GUIContent NUMBER_OF_TOUCHES_LIMITED = new GUIContent("Number Of Touches Limited", "Number of touches limited for this gesture to be recognized and failed.");

        private SerializedProperty movementThreshold;
        private SerializedProperty numberOfTouchesLimited;

        protected override void OnEnable()
        {
            base.OnEnable();

            movementThreshold = serializedObject.FindProperty("movementThreshold");
            numberOfTouchesLimited = serializedObject.FindProperty("numberOfTouchesLimited");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(movementThreshold, MOVEMENT_THRESHOLD);
            EditorGUILayout.PropertyField(numberOfTouchesLimited, NUMBER_OF_TOUCHES_LIMITED);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
