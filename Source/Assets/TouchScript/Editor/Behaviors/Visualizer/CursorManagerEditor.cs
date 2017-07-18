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
        public static readonly GUIContent TEXT_POINTER_PIXEL_SIZE = new GUIContent("Pointer size (px)", "Pointer size in pixels.");

        private SerializedProperty mousePointerProxy, touchPointerProxy, penPointerProxy, objectPointerProxy;
        private SerializedProperty useDPI, cursorSize, cursorPixelSize;
		private SerializedProperty advancedProps;

        private void OnEnable()
        {
            mousePointerProxy = serializedObject.FindProperty("mouseCursor");
            touchPointerProxy = serializedObject.FindProperty("touchCursor");
            penPointerProxy = serializedObject.FindProperty("penCursor");
            objectPointerProxy = serializedObject.FindProperty("objectCursor");

            useDPI = serializedObject.FindProperty("useDPI");
            cursorSize = serializedObject.FindProperty("cursorSize");
            cursorPixelSize = serializedObject.FindProperty("cursorPixelSize");

            advancedProps = serializedObject.FindProperty("advancedProps");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

			GUILayout.Space(5);

            //			var display = GUIElements.Header(TEXT_DPI_HEADER, useDPI, useDPI);
            //			if (display)
            //			{
            //				EditorGUI.indentLevel++;
            //				using (new EditorGUI.DisabledGroupScope(!useDPI.boolValue))
            //				{
            //					EditorGUILayout.PropertyField(useDPI, TEXT_DPI_HEADER);
            //				}
            //				EditorGUI.indentLevel--;
            //			}

            EditorGUILayout.PropertyField(useDPI, TEXT_DPI_HEADER);
            if (useDPI.boolValue)
            {
                EditorGUILayout.PropertyField(cursorSize, TEXT_POINTER_SIZE);
            }
            else
            {
                EditorGUILayout.PropertyField(cursorPixelSize, TEXT_POINTER_PIXEL_SIZE);
            }

            var display = GUIElements.Header(TEXT_ADVANCED_HEADER, advancedProps);
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
