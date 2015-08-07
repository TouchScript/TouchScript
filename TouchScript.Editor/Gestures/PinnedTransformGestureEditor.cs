/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Gestures.Base;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(PinnedTransformGesture), true)]
    internal class PinnedTransformGestureEditor : PinnedTransformGestureBaseEditor
    {
        private static readonly GUIContent PROJECTION = new GUIContent("Projection Type", "Method used to project 2d screen positions of touch points into 3d space.");
        private static readonly GUIContent PROJECTION_NORMAL = new GUIContent("Projection Normal", "Normal of the plane in 3d space where touch points' positions are projected.");

        private SerializedProperty projection;
        private SerializedProperty projectionPlaneNormal;

        protected override void OnEnable()
        {
            base.OnEnable();

            projection = serializedObject.FindProperty("projection");
            projectionPlaneNormal = serializedObject.FindProperty("projectionPlaneNormal");
        }

        protected override void doInspectorGUI()
        {
            EditorGUILayout.PropertyField(projection, PROJECTION);
            if (projection.enumValueIndex != (int)TransformGesture.ProjectionType.Layer)
            {
                EditorGUILayout.PropertyField(projectionPlaneNormal, PROJECTION_NORMAL);
            }
        }

    }
}
