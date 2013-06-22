/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleScaleGesture))]
    public class SimpleScaleGestureEditor : Transform2DGestureBaseEditor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as SimpleScaleGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scaling Threshold (cm)", GUILayout.MinWidth(200));
            instance.ScalingThreshold = EditorGUILayout.FloatField("", instance.ScalingThreshold, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum distance in cm between clusters for gesture to be considered possible.", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Point Distance (cm)", GUILayout.MinWidth(200));
            instance.MinPointDistance = EditorGUILayout.FloatField("", instance.MinPointDistance, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum distance between points (fingers) in cm for gesture to be recognized.", MessageType.Info, true);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}