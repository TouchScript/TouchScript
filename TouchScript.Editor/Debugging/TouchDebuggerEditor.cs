
using TouchScript.Debugging;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Debugging
{

    [CustomEditor(typeof(TouchDebugger))]
    internal sealed class TouchDebuggerEditor : UnityEditor.Editor
    {

        private TouchDebugger instance;
        private SerializedProperty texture, useDPI, touchSize;

        private void OnEnable()
        {
            instance = target as TouchDebugger;
            texture = serializedObject.FindProperty("texture");
            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("touchSize");
        }

        public override void OnInspectorGUI()
        {
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
