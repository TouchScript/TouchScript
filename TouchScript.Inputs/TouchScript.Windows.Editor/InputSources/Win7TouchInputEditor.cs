using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(Win7TouchInput), true)]
    internal sealed class Win7TouchInputEditor : InputSourceEditor
    {
        private SerializedProperty tags;
        private SerializedProperty disableMouseInputInBuilds;

        protected override void OnEnable()
        {
            base.OnEnable();

            tags = serializedObject.FindProperty("Tags");
            disableMouseInputInBuilds = serializedObject.FindProperty("DisableMouseInputInBuilds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(disableMouseInputInBuilds);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        protected override void drawAdvanced()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(tags);
            EditorGUI.indentLevel--;
        }
    }
}
