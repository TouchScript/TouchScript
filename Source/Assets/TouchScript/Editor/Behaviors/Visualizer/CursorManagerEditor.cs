/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Behaviors.Cursors;
using UnityEditor;
using UnityEngine;
using TouchScript.Editor.Utils;

namespace TouchScript.Editor.Behaviors.Visualizer
{

    [CustomEditor(typeof(CursorManager))]
	internal sealed class CursorManagerEditor : UnityEditor.Editor
    {

		public static readonly GUIContent TEXT_DPI_HEADER = new GUIContent("Use DPI", "Scale touch pointer based on DPI.");
		public static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced settings.");
		public static readonly GUIContent TEXT_POINTER_SIZE = new GUIContent("Pointer size (cm)", "Pointer size in cm based on current DPI.");

        private SerializedProperty mousePointerProxy, touchPointerProxy, penPointerProxy, objectPointerProxy;
        private SerializedProperty useDPI, touchSize;
		private SerializedProperty advancedProps;

        private void OnEnable()
        {
            mousePointerProxy = serializedObject.FindProperty("mouseCursor");
            touchPointerProxy = serializedObject.FindProperty("touchCursor");
            penPointerProxy = serializedObject.FindProperty("penCursor");
            objectPointerProxy = serializedObject.FindProperty("objectCursor");

            useDPI = serializedObject.FindProperty("useDPI");
            touchSize = serializedObject.FindProperty("cursorSize");

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
