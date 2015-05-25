/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
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

        /// <summary>Max movement speed.</summary>
        public float Speed = 10f;

        /// <summary>Controls pan speed.</summary>
        public float PanMultiplier = 1f;

        public bool AllowChangingFromOutside = true;

        #endregion

        #region Private variables

        private Vector3 localPositionToGo, localScaleToGo;
        private Quaternion localRotationToGo;

        // last* variables are needed to detect when Transform's properties were changed outside of this script
        private Vector3 lastLocalPosition, lastLocalScale;
        private Quaternion lastLocalRotation;

        #endregion

        #region Unity methods

        private void Awake()
        {
            setDefaults();
        }

        private void OnEnable()
        {
            if (GetComponent<SimplePanGesture>() != null) GetComponent<SimplePanGesture>().Panned += panHandler;
            if (GetComponent<SimpleScaleGesture>() != null) GetComponent<SimpleScaleGesture>().Scaled += scaleHandler;
            if (GetComponent<SimpleRotateGesture>() != null) GetComponent<SimpleRotateGesture>().Rotated += rotateHandler;
        }

        private void OnDisable()
        {
            if (GetComponent<SimplePanGesture>() != null) GetComponent<SimplePanGesture>().Panned -= panHandler;
            if (GetComponent<SimpleScaleGesture>() != null) GetComponent<SimpleScaleGesture>().Scaled -= scaleHandler;
            if (GetComponent<SimpleRotateGesture>() != null) GetComponent<SimpleRotateGesture>().Rotated -= rotateHandler;
        }

        private void Update()
        {
            var fraction = Speed * Time.deltaTime;

            if (AllowChangingFromOutside)
            {
                // changed by someone else
                if (!Mathf.Approximately(transform.localPosition.x, lastLocalPosition.x))
                    localPositionToGo.x = transform.localPosition.x;
                if (!Mathf.Approximately(transform.localPosition.y, lastLocalPosition.y))
                    localPositionToGo.y = transform.localPosition.y;
                if (!Mathf.Approximately(transform.localPosition.z, lastLocalPosition.z))
                    localPositionToGo.z = transform.localPosition.z;
            }
            transform.localPosition = lastLocalPosition = Vector3.Lerp(transform.localPosition, localPositionToGo, fraction);

            if (AllowChangingFromOutside)
            {
                // changed by someone else
                if (!Mathf.Approximately(transform.localScale.x, lastLocalScale.x))
                    localScaleToGo.x = transform.localScale.x;
                if (!Mathf.Approximately(transform.localScale.y, lastLocalScale.y))
                    localScaleToGo.y = transform.localScale.y;
                if (!Mathf.Approximately(transform.localScale.z, lastLocalScale.z))
                    localScaleToGo.z = transform.localScale.z;
            }
            var newLocalScale = Vector3.Lerp(transform.localScale, localScaleToGo, fraction);
            // prevent recalculating colliders when no scale occurs
            if (newLocalScale != transform.localScale) transform.localScale = lastLocalScale = newLocalScale;

            if (AllowChangingFromOutside)
            {
                // changed by someone else
                if (transform.localRotation != lastLocalRotation) localRotationToGo = transform.localRotation;
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

        private void panHandler(object sender, EventArgs e)
        {
            var gesture = (SimplePanGesture)sender;

            localPositionToGo += gesture.LocalDeltaPosition * PanMultiplier;
        }

        private void rotateHandler(object sender, EventArgs e)
        {
            var gesture = (SimpleRotateGesture)sender;

            if (Math.Abs(gesture.DeltaRotation) > 0.01)
            {
                if (transform.parent == null)
                {
                    localRotationToGo = Quaternion.AngleAxis(gesture.DeltaRotation, gesture.RotationAxis) * localRotationToGo;
                }
                else
                {
                    localRotationToGo = Quaternion.AngleAxis(gesture.DeltaRotation, transform.parent.InverseTransformDirection(gesture.RotationAxis)) * localRotationToGo;
                }
            }
        }

        private void scaleHandler(object sender, EventArgs e)
        {
            var gesture = (SimpleScaleGesture)sender;

            if (Math.Abs(gesture.LocalDeltaScale - 1) > 0.00001)
            {
                localScaleToGo *= gesture.LocalDeltaScale;
            }
        }

        #endregion
    }
}
