/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(Transform2DGestureBase), true)]
    internal class Transform2DGestureBaseEditor : GestureEditor
    {
        private static readonly GUIContent PROJECTION = new GUIContent("Projection Type", "Method used to project 2d screen positions of touch points into 3d space.");
        private static readonly GUIContent PROJECTION_NORMAL = new GUIContent("Projection Normal", "Normal of the plane in 3d space where touch points' positions are projected.");

        private SerializedProperty projection;
        private SerializedProperty projectionNormal;

        protected override void OnEnable()
        {
            base.OnEnable();

            projection = serializedObject.FindProperty("projection");
            projectionNormal = serializedObject.FindProperty("projectionNormal");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.PropertyField(projection, PROJECTION);
            if (projection.enumValueIndex != (int)Transform2DGestureBase.ProjectionType.Layer)
            {
                EditorGUILayout.PropertyField(projectionNormal, PROJECTION_NORMAL);
            }

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
