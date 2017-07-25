/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Gestures.TransformGestures.Base;
using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using UnityEngine;
using TouchScript.Editor.EditorUI;

namespace TouchScript.Editor.Gestures.TransformGestures
{
    [CustomEditor(typeof(PinnedTransformGesture), true)]
    internal class PinnedTransformGestureEditor : OnePointTransformGestureBaseEditor
    {

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a combination of rotation and scaling gestures on the GameObject if it was pinned to the world position. Switch to advanced view to see more options.");

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
