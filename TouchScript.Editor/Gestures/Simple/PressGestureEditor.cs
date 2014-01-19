/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(PressGesture))]
    public class PressGestureEditor : GestureEditor
    {

        public const string IGNORE_CHILDREN = "If selected this gesture ignores touch points from children.";
        
        private SerializedProperty ignoreChildren;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            ignoreChildren = serializedObject.FindProperty("ignoreChildren");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            
            EditorGUILayout.PropertyField(ignoreChildren, new GUIContent("Ignore Children", IGNORE_CHILDREN));
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}