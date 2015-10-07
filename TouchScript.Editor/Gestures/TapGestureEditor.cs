/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(TapGesture), true)]
    internal sealed class TapGestureEditor : GestureEditor
    {
        private static readonly GUIContent TIME_LIMIT = new GUIContent("Limit Time (sec)", "Gesture fails if in <value> seconds user didn't do the required number of taps.");
        private static readonly GUIContent DISTANCE_LIMIT = new GUIContent("Limit Movement (cm)", "Gesture fails if taps are made more than <value> cm away from the first touch position.");
        private static readonly GUIContent NUMBER_OF_TAPS_REQUIRED = new GUIContent("Number of Taps Required", "Number of taps required for this gesture to be recognized.");

        private SerializedProperty numberOfTapsRequired;
        private SerializedProperty distanceLimit;
        private SerializedProperty timeLimit;

        protected override void OnEnable()
        {
            base.OnEnable();

            numberOfTapsRequired = serializedObject.FindProperty("numberOfTapsRequired");
            timeLimit = serializedObject.FindProperty("timeLimit");
            distanceLimit = serializedObject.FindProperty("distanceLimit");

            shouldDrawCombineTouches = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUIUtility.labelWidth = 180;
            EditorGUILayout.IntPopup(numberOfTapsRequired, new[] {new GUIContent("One"), new GUIContent("Two"), new GUIContent("Three")}, new[] {1, 2, 3}, NUMBER_OF_TAPS_REQUIRED, GUILayout.ExpandWidth(true));

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        protected override void drawAdvanced()
        {
            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(timeLimit, TIME_LIMIT);
            EditorGUILayout.PropertyField(distanceLimit, DISTANCE_LIMIT);

            base.drawAdvanced();
        }
    }
}
