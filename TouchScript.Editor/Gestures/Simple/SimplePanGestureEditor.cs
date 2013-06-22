/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(SimplePanGesture))]
    public class SimplePanGestureEditor : Transform2DGestureBaseEditor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as SimplePanGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Movement Threshold (cm)", GUILayout.MinWidth(200));
            instance.MovementThreshold = EditorGUILayout.FloatField("", instance.MovementThreshold, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Minimum distance in cm for cluster to move to be considered as a possible gesture.", MessageType.Info, true);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}