
using TouchScript.Behaviors.Visualizer;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Behaviors.Visualizer
{

    [CustomEditor(typeof(TouchVisualizer))]
	internal sealed class TouchVisualizerEditor : UnityEditor.Editor
    {

        private SerializedProperty touchProxy, useDPI, touchSize, showTouchId, showTags;

        private void OnEnable()
        {
            showTouchId = serializedObject.FindProperty("showTouchId");
            showTags = serializedObject.FindProperty("showTags");
            touchProxy = serializedObject.FindProperty("touchProxy");
            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("touchSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(touchProxy, new GUIContent("Touch Proxy"));
            EditorGUILayout.PropertyField(showTouchId, new GUIContent("Show Touch Id"));
            EditorGUILayout.PropertyField(showTags, new GUIContent("Show Tags"));

            EditorGUILayout.PropertyField(useDPI, new GUIContent("Use DPI"));
            if (useDPI.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(touchSize, new GUIContent("Touch Size (cm)"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
