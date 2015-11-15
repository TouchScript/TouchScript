using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

namespace TouchScript.Examples.Checkers 
{
	public class Board : MonoBehaviour 
	{
		PinnedTransformGesture gesture;

		private void OnEnable() {
			gesture = GetComponent<PinnedTransformGesture>();
			gesture.Transformed += transformedHandler;
		}
		
		private void OnDisable() {
			gesture.Transformed -= transformedHandler;
		}

		private void transformedHandler(object sender, System.EventArgs e)
		{
			transform.localRotation *= Quaternion.AngleAxis(gesture.DeltaRotation, gesture.RotationAxis);
		}
	}
}