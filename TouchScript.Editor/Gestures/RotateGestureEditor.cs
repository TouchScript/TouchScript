using System;
using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures {

    [CustomEditor(typeof(RotateGesture))]
    public class RotateGestureEditor : Transform2DGestureBaseEditor {
        public override void OnInspectorGUI() {
            var instance = target as RotateGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();
            instance.RotationThreshold = EditorGUILayout.FloatField("Rotation threshold", instance.RotationThreshold);
            instance.MinClusterDistance = EditorGUILayout.FloatField("Min cluster distance", instance.MinClusterDistance);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
