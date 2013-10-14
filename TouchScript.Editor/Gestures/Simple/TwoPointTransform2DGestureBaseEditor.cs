using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    public class TwoPointTransform2DGestureBaseEditor : Transform2DGestureBaseEditor
    {

        public const string TEXT_MINPOINTSDISTANCE = "Minimum distance between two points (clusters) in cm to consider this gesture started. Used to prevent fake touch points spawned near real ones on cheap multitouch hardware to mess everything up.";

        private SerializedProperty minPointsDistance;

        protected override void OnEnable()
        {
            base.OnEnable();

            minPointsDistance = serializedObject.FindProperty("minPointsDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUIUtility.LookLikeInspector();

            EditorGUILayout.PropertyField(minPointsDistance, new GUIContent("Min Points Distance (cm)", TEXT_MINPOINTSDISTANCE));

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}
