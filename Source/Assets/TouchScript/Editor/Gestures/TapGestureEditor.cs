/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(TapGesture), true)]
    internal sealed class TapGestureEditor : GestureEditor
    {
		public static readonly GUIContent TEXT_TIME_LIMIT = new GUIContent("Limit Time (sec)", "Gesture fails if in <value> seconds user didn't do the required number of taps.");
		public static readonly GUIContent TEXT_DISTANCE_LIMIT = new GUIContent("Limit Movement (cm)", "Gesture fails if taps are made more than <value> cm away from the first pointer position.");
		public static readonly GUIContent TEXT_NUMBER_OF_TAPS_REQUIRED = new GUIContent("Number of Taps Required", "Number of taps required for this gesture to be recognized.");

        private SerializedProperty numberOfTapsRequired, distanceLimit, timeLimit;
		private SerializedProperty OnTap;

        protected override void OnEnable()
        {
            numberOfTapsRequired = serializedObject.FindProperty("numberOfTapsRequired");
            timeLimit = serializedObject.FindProperty("timeLimit");
            distanceLimit = serializedObject.FindProperty("distanceLimit");

			OnTap = serializedObject.FindProperty("OnTap");

            shouldDrawCombineTouches = true;

			base.OnEnable();
        }

		protected override void drawGeneral()
		{
			EditorGUIUtility.labelWidth = 180;
			EditorGUILayout.IntPopup(numberOfTapsRequired, new[] {new GUIContent("One"), new GUIContent("Two"), new GUIContent("Three")}, new[] {1, 2, 3}, TEXT_NUMBER_OF_TAPS_REQUIRED, GUILayout.ExpandWidth(true));

			base.drawGeneral ();
		}

		protected override void drawLimits()
        {
            EditorGUILayout.PropertyField(timeLimit, TEXT_TIME_LIMIT);
            EditorGUILayout.PropertyField(distanceLimit, TEXT_DISTANCE_LIMIT);

			base.drawLimits();
        }

		protected override void drawUnityEvents ()
		{
			EditorGUILayout.PropertyField(OnTap);

			base.drawUnityEvents();
		}
    }
}
