using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(TuioInput), true)]
    internal sealed class TuioInputEditor : InputSourceEditor
    {
        private SerializedProperty tags;
        private SerializedProperty tuioPort, movementThreshold;

        protected override void OnEnable()
        {
            base.OnEnable();

            tags = serializedObject.FindProperty("Tags");
            tuioPort = serializedObject.FindProperty("TuioPort");
            movementThreshold = serializedObject.FindProperty("MovementThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(tuioPort);
            EditorGUILayout.PropertyField(movementThreshold);

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
