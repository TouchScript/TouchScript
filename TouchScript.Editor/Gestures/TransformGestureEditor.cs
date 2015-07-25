/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(TransformGesture), true)]
    internal class TransformGestureEditor : GestureEditor
    {
        private static readonly GUIContent TYPE = new GUIContent("Transform Type", "Specifies what gestures should be detected: Translation, Rotation, Scaling.");
        private static readonly GUIContent TYPE_TRANSLATION = new GUIContent("Translation", "Dragging with one ore more fingers.");
        private static readonly GUIContent TYPE_ROTATION = new GUIContent("Rotation", "Rotating with two or more fingers.");
        private static readonly GUIContent TYPE_SCALING = new GUIContent("Scaling", "Scaling with two or more fingers.");
        private static readonly GUIContent PROJECTION = new GUIContent("Projection Type", "Method used to project 2d screen positions of touch points into 3d space.");
        private static readonly GUIContent PROJECTION_NORMAL = new GUIContent("Projection Normal", "Normal of the plane in 3d space where touch points' positions are projected.");
        private static readonly GUIContent MIN_SCREEN_POINTS_DISTANCE = new GUIContent("Min Points Distance (cm)", "Minimum distance between two points (clusters) in cm to consider this gesture started. Used to prevent fake touch points spawned near real ones on cheap multitouch hardware to mess everything up.");
        private static readonly GUIContent SCREEN_TRANSFORM_THRESHOLD = new GUIContent("Movement Threshold (cm)", "Minimum distance in cm touch points must move for the gesture to begin.");

        private SerializedProperty type;
        private SerializedProperty projection;
        private SerializedProperty projectionNormal;
        private SerializedProperty minScreenPointsDistance;
        private SerializedProperty screenTransformThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            type = serializedObject.FindProperty("type");
            projection = serializedObject.FindProperty("projection");
            projectionNormal = serializedObject.FindProperty("projectionNormal");
            minScreenPointsDistance = serializedObject.FindProperty("minScreenPointsDistance");
            screenTransformThreshold = serializedObject.FindProperty("screenTransformThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            var typeValue = type.intValue;
            int newType = 0;
            EditorGUILayout.LabelField(TYPE);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.ToggleLeft(TYPE_TRANSLATION,
                (typeValue & (int)TransformGesture.TransformType.Translation) != 0, GUILayout.Width(100)))
                newType |= (int) TransformGesture.TransformType.Translation;
            EditorGUI.indentLevel--;
            if (EditorGUILayout.ToggleLeft(TYPE_ROTATION,
                (typeValue & (int)TransformGesture.TransformType.Rotation) != 0, GUILayout.Width(70)))
                newType |= (int)TransformGesture.TransformType.Rotation;
            if (EditorGUILayout.ToggleLeft(TYPE_SCALING,
                (typeValue & (int)TransformGesture.TransformType.Scaling) != 0, GUILayout.Width(70)))
                newType |= (int)TransformGesture.TransformType.Scaling;
            type.intValue = newType;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(projection, PROJECTION);
            if (projection.enumValueIndex != (int)TransformGesture.ProjectionType.Layer &&
                projection.enumValueIndex != (int)TransformGesture.ProjectionType.Screen)
            {
                EditorGUILayout.PropertyField(projectionNormal, PROJECTION_NORMAL);
            }

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(minScreenPointsDistance, MIN_SCREEN_POINTS_DISTANCE);
            EditorGUILayout.PropertyField(screenTransformThreshold, SCREEN_TRANSFORM_THRESHOLD);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}
