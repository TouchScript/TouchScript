/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
	[CustomEditor(typeof(MetaGesture), true)]
	internal sealed class MetaGestureEditor : GestureEditor
	{
		public static readonly GUIContent TEXT_HELP = new GUIContent("This component serves as a proxy from TouchScript gesture recognition logic to C# events. It catches pointers like a normal event and dispatches events for every event of caught pointers.");

		protected override void OnEnable()
		{
			base.OnEnable();

            shouldDrawGeneral = false;
		}

		protected override GUIContent getHelpText()
		{
			return TEXT_HELP;
		}
	}
}
