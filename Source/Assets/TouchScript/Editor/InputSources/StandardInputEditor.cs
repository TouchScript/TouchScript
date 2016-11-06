/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof (StandardInput), true)]
    internal sealed class StandardInputEditor : InputSourceEditor
    {
        private SerializedProperty windows8Touch,
            windows7Touch,
            webGLTouch,
            windows8Mouse,
            windows7Mouse,
            universalWindowsMouse,
            emulateSecondMousePointer;

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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(windows8Touch);
            EditorGUILayout.PropertyField(windows7Touch);
            EditorGUILayout.PropertyField(webGLTouch);
            EditorGUILayout.PropertyField(windows8Mouse);
            EditorGUILayout.PropertyField(windows7Mouse);
            EditorGUILayout.PropertyField(universalWindowsMouse);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(emulateSecondMousePointer);
            if (EditorGUI.EndChangeCheck())
            {
                instance.EmulateSecondMousePointer = emulateSecondMousePointer.boolValue;
            }
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}