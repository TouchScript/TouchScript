using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(Win8TouchInput), true)]
    internal sealed class Win8TouchInputEditor : InputSourceEditor
    {
        private SerializedProperty touchTags, mouseTags, penTags;

        protected override void OnEnable()
        {
            base.OnEnable();

            touchTags = serializedObject.FindProperty("TouchTags");
            mouseTags = serializedObject.FindProperty("MouseTags");
            penTags = serializedObject.FindProperty("PenTags");
        }

        protected override void drawAdvanced()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(touchTags);
            EditorGUILayout.PropertyField(mouseTags);
            EditorGUILayout.PropertyField(penTags);
            EditorGUI.indentLevel--;
        }
    }
}
