/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(ScaleGesture))]
    public class ScaleGestureEditor : Transform2DGestureBaseEditor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as ScaleGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scaling Threshold (cm)", GUILayout.MinWidth(200));
            instance.ScalingThreshold = EditorGUILayout.FloatField("", instance.ScalingThreshold, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum distance in cm between clusters for gesture to be considered possible.", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Cluster Distance (cm)", GUILayout.MinWidth(200));
            instance.MinClusterDistance = EditorGUILayout.FloatField("", instance.MinClusterDistance, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum distance between clusters (fingers) in cm for gesture to be recognized.", MessageType.Info, true);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}