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
		public static readonly GUIContent TEXT_COMBINE_POINTERS = new GUIContent("Combine Pointers", "When several fingers are used to perform a tap, pointers released not earlier than <CombineInterval> seconds ago are used to calculate gesture's final screen position.");
		public static readonly GUIContent TEXT_COMBINE_TOUCH_POINTERS = new GUIContent("Combine Interval (sec)", TEXT_COMBINE_POINTERS.tooltip);

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a gesture when this GameObject is tapped.");

		private SerializedProperty numberOfTapsRequired, distanceLimit, timeLimit, combinePointers, combinePointersInterval;
		private SerializedProperty OnTap;

        protected override void OnEnable()
        {
            numberOfTapsRequired = serializedObject.FindProperty("numberOfTapsRequired");
            timeLimit = serializedObject.FindProperty("timeLimit");
            distanceLimit = serializedObject.FindProperty("distanceLimit");
			combinePointers = serializedObject.FindProperty("combinePointers");
			combinePointersInterval = serializedObject.FindProperty("combinePointersInterval");

			OnTap = serializedObject.FindProperty("OnTap");

			base.OnEnable();
        }

        protected override void drawBasic()
        {
			EditorGUIUtility.labelWidth = 180;
			EditorGUILayout.IntPopup(numberOfTapsRequired, new[] { new GUIContent("One"), new GUIContent("Two"), new GUIContent("Three") }, new[] { 1, 2, 3 }, TEXT_NUMBER_OF_TAPS_REQUIRED, GUILayout.ExpandWidth(true));
		}

        protected override GUIContent getHelpText()
        {
            return TEXT_HELP;
        }

		protected override void drawGeneral()
		{
			EditorGUIUtility.labelWidth = 180;
			EditorGUILayout.IntPopup(numberOfTapsRequired, new[] {new GUIContent("One"), new GUIContent("Two"), new GUIContent("Three")}, new[] {1, 2, 3}, TEXT_NUMBER_OF_TAPS_REQUIRED, GUILayout.ExpandWidth(true));
			EditorGUILayout.PropertyField(combinePointers, TEXT_COMBINE_POINTERS);
			if (combinePointers.boolValue)
			{
				EditorGUIUtility.labelWidth = 160;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(GUIContent.none, GUILayout.Width(10));
				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
				EditorGUILayout.PropertyField(combinePointersInterval, TEXT_COMBINE_TOUCH_POINTERS);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			base.drawGeneral ();
		}

		protected override void drawLimits()
        {
            EditorGUILayout.PropertyField(timeLimit, TEXT_TIME_LIMIT);
            EditorGUILayout.PropertyField(distanceLimit, TEXT_DISTANCE_LIMIT);

			base.drawLimits();
        }

		protected override void drawUnityEvents()
		{
			EditorGUILayout.PropertyField(OnTap);

			base.drawUnityEvents();
		}

    }
}
