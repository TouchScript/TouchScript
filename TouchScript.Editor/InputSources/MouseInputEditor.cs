using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(MouseInput), true)]
    internal sealed class MouseInputEditor : InputSourceEditor
    {
        private SerializedProperty tags;
        private SerializedProperty disableOnMobilePlatforms;

        protected override void OnEnable()
        {
            base.OnEnable();

            tags = serializedObject.FindProperty("Tags");
            disableOnMobilePlatforms = serializedObject.FindProperty("DisableOnMobilePlatforms");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(disableOnMobilePlatforms);

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
