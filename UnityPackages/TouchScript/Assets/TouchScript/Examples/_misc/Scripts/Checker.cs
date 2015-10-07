using UnityEngine;
using System.Collections;
using TouchScript.Gestures;
using TouchScript.Behaviors;

namespace TouchScript.Examples 
{
	public class Checker : MonoBehaviour 
	{
		TransformGesture gesture;
		Transformer transformer;
		Rigidbody rb;

		private void OnEnable() 
		{
			gesture = GetComponent<TransformGesture>();
			transformer = GetComponent<Transformer>();
			rb = GetComponent<Rigidbody>();

			transformer.enabled = false;
			rb.isKinematic = false;
			gesture.TransformStarted += transformStartedHandler;
			gesture.TransformCompleted += transformCompletedHandler;
		}

		private void OnDisable() 
		{
			gesture.TransformStarted -= transformStartedHandler;
			gesture.TransformCompleted -= transformCompletedHandler;
		}

		private void transformStartedHandler(object sender, System.EventArgs e)
		{
			rb.isKinematic = true;
			transformer.enabled = true;
		}

		private void transformCompletedHandler(object sender, System.EventArgs e)
		{
			transformer.enabled = false;
			rb.isKinematic = false;
			rb.WakeUp();
		}
	}
}