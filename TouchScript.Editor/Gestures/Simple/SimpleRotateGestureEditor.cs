/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimpleRotateGesture))]
    public class SimpleRotateGestureEditor : Transform2DGestureBaseEditor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as SimpleRotateGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rotation Threshold (deg)", GUILayout.MinWidth(200));
            instance.RotationThreshold = EditorGUILayout.FloatField("", instance.RotationThreshold, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum rotation in degrees for gesture to be considered possible.", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Cluster Distance (cm)", GUILayout.MinWidth(200));
            instance.MinPointDistance = EditorGUILayout.FloatField("", instance.MinPointDistance, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum distance between clusters (fingers) in cm for gesture to be recognized.", MessageType.Info, true);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}