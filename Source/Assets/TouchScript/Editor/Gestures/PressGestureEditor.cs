/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(PressGesture), true)]
    internal sealed class PressGestureEditor : GestureEditor
    {
		public static readonly GUIContent TEXT_IGNORE_CHILDREN = new GUIContent("Ignore Children", "If selected this gesture ignores pointers from children.");

        public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a gesture when at least one pointer is pressed over this GameObject.");

        private SerializedProperty ignoreChildren;
		private SerializedProperty OnPress;

        protected override void OnEnable()
        {
            ignoreChildren = serializedObject.FindProperty("ignoreChildren");
			OnPress = serializedObject.FindProperty("OnPress");

			base.OnEnable();
        }

		protected override GUIContent getHelpText()
		{
			return TEXT_HELP;
		}

		protected override void drawGeneral()
        {
            EditorGUILayout.PropertyField(ignoreChildren, TEXT_IGNORE_CHILDREN);

			base.drawGeneral();
        }

		protected override void drawUnityEvents ()
		{
			EditorGUILayout.PropertyField(OnPress);

			base.drawUnityEvents();
		}
    }
}
