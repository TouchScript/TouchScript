/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_TUIO
using TouchScript.InputSources;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(TuioInput), true)]
    internal sealed class TuioInputEditor : InputSourceEditor
    {
        private static readonly GUIContent INPUT_TYPES = new GUIContent("Input Types", "Supported input types.");

        private TuioInput instance;
        private SerializedProperty supportedInputs;
        private SerializedProperty tuioPort;

        protected override void OnEnable()
        {
            base.OnEnable();

            instance = target as TuioInput;
            supportedInputs = serializedObject.FindProperty("supportedInputs");
            tuioPort = serializedObject.FindProperty("tuioPort");
        }

        public override void OnInspectorGUI()
        {
#if UNITY_5_6_OR_NEWER
			serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript();
#endif

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

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
#endif