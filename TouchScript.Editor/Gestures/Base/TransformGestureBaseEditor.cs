/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Base
{
    internal class TransformGestureBaseEditor : GestureEditor
    {
        public static readonly GUIContent TYPE = new GUIContent("Transform Type", "Specifies what gestures should be detected: Translation, Rotation, Scaling.");
        public static readonly GUIContent TYPE_TRANSLATION = new GUIContent("Translation", "Dragging with one ore more fingers.");
        public static readonly GUIContent TYPE_ROTATION = new GUIContent("Rotation", "Rotating with two or more fingers.");
        public static readonly GUIContent TYPE_SCALING = new GUIContent("Scaling", "Scaling with two or more fingers.");
        public static readonly GUIContent MIN_SCREEN_POINTS_DISTANCE = new GUIContent("Min Points Distance (cm)", "Minimum distance between two points (clusters) in cm to consider this gesture started. Used to prevent fake touch points spawned near real ones on cheap multitouch hardware to mess everything up.");
        public static readonly GUIContent SCREEN_TRANSFORM_THRESHOLD = new GUIContent("Movement Threshold (cm)", "Minimum distance in cm touch points must move for the gesture to begin.");

        protected SerializedProperty type;
        protected SerializedProperty minScreenPointsDistance;
        protected SerializedProperty screenTransformThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            type = serializedObject.FindProperty("type");
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
                newType |= (int)TransformGesture.TransformType.Translation;
            EditorGUI.indentLevel--;
            if (EditorGUILayout.ToggleLeft(TYPE_ROTATION,
                (typeValue & (int)TransformGesture.TransformType.Rotation) != 0, GUILayout.Width(70)))
                newType |= (int)TransformGesture.TransformType.Rotation;
            if (EditorGUILayout.ToggleLeft(TYPE_SCALING,
                (typeValue & (int)TransformGesture.TransformType.Scaling) != 0, GUILayout.Width(70)))
                newType |= (int)TransformGesture.TransformType.Scaling;
            type.intValue = newType;
            EditorGUILayout.EndHorizontal();

            doInspectorGUI();

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        protected virtual void doInspectorGUI() {}

        protected override void drawAdvanced()
        {
            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(minScreenPointsDistance, MIN_SCREEN_POINTS_DISTANCE);
            EditorGUILayout.PropertyField(screenTransformThreshold, SCREEN_TRANSFORM_THRESHOLD);

            base.drawAdvanced();
        }
    }
}
