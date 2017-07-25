/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Gestures.TransformGestures.Base;
using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using TouchScript.Editor.EditorUI;
using UnityEngine;

namespace TouchScript.Editor.Gestures.TransformGestures
{
    [CustomEditor(typeof(TransformGesture), true)]
    internal class TransformGestureEditor : TwoPointTransformGestureBaseEditor
    {

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a combination of translation, rotation and scaling gestures on the GameObject. Switch to advanced view to see more options.");

		public SerializedProperty projection, projectionPlaneNormal;
		public SerializedProperty projectionProps;

        protected override void OnEnable()
        {
            projection = serializedObject.FindProperty("projection");
            projectionPlaneNormal = serializedObject.FindProperty("projectionPlaneNormal");

			projectionProps = serializedObject.FindProperty("projectionProps");

			base.OnEnable();
        }

        protected override void drawBasic()
        {
            base.drawBasic();

			EditorGUILayout.PropertyField(projection, TEXT_PROJECTION);
			if (projection.enumValueIndex != (int)TransformGesture.ProjectionType.Layer)
			{
				EditorGUILayout.PropertyField(projectionPlaneNormal, TEXT_PROJECTION_NORMAL);
			}
        }

		protected override GUIContent getHelpText()
		{
			return TEXT_HELP;
		}

		protected override void drawOtherGUI()
        {
			var display = GUIElements.Header(TEXT_PROJECTION_HEADER, projectionProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(projection, TEXT_PROJECTION);
				if (projection.enumValueIndex != (int)TransformGesture.ProjectionType.Layer)
				{
					EditorGUILayout.PropertyField(projectionPlaneNormal, TEXT_PROJECTION_NORMAL);
				}
				EditorGUI.indentLevel--;
			}
        }

    }
}
