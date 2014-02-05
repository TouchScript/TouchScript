/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Events;
using TouchScript.Gestures;
using TouchScript.Gestures.Simple;
using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Simple Component which transforms an object according to events from gestures.
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/Transformer2D")]
    public class Transformer2D : MonoBehaviour
    {
        #region Public properties

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

        #region Unity methods

        private void Start()
        {
            setDefaults();

            if (GetComponent<SimplePanGesture>() != null)
            {
                GetComponent<SimplePanGesture>().StateChanged += panStateChanged;
            }
            if (GetComponent<SimpleScaleGesture>() != null)
            {
                GetComponent<SimpleScaleGesture>().StateChanged += scaleStateChanged;
            }
            if (GetComponent<SimpleRotateGesture>() != null)
            {
                GetComponent<SimpleRotateGesture>().StateChanged += rotateStateChanged;
            }
        }

        private void Update()
        {
            var fraction = Speed*Time.deltaTime;
            if (transform.localPosition != lastLocalPosition)
            {
                // changed by someone else
                localPositionToGo = transform.localPosition;
            }
            transform.localPosition = lastLocalPosition = Vector3.Lerp(transform.localPosition, localPositionToGo, fraction);
            if (transform.localScale != lastLocalScale)
            {
                localScaleToGo = transform.localScale;
            }
            transform.localScale = lastLocalScale = Vector3.Lerp(transform.localScale, localScaleToGo, fraction);
            if (transform.localRotation != lastLocalRotation)
            {
                localRotationToGo = transform.localRotation;
            }
            transform.localRotation = lastLocalRotation = Quaternion.Lerp(transform.localRotation, localRotationToGo, fraction);
        }

        #endregion

        #region Private functions

        private void setDefaults()
        {
            localPositionToGo = lastLocalPosition = transform.localPosition;
            localRotationToGo = lastLocalRotation = transform.localRotation;
            localScaleToGo = lastLocalScale = transform.localScale;
        }

        #endregion

        #region Event handlers

        private void panStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:
                    var gesture = (SimplePanGesture)sender;

                    if (gesture.LocalDeltaPosition != Vector3.zero)
                    {
                        localPositionToGo += gesture.LocalDeltaPosition;
                    }
                    break;
            }
        }

        private void rotateStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:
                    var gesture = (SimpleRotateGesture)sender;

                    if (Math.Abs(gesture.LocalDeltaRotation) > 0.01)
                    {
                        if (transform.parent == null)
                        {
                            localRotationToGo = Quaternion.AngleAxis(gesture.LocalDeltaRotation, gesture.WorldTransformPlane.normal)*localRotationToGo;
                        } else
                        {
                            localRotationToGo = Quaternion.AngleAxis(gesture.LocalDeltaRotation, transform.parent.InverseTransformDirection(gesture.WorldTransformPlane.normal))*localRotationToGo;
                        }
                    }
                    break;
            }
        }

        private void scaleStateChanged(object sender, GestureStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:
                    var gesture = (SimpleScaleGesture)sender;

                    if (Math.Abs(gesture.LocalDeltaScale - 1) > 0.00001)
                    {
                        localScaleToGo *= gesture.LocalDeltaScale;
                    }
                    break;
            }
        }

        #endregion
    }
}