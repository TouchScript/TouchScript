using TouchScript.InputSources;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(TuioInput), true)]
    internal sealed class TuioInputEditor : InputSourceEditor
    {
        private static readonly GUIContent INPUT_TYPES = new GUIContent("Input Types", "Supported input types.");
        private static readonly GUIContent OBJECT_MAPPINGS = new GUIContent("TUIO Object Mappings", "TUIO Object to Tag list.");

        private TuioInput instance;
        private SerializedProperty supportedInputs, tuioObjectMappings;
        private SerializedProperty cursorTags, blobTags, objectTags;
        private SerializedProperty tuioPort;
        private ReorderableList list;

        protected override void OnEnable()
        {
            base.OnEnable();

            instance = target as TuioInput;
            supportedInputs = serializedObject.FindProperty("supportedInputs");
            tuioObjectMappings = serializedObject.FindProperty("tuioObjectMappings");
            cursorTags = serializedObject.FindProperty("cursorTags");
            blobTags = serializedObject.FindProperty("blobTags");
            objectTags = serializedObject.FindProperty("objectTags");
            tuioPort = serializedObject.FindProperty("tuioPort");

            list = new ReorderableList(serializedObject, tuioObjectMappings, false, true, true, true);
            list.drawHeaderCallback += rect => GUI.Label(rect, OBJECT_MAPPINGS);
            list.drawElementCallback += (rect, index, active, focused) =>
            {
                EditorGUI.BeginChangeCheck();
                var element = instance.TuioObjectMappings[index];
                rect.height = 16;
                rect.y += 2;
                var r = rect;
                r.width = 20;
                GUI.Label(r, "id:");
                r.x += r.width;
                r.width = 50;
                var newId = EditorGUI.IntField(r, element.Id);
                r.x += r.width;
                r.width = 40;
                GUI.Label(r, "  tag:");
                r.x += r.width;
                r.width = rect.width - r.x + rect.x;
                var newTag = GUI.TextField(r, element.Tag);

                if (EditorGUI.EndChangeCheck())
                {
                    element.Id = newId;
                    element.Tag = newTag;
                    EditorUtility.SetDirty(target);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(tuioPort);

            var r = EditorGUILayout.GetControlRect(true, 16f, EditorStyles.layerMaskField);
            var label = EditorGUI.BeginProperty(r, INPUT_TYPES, supportedInputs);
            EditorGUI.BeginChangeCheck();
            r = EditorGUI.PrefixLabel(r, label);
            var sMask = (TuioInput.InputType)EditorGUI.EnumMaskField(r, instance.SupportedInputs);
            if (EditorGUI.EndChangeCheck())
            {
                instance.SupportedInputs = sMask;
                EditorUtility.SetDirty(instance);
            }
            EditorGUI.EndProperty();

            if ((sMask & TuioInput.InputType.Objects) != 0)
            {
                list.DoLayoutList();
            }

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        protected override void drawAdvanced()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(cursorTags);
            EditorGUILayout.PropertyField(blobTags);
            EditorGUILayout.PropertyField(objectTags);
            EditorGUI.indentLevel--;
        }
    }
}
