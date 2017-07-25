/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Behaviors;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using TouchScript.Editor.EditorUI;

namespace TouchScript.Editor.Behaviors
{
    [CustomEditor(typeof(Transformer), true)]
    internal class TransformerEditor : UnityEditor.Editor
    {
		public static readonly GUIContent TEXT_SMOOTHING_HEADER = new GUIContent("Smoothing", "Applies smoothing to transform actions. This allows to reduce jagged movements but adds some visual lag.");
		public static readonly GUIContent TEXT_SMOOTHING_FACTOR = new GUIContent("Factor", "Indicates how much smoothing to apply. 0 - no smoothing, 100000 - maximum.");
		public static readonly GUIContent TEXT_POSITION_THRESHOLD = new GUIContent("Position Threshold", "Minimum distance between target position and smoothed position when to stop automatic movement.");
		public static readonly GUIContent TEXT_ROTATION_THRESHOLD = new GUIContent("Rotation Threshold", "Minimum angle between target rotation and smoothed rotation when to stop automatic movement.");
		public static readonly GUIContent TEXT_SCALE_THRESHOLD = new GUIContent("Scale Threshold", "Minimum difference between target scale and smoothed scale when to stop automatic movement.");
		public static readonly GUIContent TEXT_ALLOW_CHANGING = new GUIContent("Allow Changing From Outside", "Indicates if this transform can be changed from another script.");
		public static readonly GUIContent TEXT_SMOOTHING_FACTOR_DESC = new GUIContent("Indicates how much smoothing to apply. \n0 - no smoothing, 100000 - maximum.");

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component receives transform data from Transform Gestures and applies changes to the GameObject.");

		private Transformer instance;

        private SerializedProperty enableSmoothing, allowChangingFromOutside;
		private PropertyInfo enableSmoothing_prop;

        protected virtual void OnEnable()
        {
            enableSmoothing = serializedObject.FindProperty("enableSmoothing");
            allowChangingFromOutside = serializedObject.FindProperty("allowChangingFromOutside");

            instance = target as Transformer;

			var type = instance.GetType();
			enableSmoothing_prop = type.GetProperty("EnableSmoothing", BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
#if UNITY_5_6_OR_NEWER
			serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript();
#endif

			GUILayout.Space(5);

			var display = GUIElements.Header(TEXT_SMOOTHING_HEADER, enableSmoothing, enableSmoothing, enableSmoothing_prop);
			if (display)
			{
				EditorGUI.indentLevel++;
				using (new EditorGUI.DisabledGroupScope(!enableSmoothing.boolValue))
				{
					instance.SmoothingFactor = EditorGUILayout.FloatField(TEXT_SMOOTHING_FACTOR, instance.SmoothingFactor);
					EditorGUILayout.LabelField(TEXT_SMOOTHING_FACTOR_DESC, GUIElements.HelpBox);
					instance.PositionThreshold = EditorGUILayout.FloatField(TEXT_POSITION_THRESHOLD, instance.PositionThreshold);
					instance.RotationThreshold = EditorGUILayout.FloatField(TEXT_ROTATION_THRESHOLD, instance.RotationThreshold);
					instance.ScaleThreshold = EditorGUILayout.FloatField(TEXT_SCALE_THRESHOLD, instance.ScaleThreshold);
					EditorGUILayout.PropertyField(allowChangingFromOutside, TEXT_ALLOW_CHANGING);
				}
				EditorGUI.indentLevel--;
			}
            EditorGUILayout.LabelField(TEXT_HELP, GUIElements.HelpBox);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
