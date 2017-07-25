/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.TransformGestures.Base
{
	internal class TransformGestureBaseEditor : GestureEditor
	{
		public static readonly GUIContent TEXT_PROJECTION_HEADER = new GUIContent("Projection", "Screen to 3D object projection parameters.");

		public static readonly GUIContent TEXT_TYPE = new GUIContent("Transform Type", "Specifies what gestures should be detected: Translation, Rotation, Scaling.");
		public static readonly GUIContent TEXT_TYPE_TRANSLATION = new GUIContent(" Translation", "Dragging with one ore more fingers.");
		public static readonly GUIContent TEXT_TYPE_ROTATION = new GUIContent(" Rotation", "Rotating with two or more fingers.");
		public static readonly GUIContent TEXT_TYPE_SCALING = new GUIContent(" Scaling", "Scaling with two or more fingers.");
		public static readonly GUIContent TEXT_MIN_SCREEN_POINTS_DISTANCE = new GUIContent("Min Points Distance (cm)", "Minimum distance between two pointers (clusters) in cm to consider this gesture started. Used to prevent fake pointers spawned near real ones on cheap multitouch hardware to mess everything up.");
		public static readonly GUIContent TEXT_SCREEN_TRANSFORM_THRESHOLD = new GUIContent("Movement Threshold (cm)", "Minimum distance in cm pointers must move for the gesture to begin.");
		public static readonly GUIContent TEXT_PROJECTION = new GUIContent("Projection Type", "Method used to project 2d screen positions of pointers into 3d space.");
		public static readonly GUIContent TEXT_PROJECTION_NORMAL = new GUIContent("Projection Normal", "Normal of the plane in 3d space where pointers' positions are projected.");

		protected SerializedProperty type, minScreenPointsDistance, screenTransformThreshold;
		protected SerializedProperty OnTransformStart, OnTransform, OnTransformComplete;

		protected override void OnEnable()
		{
			type = serializedObject.FindProperty("type");
			minScreenPointsDistance = serializedObject.FindProperty("minScreenPointsDistance");
			screenTransformThreshold = serializedObject.FindProperty("screenTransformThreshold");
			OnTransformStart = serializedObject.FindProperty("OnTransformStart");
			OnTransform = serializedObject.FindProperty("OnTransform");
			OnTransformComplete = serializedObject.FindProperty("OnTransformComplete");

			base.OnEnable();
		}

		protected override void drawUnityEvents ()
		{
			EditorGUILayout.PropertyField(OnTransformStart);
			EditorGUILayout.PropertyField(OnTransform);
			EditorGUILayout.PropertyField(OnTransformComplete);

			base.drawUnityEvents ();
		}

	}
}

