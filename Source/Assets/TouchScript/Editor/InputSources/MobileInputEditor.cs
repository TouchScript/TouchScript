/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
#pragma warning disable 0618
    [CustomEditor(typeof (MobileInput), true)]
#pragma warning restore 0618
    internal sealed class MobileInputEditor : InputSourceEditor
    {
        private SerializedProperty disableOnNonTouchPlatforms;

        protected override void OnEnable()
        {
            base.OnEnable();

            disableOnNonTouchPlatforms = serializedObject.FindProperty("DisableOnNonTouchPlatforms");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(disableOnNonTouchPlatforms);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}