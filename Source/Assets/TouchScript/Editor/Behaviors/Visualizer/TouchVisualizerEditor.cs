/*
 * @author Valentin Simonov / http://va.lent.in/
 */
 
using TouchScript.Behaviors.Visualizer;
using UnityEditor;
using UnityEngine;
using TouchScript.Editor.Utils;

namespace TouchScript.Editor.Behaviors.Visualizer
{

    [CustomEditor(typeof(PointerVisualizer))]
	internal sealed class TouchVisualizerEditor : UnityEditor.Editor
    {

		public static readonly GUIContent TEXT_DPI_HEADER = new GUIContent("Use DPI", "Scale touch pointer based on DPI.");
		public static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced settings.");
		public static readonly GUIContent TEXT_POINTER_SIZE = new GUIContent("Pointer size (cm)", "Pointer size in cm based on current DPI.");

        private SerializedProperty mousePointerProxy, touchPointerProxy, penPointerProxy, objectPointerProxy;
        private SerializedProperty useDPI, touchSize;
		private SerializedProperty advancedProps;

        private void OnEnable()
        {
            mousePointerProxy = serializedObject.FindProperty("mousePointerProxy");
            touchPointerProxy = serializedObject.FindProperty("touchPointerProxy");
            penPointerProxy = serializedObject.FindProperty("penPointerProxy");
            objectPointerProxy = serializedObject.FindProperty("objectPointerProxy");

            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("pointerSize");

			advancedProps = serializedObject.FindProperty("advancedProps");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

			GUILayout.Space(5);

			var display = GUIElements.Header(TEXT_DPI_HEADER, useDPI, useDPI);
			if (display)
			{
				EditorGUI.indentLevel++;
				using (new EditorGUI.DisabledGroupScope(!useDPI.boolValue))
				{
					EditorGUILayout.PropertyField(touchSize, TEXT_POINTER_SIZE);
				}
				EditorGUI.indentLevel--;
			}

			display = GUIElements.Header(TEXT_ADVANCED_HEADER, advancedProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(mousePointerProxy, new GUIContent("Mouse Pointer Proxy"));
                EditorGUILayout.PropertyField(touchPointerProxy, new GUIContent("Touch Pointer Proxy"));
                EditorGUILayout.PropertyField(penPointerProxy, new GUIContent("Pen Pointer Proxy"));
                EditorGUILayout.PropertyField(objectPointerProxy, new GUIContent("Object Pointer Proxy"));
                EditorGUI.indentLevel--;
			}

            serializedObject.ApplyModifiedProperties();
        }
    }
}
