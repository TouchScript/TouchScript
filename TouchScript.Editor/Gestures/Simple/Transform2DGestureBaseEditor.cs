/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Utils;
using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    public class Transform2DGestureBaseEditor : GestureEditor
    {

        public const string TEXT_PROJECTION = "Method used to project 2d screen positions of touch points into 3d space.";
        public const string TEXT_PROJECTIONNORMAL = "Normal of the plane in 3d space where touch points' positions are projected.";

        private SerializedProperty projection, projectionNormal;

        protected override void OnEnable()
        {
            base.OnEnable();

            projection = serializedObject.FindProperty("projection");
            projectionNormal = serializedObject.FindProperty("projectionNormal");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUIUtility.LookLikeInspector();

            EditorGUILayout.PropertyField(projection, new GUIContent("Projection Type", TEXT_PROJECTION));
            if (projection.enumValueIndex != (int)Transform2DGestureBase.ProjectionType.Camera)
            {
                GUIElements.DrawCompactVector3(new GUIContent("Projection Normal", TEXT_PROJECTIONNORMAL), projectionNormal);
            }

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}