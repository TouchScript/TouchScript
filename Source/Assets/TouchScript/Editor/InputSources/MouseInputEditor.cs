using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
#pragma warning disable 0618
    [CustomEditor(typeof(MouseInput), true)]
#pragma warning restore 0618
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
