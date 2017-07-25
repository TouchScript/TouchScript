/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Gestures.TransformGestures.Base;
using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.TransformGestures
{
    [CustomEditor(typeof(ScreenTransformGesture), true)]
    internal class ScreenTransformGestureEditor : TwoPointTransformGestureBaseEditor
    {

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a combination of translation, rotation and scaling gestures on the GameObject in screen space.");

		protected override GUIContent getHelpText()
		{
			return TEXT_HELP;
		}

	}
}
