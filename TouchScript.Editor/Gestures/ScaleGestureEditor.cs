using System;
using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures {

    [CustomEditor(typeof(ScaleGesture))]
    public class ScaleGestureEditor : Transform2DGestureBaseEditor {
        public override void OnInspectorGUI() {
            var instance = target as ScaleGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();
            instance.ScalingThreshold = EditorGUILayout.FloatField("Scaling threshold", instance.ScalingThreshold);
            instance.MinClusterDistance = EditorGUILayout.FloatField("Min cluster distance", instance.MinClusterDistance);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}