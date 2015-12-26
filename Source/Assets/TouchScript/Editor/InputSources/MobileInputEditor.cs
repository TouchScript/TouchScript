using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
#pragma warning disable 0618
    [CustomEditor(typeof (MobileInput), true)]
#pragma warning restore 0618
    internal sealed class MobileInputEditor : InputSourceEditor
    {
        private SerializedProperty tags;
        private SerializedProperty disableOnNonTouchPlatforms;

        protected override void OnEnable()
        {
            base.OnEnable();

            tags = serializedObject.FindProperty("Tags");
            disableOnNonTouchPlatforms = serializedObject.FindProperty("DisableOnNonTouchPlatforms");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(disableOnNonTouchPlatforms);

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