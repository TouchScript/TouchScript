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

		public static readonly GUIContent TEXT_SETTINGS_HEADER = new GUIContent("Pointer settings", "General pointersettings.");
		public static readonly GUIContent TEXT_DPI_HEADER = new GUIContent("Use DPI", "Scale touch pointer based on DPI.");
		public static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced settings.");

		public static readonly GUIContent TEXT_POINTER_ID = new GUIContent("Show Pointer Id", "Display pointer id.");
		public static readonly GUIContent TEXT_POINTER_FLAGS = new GUIContent("Show Pointer Flags", "Display pointer flags.");
		public static readonly GUIContent TEXT_POINTER_SIZE = new GUIContent("Pointer size (cm)", "Pointer size in cm based on current DPI.");

        private SerializedProperty touchProxy, useDPI, touchSize, showTouchId, showFlags;
		private SerializedProperty generalProps, advancedProps;

        private void OnEnable()
        {
            showTouchId = serializedObject.FindProperty("showPointerId");
            showFlags = serializedObject.FindProperty("showFlags");
            touchProxy = serializedObject.FindProperty("pointerProxy");
            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("pointerSize");

			generalProps = serializedObject.FindProperty("generalProps");
			advancedProps = serializedObject.FindProperty("advancedProps");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

			GUILayout.Space(5);

			var display = GUIElements.Header(TEXT_SETTINGS_HEADER, generalProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(showTouchId, TEXT_POINTER_ID);
				EditorGUILayout.PropertyField(showFlags, TEXT_POINTER_FLAGS);
				EditorGUI.indentLevel--;
			}

			display = GUIElements.Header(TEXT_DPI_HEADER, useDPI, useDPI);
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
				EditorGUILayout.PropertyField(touchProxy, new GUIContent("Pointer Proxy"));
				EditorGUI.indentLevel--;
			}

            serializedObject.ApplyModifiedProperties();
        }
    }
}
