using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

namespace TouchScript.Examples.Camera {

	public class CameraController : MonoBehaviour {

		public float PanSpeed = 200f;
		public float ZoomSpeed = 10f;

		private Transform pivot;
		private ScreenTransformGesture gesture;
		private Transform cam;

		private void Awake() {
			pivot = transform.Find("Pivot");
			cam = transform.Find("Pivot/Camera");
		}

		private void OnEnable() {
			gesture = GetComponent<ScreenTransformGesture>();
			gesture.Transformed += transformedHandler;
		}

		private void OnDisable() {
			gesture.Transformed -= transformedHandler;
		}

		private void transformedHandler (object sender, System.EventArgs e)
		{
			var rotation = Quaternion.Euler(gesture.DeltaPosition.y / Screen.height * PanSpeed, -gesture.DeltaPosition.x / Screen.width * PanSpeed, gesture.DeltaRotation);
			pivot.localRotation *= rotation;

			cam.transform.localPosition += Vector3.forward * (gesture.DeltaScale - 1f) * ZoomSpeed;
		}
		
	}

}