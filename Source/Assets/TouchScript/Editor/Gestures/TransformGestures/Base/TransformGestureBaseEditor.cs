/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.EditorUI;
using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.TransformGestures.Base
{
	internal class TransformGestureBaseEditor : GestureEditor
	{
		public static readonly GUIContent TEXT_PROJECTION_HEADER = new GUIContent("Projection", "Screen to 3D object projection parameters.");


		public static readonly GUIContent TEXT_TYPE = new GUIContent("Transform Type", "Specifies what gestures should be detected: Translation, Rotation, Scaling.");
		public static readonly GUIContent TEXT_TYPE_TRANSLATION = new GUIContent("Translation", "Dragging with one ore more fingers.");
		public static readonly GUIContent TEXT_TYPE_ROTATION = new GUIContent("Rotation", "Rotating with two or more fingers.");
		public static readonly GUIContent TEXT_TYPE_SCALING = new GUIContent("Scaling", "Scaling with two or more fingers.");
		public static readonly GUIContent TEXT_MIN_SCREEN_POINTS_DISTANCE = new GUIContent("Min Points Distance (cm)", "Minimum distance between two pointers (clusters) in cm to consider this gesture started. Used to prevent fake pointers spawned near real ones on cheap multitouch hardware to mess everything up.");
		public static readonly GUIContent TEXT_SCREEN_TRANSFORM_THRESHOLD = new GUIContent("Movement Threshold (cm)", "Minimum distance in cm pointers must move for the gesture to begin.");

		public static readonly GUIContent TEXT_PROJECTION = new GUIContent("Projection Type", "Method used to project 2d screen positions of pointers into 3d space.");
        public static readonly GUIContent TEXT_PROJECTION_LAYER = new GUIContent("Transform plane is parallel to the camera.");
        public static readonly GUIContent TEXT_PROJECTION_OBJECT = new GUIContent("Transform plane is relative to the object.");
        public static readonly GUIContent TEXT_PROJECTION_GLOBAL = new GUIContent("Transform plane is relative to the world.");
		public static readonly GUIContent TEXT_PROJECTION_NORMAL = new GUIContent("Projection Normal", "Normal of the plane in 3d space where pointers' positions are projected.");


		protected SerializedProperty type, minScreenPointsDistance, screenTransformThreshold;
		protected SerializedProperty OnTransformStart, OnTransform, OnTransformComplete;

		public SerializedProperty projection, projectionPlaneNormal;
		public SerializedProperty projectionProps;

		private Texture2D xy, xz, yz, unknown, selector;
		private Color selectorColor = new Color(1, 1, 1, .05f);
		private Color selectorColorSelected = new Color(1, 1, 1, .9f);
        protected bool customProjection = false;

		protected override void OnEnable()
		{
			type = serializedObject.FindProperty("type");
			minScreenPointsDistance = serializedObject.FindProperty("minScreenPointsDistance");
			screenTransformThreshold = serializedObject.FindProperty("screenTransformThreshold");
			OnTransformStart = serializedObject.FindProperty("OnTransformStart");
			OnTransform = serializedObject.FindProperty("OnTransform");
			OnTransformComplete = serializedObject.FindProperty("OnTransformComplete");

			projection = serializedObject.FindProperty("projection");
			projectionPlaneNormal = serializedObject.FindProperty("projectionPlaneNormal");
			projectionProps = serializedObject.FindProperty("projectionProps");

			xy = EditorResources.Load<Texture2D>("Icons/xy.png");
			xz = EditorResources.Load<Texture2D>("Icons/xz.png");
			yz = EditorResources.Load<Texture2D>("Icons/yz.png");
			unknown = EditorResources.Load<Texture2D>("Icons/unknown.png");
			selector = EditorResources.Load<Texture2D>("Icons/selector.png");

			base.OnEnable();
		}

		protected override void drawUnityEvents ()
		{
			EditorGUILayout.PropertyField(OnTransformStart);
			EditorGUILayout.PropertyField(OnTransform);
			EditorGUILayout.PropertyField(OnTransformComplete);

			base.drawUnityEvents ();
		}

        protected void initCustomProjection()
        {
			var v = projectionPlaneNormal.vector3Value;
			customProjection = !(v == Vector3.up || v == Vector3.right || v == Vector3.forward);
        }

        protected bool drawProjection(bool custom)
        {
			EditorGUILayout.PropertyField(projection, TEXT_PROJECTION);
			switch (projection.enumValueIndex)
			{
				case (int)TransformGesture.ProjectionType.Layer:
                    EditorGUILayout.LabelField(TEXT_PROJECTION_LAYER, GUIElements.HelpBox);
					break;
				case (int)TransformGesture.ProjectionType.Object:
                    EditorGUILayout.LabelField(TEXT_PROJECTION_OBJECT, GUIElements.HelpBox);
					break;
				case (int)TransformGesture.ProjectionType.Global:
                    EditorGUILayout.LabelField(TEXT_PROJECTION_GLOBAL, GUIElements.HelpBox);
					break;
			}
			
			if (projection.enumValueIndex != (int)TransformGesture.ProjectionType.Layer)
			{
				var v = projectionPlaneNormal.vector3Value;
                var rect = GUILayoutUtility.GetRect(0, 35, GUILayout.ExpandWidth(true));

				rect.width = 44;
				rect.x += 10;
				GUI.DrawTexture(rect, yz);
                if (drawSelector(rect, !custom && v == Vector3.right)) 
                {
                    projectionPlaneNormal.vector3Value = Vector3.right;
                    custom = false;
                }

				rect.x += rect.width + 5;
				GUI.DrawTexture(rect, xz);
                if (drawSelector(rect, !custom && v == Vector3.up))
                {
                    projectionPlaneNormal.vector3Value = Vector3.up;
                    custom = false;
                }

				rect.x += rect.width + 5;
				GUI.DrawTexture(rect, xy);
                if (drawSelector(rect, !custom && v == Vector3.forward))
                {
                    projectionPlaneNormal.vector3Value = Vector3.forward;
                    custom = false;
                }

				rect.x += rect.width + 10;
				GUI.DrawTexture(rect, unknown);
                if (drawSelector(rect, custom)) custom = true;

                if (custom) EditorGUILayout.PropertyField(projectionPlaneNormal, TEXT_PROJECTION_NORMAL);
			}

            return custom;
        }

		protected bool drawSelector(Rect rect, bool selected)
		{
			GUI.color = selected ? selectorColorSelected : selectorColor;
			GUI.DrawTexture(rect, selector);
			GUI.color = Color.white;

            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                return true;
            }
            return false;
		}

	}
}

