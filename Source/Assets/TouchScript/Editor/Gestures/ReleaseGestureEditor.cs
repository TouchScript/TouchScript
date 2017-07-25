/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(ReleaseGesture), true)]
    internal sealed class ReleaseGestureEditor : GestureEditor
    {
		public static readonly GUIContent TEXT_IGNORE_CHILDREN = new GUIContent("Ignore Children", "If selected this gesture ignores pointers from children.");

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component recognizes a gesture when all pointers are lifted off from this GameObject.");

		private SerializedProperty ignoreChildren;
		private SerializedProperty OnRelease;

        protected override void OnEnable()
        {
            ignoreChildren = serializedObject.FindProperty("ignoreChildren");
			OnRelease = serializedObject.FindProperty("OnRelease");

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
			EditorGUILayout.PropertyField(OnRelease);

			base.drawUnityEvents();
		}
    }
}
