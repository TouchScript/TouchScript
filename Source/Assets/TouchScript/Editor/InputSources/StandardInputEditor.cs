using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof (StandardInput), true)]
    internal sealed class StandardInputEditor : InputSourceEditor
    {
        private SerializedProperty windows8Touch,
            windows7Touch,
            webPlayerTouch,
            webGLTouch,
            windows8Mouse,
            windows7Mouse,
            universalWindowsMouse;

        protected override void OnEnable()
        {
            base.OnEnable();

            windows8Touch = serializedObject.FindProperty("Windows8API");
            windows7Touch = serializedObject.FindProperty("Windows7API");
            webPlayerTouch = serializedObject.FindProperty("WebPlayerTouch");
            webGLTouch = serializedObject.FindProperty("WebGLTouch");
            windows8Mouse = serializedObject.FindProperty("Windows8Mouse");
            windows7Mouse = serializedObject.FindProperty("Windows7Mouse");
            universalWindowsMouse = serializedObject.FindProperty("UniversalWindowsMouse");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUILayout.PropertyField(windows8Touch);
            EditorGUILayout.PropertyField(windows7Touch);
            EditorGUILayout.PropertyField(webPlayerTouch);
            EditorGUILayout.PropertyField(webGLTouch);
            EditorGUILayout.PropertyField(windows8Mouse);
            EditorGUILayout.PropertyField(windows7Mouse);
            EditorGUILayout.PropertyField(universalWindowsMouse);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}