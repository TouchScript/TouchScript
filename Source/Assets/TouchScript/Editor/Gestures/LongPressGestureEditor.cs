/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(LongPressGesture), true)]
    internal sealed class LongPressGestureEditor : GestureEditor
    {
		public static readonly GUIContent TEXT_TIME_TO_PRESS = new GUIContent("Time to Press (sec)", "Limit maximum number of simultaneous pointers.");
		public static readonly GUIContent TEXT_DISTANCE_LIMIT = new GUIContent("Limit Movement (cm)", "Gesture fails if fingers move more than <Value> cm.");

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a gesture when this GameObject is being pressed for <TimeToPress> seconds.");

		private SerializedProperty distanceLimit, timeToPress;
		private SerializedProperty OnLongPress;

        protected override void OnEnable()
        {
            timeToPress = serializedObject.FindProperty("timeToPress");
            distanceLimit = serializedObject.FindProperty("distanceLimit");
			OnLongPress = serializedObject.FindProperty("OnLongPress");

			base.OnEnable();
        }

		protected override void drawBasic()
		{
            EditorGUILayout.PropertyField(timeToPress, TEXT_TIME_TO_PRESS);
		}

		protected override GUIContent getHelpText()
		{
			return TEXT_HELP;
		}

		protected override void drawGeneral ()
		{
			EditorGUILayout.PropertyField(timeToPress, TEXT_TIME_TO_PRESS);

			base.drawGeneral();
		}

        protected override void drawLimits ()
		{
            EditorGUILayout.PropertyField(distanceLimit, TEXT_DISTANCE_LIMIT);

			base.drawLimits();
        }

		protected override void drawUnityEvents ()
		{
			EditorGUILayout.PropertyField(OnLongPress);

			base.drawUnityEvents ();
		}
    }
}
