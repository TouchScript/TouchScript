/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;
using UnityEditor;
using UnityEngine;
using TouchScript.Editor.Utils;
using System.Reflection;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof (StandardInput), true)]
    internal sealed class StandardInputEditor : InputSourceEditor
    {
		public static readonly GUIContent TEXT_GENERAL_HEADER = new GUIContent("General", "General settings.");
		public static readonly GUIContent TEXT_WINDOWS_HEADER = new GUIContent("Windows", "Windows specific settings.");

		public static readonly GUIContent TEXT_WINDOWS_API = new GUIContent("Select which touch API to use:\n - Windows 8 — new WM_POINTER API,\n - Windows 7 — old WM_TOUCH API,\n - Unity — Unity's native WM_TOUCH implementation,\n - None — no touch please.");

		public static readonly GUIContent TEXT_WINDOWS8 = new GUIContent("Windows 8+ API");
		public static readonly GUIContent TEXT_WINDOWS7 = new GUIContent("Windows 7 API");
		public static readonly GUIContent TEXT_WINDOWS8_MOUSE = new GUIContent("Enable Mouse on Windows 8+");
		public static readonly GUIContent TEXT_WINDOWS7_MOUSE = new GUIContent("Enable Mouse on Windows 7");
		public static readonly GUIContent TEXT_UWP_MOUSE = new GUIContent("Enable Mouse on UWP");

        private SerializedProperty windows8Touch, windows7Touch, webGLTouch, windows8Mouse,
            windows7Mouse, universalWindowsMouse, emulateSecondMousePointer;
		private SerializedProperty generalProps, windowsProps;

        private StandardInput instance;

        protected override void OnEnable()
        {
            base.OnEnable();

            instance = target as StandardInput;
            windows8Touch = serializedObject.FindProperty("windows8API");
            windows7Touch = serializedObject.FindProperty("windows7API");
            webGLTouch = serializedObject.FindProperty("webGLTouch");
            windows8Mouse = serializedObject.FindProperty("windows8Mouse");
            windows7Mouse = serializedObject.FindProperty("windows7Mouse");
            universalWindowsMouse = serializedObject.FindProperty("universalWindowsMouse");
            emulateSecondMousePointer = serializedObject.FindProperty("emulateSecondMousePointer");

			generalProps = serializedObject.FindProperty("generalProps");
			windowsProps = serializedObject.FindProperty("windowsProps");
        }

        public override void OnInspectorGUI()
        {
#if UNITY_5_6_OR_NEWER
			serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript();
#endif

			GUILayout.Space(5);

			var display = GUIElements.Header(TEXT_GENERAL_HEADER, generalProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(emulateSecondMousePointer);
				if (EditorGUI.EndChangeCheck())
				{
					instance.EmulateSecondMousePointer = emulateSecondMousePointer.boolValue;
				}
				EditorGUILayout.PropertyField(webGLTouch);
				EditorGUI.indentLevel--;
			}

			display = GUIElements.Header(TEXT_WINDOWS_HEADER, windowsProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(windows8Touch, TEXT_WINDOWS8);
				EditorGUILayout.PropertyField(windows7Touch, TEXT_WINDOWS7);
				EditorGUILayout.LabelField(TEXT_WINDOWS_API, GUIElements.HelpBox);
				EditorGUILayout.PropertyField(windows8Mouse, TEXT_WINDOWS8_MOUSE);
				EditorGUILayout.PropertyField(windows7Mouse, TEXT_WINDOWS7_MOUSE);
				EditorGUILayout.PropertyField(universalWindowsMouse, TEXT_UWP_MOUSE);
				EditorGUI.indentLevel--;
			}
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}