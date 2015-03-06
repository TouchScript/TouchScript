/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(MultiPointPanGesture), true)]
    internal class MultiPointPanGestureEditor : MultiPointTransform2DGestureBaseEditor
    {
        private static readonly GUIContent MIN_ALIGNMENT = new GUIContent("Min Alignment", "Minimum alignment of touch points move direction to begin this gesture.  -1 = can move in any direction, 1 = must move in the exact same direction");
        
        private SerializedProperty minAlignment;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            minAlignment = serializedObject.FindProperty("minAlignment");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            
            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(minAlignment, MIN_ALIGNMENT);
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
