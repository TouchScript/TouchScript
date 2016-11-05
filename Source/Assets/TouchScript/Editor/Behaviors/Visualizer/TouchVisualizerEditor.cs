/*
 * @author Valentin Simonov / http://va.lent.in/
 */
 
using TouchScript.Behaviors.Visualizer;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Behaviors.Visualizer
{

    [CustomEditor(typeof(PointerVisualizer))]
	internal sealed class TouchVisualizerEditor : UnityEditor.Editor
    {

        private SerializedProperty touchProxy, useDPI, touchSize, showTouchId, showFlags;

        private void OnEnable()
        {
            showTouchId = serializedObject.FindProperty("showPointerId");
            showFlags = serializedObject.FindProperty("showFlags");
            touchProxy = serializedObject.FindProperty("pointerProxy");
            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("pointerSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(touchProxy, new GUIContent("Pointer Proxy"));
            EditorGUILayout.PropertyField(showTouchId, new GUIContent("Show Pointer Id"));
            EditorGUILayout.PropertyField(showFlags, new GUIContent("Show Flags"));

            EditorGUILayout.PropertyField(useDPI, new GUIContent("Use DPI"));
            if (useDPI.boolValue)
            {
                EditorGUILayout.PropertyField(touchSize, new GUIContent("Pointer Size (cm)"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
