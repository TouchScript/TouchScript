/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Layers
{

    [CustomEditor(typeof(UILayer))]
    internal sealed class UILayerEditor : UnityEditor.Editor
    {

        private static readonly GUIContent MODE = new GUIContent("Mode", "Determines layer behavior: (a) Layer - works as a touch layer using UI EventSystem to check if touch points hit any UI elements; (b) Proxy - works as a UI input module redirecting touch points to UI EventSystem.");
        private static readonly GUIContent Z_OFFSET = new GUIContent("Screen Space Z Offset", "Z offset used to cast a ray from a screen space canvas.");

        private SerializedProperty mode, zOffset;

        private void OnEnable()
        {
            mode = serializedObject.FindProperty("mode");
            zOffset = serializedObject.FindProperty("screenSpaceZOffset");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(mode, MODE);
            EditorGUILayout.PropertyField(zOffset, Z_OFFSET);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
