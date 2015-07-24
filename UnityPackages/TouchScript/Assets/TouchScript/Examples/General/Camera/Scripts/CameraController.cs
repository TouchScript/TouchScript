using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

namespace TouchScript.Examples.Camera {

	public class CameraController : MonoBehaviour {

		public float PanSpeed = 200f;
		public float ZoomSpeed = 1f;

		private TransformGesture gesture;
		private Transform camera;

		private void Awake() {
			camera = transform.FindChild("Camera");
		}

		private void OnEnable() {
			gesture = GetComponent<TransformGesture>();
			gesture.Transformed += transformedHandler;
		}

		private void OnDisable() {
			gesture.Transformed -= transformedHandler;
		}

		private void transformedHandler (object sender, System.EventArgs e)
		{
			var rotation = Quaternion.Euler(gesture.DeltaPosition.y / Screen.height * PanSpeed, -gesture.DeltaPosition.x / Screen.width * PanSpeed, gesture.DeltaRotation);
			transform.localRotation *= rotation;

			camera.transform.localPosition += Vector3.forward * (gesture.DeltaScale - 1f) * ZoomSpeed;
		}
		
	}

}