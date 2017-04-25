using TouchScript.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.InputSources
{
    public class InputSourceEditor : UnityEditor.Editor
    {
        private const string TEXT_ADVANCED_HEADER = "Advanced properties.";

        private SerializedProperty advanced;

        protected virtual void OnEnable()
        {
            advanced = serializedObject.FindProperty("advancedProps");
        }

        public override void OnInspectorGUI()
        {
#if UNITY_5_6_OR_NEWER
			serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript();
#endif

            EditorGUI.BeginChangeCheck();
            var expanded = GUIElements.BeginFoldout(advanced.isExpanded, new GUIContent("Advanced", TEXT_ADVANCED_HEADER));
            if (EditorGUI.EndChangeCheck())
            {
                advanced.isExpanded = expanded;
            }
            if (expanded)
            {
                GUILayout.BeginVertical(GUIElements.FoldoutStyle);
                drawAdvanced();
                GUILayout.EndVertical();
            }
            GUIElements.EndFoldout();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void drawAdvanced() {}
    }
}
