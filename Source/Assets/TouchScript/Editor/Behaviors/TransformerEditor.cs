/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Behaviors;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editorr.Behaviors
{
    [CustomEditor(typeof(Transformer), true)]
    internal class TransformerEditor : UnityEditor.Editor
    {
        private static readonly GUIContent TEXT_ENABLE_SMOOTHING = new GUIContent("Smoothing", "Applies smoothing to transform actions. This allows to reduce jagged movements but adds some visual lag.");
        private static readonly GUIContent TEXT_SMOOTHING_FACTOR = new GUIContent("Factor", "Indicates how much smoothing to apply. 0 - no smoothing, 10000 - maximum.");
        private static readonly GUIContent TEXT_POSITION_THRESHOLD = new GUIContent("Position Threshold", "Minimum distance between target position and smoothed position when to stop automatic movement.");
        private static readonly GUIContent TEXT_ROTATION_THRESHOLD = new GUIContent("Rotation Threshold", "Minimum angle between target rotation and smoothed rotation when to stop automatic movement.");
        private static readonly GUIContent TEXT_SCALE_THRESHOLD = new GUIContent("Scale Threshold", "Minimum difference between target scale and smoothed scale when to stop automatic movement.");
        private static readonly GUIContent TEXT_ALLOW_CHANGING = new GUIContent("Allow Changing From Outside", "Indicates if this transform can be changed from another script.");

        private SerializedProperty enableSmoothing;
        private SerializedProperty smoothingFactor;
        private SerializedProperty positionThreshold;
        private SerializedProperty rotationThreshold;
        private SerializedProperty scaleThreshold;
        private SerializedProperty allowChangingFromOutside;
        private Transformer instance;

        protected virtual void OnEnable()
        {
            enableSmoothing = serializedObject.FindProperty("enableSmoothing");
//            smoothingFactor = serializedObject.FindProperty("smoothingFactor");
//            positionThreshold = serializedObject.FindProperty("positionThreshold");
//            rotationThreshold = serializedObject.FindProperty("rotationThreshold");
//            scaleThreshold = serializedObject.FindProperty("scaleThreshold");
            allowChangingFromOutside = serializedObject.FindProperty("allowChangingFromOutside");

            instance = target as Transformer;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(enableSmoothing, TEXT_ENABLE_SMOOTHING);
            if (enableSmoothing.boolValue)
            {
                instance.SmoothingFactor = EditorGUILayout.FloatField(TEXT_SMOOTHING_FACTOR, instance.SmoothingFactor);
                instance.PositionThreshold = EditorGUILayout.FloatField(TEXT_POSITION_THRESHOLD, instance.PositionThreshold);
                instance.RotationThreshold = EditorGUILayout.FloatField(TEXT_ROTATION_THRESHOLD, instance.RotationThreshold);
                instance.ScaleThreshold = EditorGUILayout.FloatField(TEXT_SCALE_THRESHOLD, instance.ScaleThreshold);
                EditorGUILayout.PropertyField(allowChangingFromOutside, TEXT_ALLOW_CHANGING);
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}
