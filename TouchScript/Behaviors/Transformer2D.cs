/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Behaviors {
	/// <summary>
	/// Simple Component which transforms an object according to events from gestures.
	/// </summary>
	public class Transformer2D : MonoBehaviour {
		#region Unity fields

		/// <summary>
		/// Max movement speed
		/// </summary>
		public float Speed = 10f;

		#endregion

		#region Private variables

		private Vector3 localPositionToGo;
		private float scaleToGo;
		private Quaternion localRotationToGo;

		#endregion

		#region Unity

		private void Start() {
			setDefaults();

			if (GetComponent<PanGesture>() != null) {
				GetComponent<PanGesture>().StateChanged += onPanStateChanged;
			}
			if (GetComponent<ScaleGesture>() != null) {
				GetComponent<ScaleGesture>().StateChanged += onScaleStateChanged;
			}
			if (GetComponent<RotateGesture>() != null) {
				GetComponent<RotateGesture>().StateChanged += onRotateStateChanged;
			}
		}

		private void Update() {
			var fraction = Speed * Time.deltaTime;
			transform.localPosition = Vector3.Lerp(transform.localPosition, localPositionToGo, fraction);
			var newScale = Mathf.Lerp(transform.localScale.x, scaleToGo, fraction);
			transform.localScale = new Vector3(newScale, newScale, newScale);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotationToGo, fraction);
		}

		#endregion

		#region Public functions

		public void PanTeleport(Vector3 destination) {
			localPositionToGo = destination;
			transform.localPosition = destination;
		}

		public void RotateTeleport(Quaternion destination) {
			localRotationToGo = destination;
			transform.localRotation = destination;
		}

		public void ScaleTeleport(float destination) {
			scaleToGo = destination;
			transform.localScale = new Vector3(destination, destination, destination); ;
		}

		#endregion

		#region Private functions

		private void setDefaults() {
			localPositionToGo = transform.localPosition;
			scaleToGo = transform.localScale.x;
			localRotationToGo = transform.localRotation;
		}

		#endregion

		#region Event handlers

		private void onPanStateChanged(object sender, GestureStateChangeEventArgs e) {
			var gesture = (PanGesture)sender;

			if (gesture.LocalDeltaPosition != Vector3.zero) {
				localPositionToGo += gesture.LocalDeltaPosition;
			}
		}

		private void onRotateStateChanged(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs) {
			var gesture = (RotateGesture)sender;

			if (Math.Abs(gesture.LocalDeltaRotation) > 0.01) {
				localRotationToGo = Quaternion.AngleAxis(gesture.LocalDeltaRotation, gesture.GlobalTransformPlane.normal) * localRotationToGo;
			}
		}

		private void onScaleStateChanged(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs) {
			var gesture = (ScaleGesture)sender;

			if (Math.Abs(gesture.LocalDeltaScale - 1) > 0.00001) {
				scaleToGo *= gesture.LocalDeltaScale;
			}
		}

		#endregion
	}
}