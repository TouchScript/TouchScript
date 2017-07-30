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

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a combination of translation, rotation and scaling gestures on the GameObject.");


        protected override void OnEnable()
        {
			base.OnEnable();

            initCustomProjection();
        }

        protected override void drawBasic()
        {
            base.drawBasic();

			customProjection = drawProjection(customProjection);
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
				customProjection = drawProjection(customProjection);
                EditorGUILayout.Space();
				EditorGUI.indentLevel--;
			}
        }

    }
}
