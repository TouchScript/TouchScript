
using TouchScript.Behaviors.Visualizer;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Behaviors.Visualizer
{

    [CustomEditor(typeof(PointerVisualizer))]
	internal sealed class TouchVisualizerEditor : UnityEditor.Editor
    {

        private SerializedProperty touchProxy, useDPI, touchSize, showTouchId, showTags;

        private void OnEnable()
        {
            showTouchId = serializedObject.FindProperty("showPointerId");
            showTags = serializedObject.FindProperty("showTags");
            touchProxy = serializedObject.FindProperty("pointerProxy");
            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("pointerSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(touchProxy, new GUIContent("Pointer Proxy"));
            EditorGUILayout.PropertyField(showTouchId, new GUIContent("Show Pointer Id"));
            EditorGUILayout.PropertyField(showTags, new GUIContent("Show Tags"));

            EditorGUILayout.PropertyField(useDPI, new GUIContent("Use DPI"));
            if (useDPI.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(touchSize, new GUIContent("Pointer Size (cm)"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
