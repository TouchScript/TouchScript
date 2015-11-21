
using TouchScript.Behaviors;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Behaviors
{

    /*[CustomEditor(typeof(TouchVisualizer))]
	internal sealed class TouchVisualizerEditor : UnityEditor.Editor
    {

		private TouchVisualizer instance;
        private SerializedProperty texture, useDPI, touchSize, showTouchId, showTags;

        private void OnEnable()
        {
			instance = target as TouchVisualizer;
            showTouchId = serializedObject.FindProperty("showTouchId");
            showTags = serializedObject.FindProperty("showTags");
            texture = serializedObject.FindProperty("texture");
            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("touchSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(texture, new GUIContent("Touch Texture"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                instance.TouchTexture = texture.objectReferenceValue as Texture2D;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useDPI, new GUIContent("Use DPI"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                instance.UseDPI = useDPI.boolValue;
            }

            if (useDPI.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(touchSize, new GUIContent("Touch Size"));
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    instance.TouchSize = touchSize.floatValue;
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showTouchId, new GUIContent("Show Touch Id"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                instance.ShowTouchId = showTouchId.boolValue;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showTags, new GUIContent("Show Tags"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                instance.ShowTags = showTags.boolValue;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }*/
}
