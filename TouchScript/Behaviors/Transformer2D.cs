/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Simple Component which transforms an object according to events from gestures.
    /// </summary>
    public class Transformer2D : MonoBehaviour
    {
        #region Unity fields

        /// <summary>
        /// Max movement speed
        /// </summary>
        public float Speed = 10f;

        #endregion

        #region Private variables

        private Vector3 localPositionToGo, localScaleToGo;
        private Quaternion localRotationToGo;
		
		private Vector3 lastLocalPosition, lastLocalScale;
		private Quaternion lastLocalRotation;

        #endregion

        #region Unity

        private void Start()
        {
            setDefaults();

            if (GetComponent<PanGesture>() != null)
            {
                GetComponent<PanGesture>().StateChanged += onPanStateChanged;
            }
            if (GetComponent<ScaleGesture>() != null)
            {
                GetComponent<ScaleGesture>().StateChanged += onScaleStateChanged;
            }
            if (GetComponent<RotateGesture>() != null)
            {
                GetComponent<RotateGesture>().StateChanged += onRotateStateChanged;
            }
        }

        private void Update()
        {
            var fraction = Speed*Time.deltaTime;
			if (transform.localPosition != lastLocalPosition) { // changed by some other code
				localPositionToGo = transform.localPosition;
			}
            transform.localPosition = lastLocalPosition = Vector3.Lerp(transform.localPosition, localPositionToGo, fraction);		
			if (transform.localScale != lastLocalScale) {
				localScaleToGo = transform.localScale;
			}
            transform.localScale = lastLocalScale = Vector3.Lerp(transform.localScale, localScaleToGo, fraction);		
			if (transform.localRotation != lastLocalRotation) {
				localRotationToGo = transform.localRotation;
			}
            transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotationToGo, fraction);
        }

        #endregion

        #region Private functions

        private void setDefaults()
        {
            localPositionToGo = lastLocalPosition = transform.localPosition;
			localRotationToGo = lastLocalRotation = transform.localRotation;
            localScaleToGo = lastLocalScale = transform.localScale;
        }

        private void onPanStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            var gesture = (PanGesture)sender;

            if (gesture.LocalDeltaPosition != Vector3.zero)
            {
                localPositionToGo += gesture.LocalDeltaPosition;
            }
        }

        #endregion

        #region Event handlers

        private void onRotateStateChanged(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
        {
            var gesture = (RotateGesture)sender;

            if (Math.Abs(gesture.LocalDeltaRotation) > 0.01)
            {
                localRotationToGo = Quaternion.AngleAxis(gesture.LocalDeltaRotation, gesture.WorldTransformPlane.normal)*localRotationToGo;
            }
        }

        private void onScaleStateChanged(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
        {
            var gesture = (ScaleGesture)sender;

            if (Math.Abs(gesture.LocalDeltaScale - 1) > 0.00001)
            {
                localScaleToGo *= gesture.LocalDeltaScale;
            }
        }

        #endregion
    }
}