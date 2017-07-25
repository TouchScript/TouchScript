/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Gestures.TransformGestures.Base;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Component which transforms an object according to events from transform gestures: <see cref="TransformGesture"/>, <see cref="ScreenTransformGesture"/>, <see cref="PinnedTransformGesture"/> and others.
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/Transformer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Transformer.htm")]
    public class Transformer : MonoBehaviour
    {

        #region Consts

        private enum TransformerState
        {
            Idle,
            Manual,
            Automatic
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether Smoothing is enabled. Smoothing allows to reduce jagged movements but adds some visual lag.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Smoothing is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSmoothing
        {
            get { return enableSmoothing; }
            set { enableSmoothing = value; }
        }

        /// <summary>
        /// Gets or sets the smoothing factor.
        /// </summary>
        /// <value>
        /// The smoothing factor. Indicates how much smoothing to apply. 0 - no smoothing, 100000 - maximum.
        /// </value>
        public float SmoothingFactor
        {
            get { return smoothingFactor * 100000f; }
            set
            {
                smoothingFactor = Mathf.Clamp(value / 100000f, 0, 1);
            }
        }

        /// <summary>
        /// Gets or sets the position threshold.
        /// </summary>
        /// <value>
        /// Minimum distance between target position and smoothed position when to stop automatic movement.
        /// </value>
        public float PositionThreshold
        {
            get { return Mathf.Sqrt(positionThreshold); }
            set { positionThreshold = value * value; }
        }

        /// <summary>
        /// Gets or sets the rotation threshold.
        /// </summary>
        /// <value>
        /// Minimum angle between target rotation and smoothed rotation when to stop automatic movement.
        /// </value>
        public float RotationThreshold
        {
            get { return rotationThreshold; }
            set { rotationThreshold = value; }
        }

        /// <summary>
        /// Gets or sets the scale threshold.
        /// </summary>
        /// <value>
        /// Minimum difference between target scale and smoothed scale when to stop automatic movement.
        /// </value>
        public float ScaleThreshold
        {
            get { return Mathf.Sqrt(scaleThreshold); }
            set { scaleThreshold = value * value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this transform can be changed from another script.
        /// </summary>
        /// <value>
        /// <c>true</c> if this transform can be changed from another script; otherwise, <c>false</c>.
        /// </value>
        public bool AllowChangingFromOutside
        {
            get { return allowChangingFromOutside; }
            set { allowChangingFromOutside = value; }
        }

		#endregion

		#region Private variables

		[SerializeField]
        [ToggleLeft]
        private bool enableSmoothing = false;

        [SerializeField]
        private float smoothingFactor = 1f/100000f;

        [SerializeField]
        private float positionThreshold = 0.0001f; 

        [SerializeField]
        private float rotationThreshold = 0.01f;

        [SerializeField]
        private float scaleThreshold = 0.0001f;

        [SerializeField]
        [ToggleLeft]
        private bool allowChangingFromOutside = false;

        private TransformerState state;

        private TransformGestureBase gesture;
        private Transform cachedTransform;

        private Vector3 targetPosition, targetScale;
        private Quaternion targetRotation;

        // last* variables are needed to detect when Transform's properties were changed outside of this script
        private Vector3 lastPosition, lastScale;
        private Quaternion lastRotation;

        #endregion

        #region Unity methods

        private void Awake()
        {
            cachedTransform = transform;
        }

        private void OnEnable()
        {
            gesture = GetComponent<TransformGestureBase>();
            gesture.StateChanged += stateChangedHandler;
            TouchManager.Instance.FrameFinished += frameFinishedHandler;

            stateIdle();
        }

        private void OnDisable()
        {
            if (gesture != null) gesture.StateChanged -= stateChangedHandler;
            if (TouchManager.Instance != null)
                TouchManager.Instance.FrameFinished -= frameFinishedHandler;

            stateIdle();
        }

        #endregion

        #region States

        private void stateIdle()
        {
            var prevState = state;
            setState(TransformerState.Idle);

            if (enableSmoothing && prevState == TransformerState.Automatic)
            {
                transform.position = lastPosition = targetPosition;
                var newLocalScale = lastScale = targetScale;
                // prevent recalculating colliders when no scale occurs
                if (newLocalScale != transform.localScale) transform.localScale = newLocalScale;
                transform.rotation = lastRotation = targetRotation;
            }
        }

        private void stateManual()
        {
            setState(TransformerState.Manual);

            targetPosition = lastPosition = cachedTransform.position;
            targetRotation = lastRotation = cachedTransform.rotation;
            targetScale = lastScale = cachedTransform.localScale;
        }

        private void stateAutomatic()
        {
            setState(TransformerState.Automatic);

            if (!enableSmoothing) stateIdle();
        }

        private void setState(TransformerState newState)
        {
            state = newState;
        }

        #endregion

        #region Private functions

        private void update()
        {
            if (state == TransformerState.Idle) return;

            if (!enableSmoothing) return;

            var fraction = 1 - Mathf.Pow(smoothingFactor, Time.unscaledDeltaTime);

            var scale = transform.localScale;
            if (allowChangingFromOutside)
            {
                // Changed by someone else.
                // Need to make sure to check per component here.
                if (!Mathf.Approximately(scale.x, lastScale.x))
                    targetScale.x = scale.x;
                if (!Mathf.Approximately(scale.y, lastScale.y))
                    targetScale.y = scale.y;
                if (!Mathf.Approximately(scale.z, lastScale.z))
                    targetScale.z = scale.z;
            }
            var newLocalScale = Vector3.Lerp(scale, targetScale, fraction);
            // Prevent recalculating colliders when no scale occurs.
            if (newLocalScale != scale)
            {
                transform.localScale = newLocalScale;
                // Something might have adjusted our scale.
                lastScale = transform.localScale;
            }

            if (allowChangingFromOutside)
            {
                // Changed by someone else.
                if (transform.rotation != lastRotation) targetRotation = transform.rotation;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, fraction);
            // Something might have adjusted our rotation.
            lastRotation = transform.rotation;

            var pos = transform.position;
            if (allowChangingFromOutside)
            {
                // Changed by someone else.
                // Need to make sure to check per component here.
                if (!Mathf.Approximately(pos.x, lastPosition.x))
                    targetPosition.x = pos.x;
                if (!Mathf.Approximately(pos.y, lastPosition.y))
                    targetPosition.y = pos.y;
                if (!Mathf.Approximately(pos.z, lastPosition.z))
                    targetPosition.z = pos.z;
            }
            transform.position = Vector3.Lerp(pos, targetPosition, fraction);
            // Something might have adjusted our position (most likely Unity UI).
            lastPosition = transform.position;

            if (state == TransformerState.Automatic)
            {
                var dP = (targetPosition - lastPosition).sqrMagnitude;
                var dS = (targetScale - lastScale).sqrMagnitude;
                var dR = Quaternion.Angle(targetRotation, lastRotation);
                if (dP < positionThreshold && dR < rotationThreshold && dS < scaleThreshold) stateIdle();
            }
        }

        private void manualUpdate()
        {
			if (state != TransformerState.Manual) stateManual();

            var mask = gesture.TransformMask;
            if ((mask & TransformGesture.TransformType.Scaling) != 0) targetScale *= gesture.DeltaScale;
            if ((mask & TransformGesture.TransformType.Rotation) != 0)
                targetRotation = Quaternion.AngleAxis(gesture.DeltaRotation, gesture.RotationAxis) * targetRotation;
            if ((mask & TransformGesture.TransformType.Translation) != 0) targetPosition += gesture.DeltaPosition;

            gesture.OverrideTargetPosition(targetPosition);

            if (!enableSmoothing) applyValues();
        }

        private void applyValues()
        {
            cachedTransform.localScale = targetScale;
            cachedTransform.rotation = targetRotation;
            cachedTransform.position = targetPosition;
        }

        #endregion

        #region Event handlers

        private void stateChangedHandler(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
        {
            switch (gestureStateChangeEventArgs.State)
            {
                case Gesture.GestureState.Possible:
                    stateManual();
                    break;
                case Gesture.GestureState.Changed:
                    manualUpdate();
                    break;
                case Gesture.GestureState.Ended:
                case Gesture.GestureState.Cancelled:
                    stateAutomatic();
                    break;
                case Gesture.GestureState.Failed:
                    if (gestureStateChangeEventArgs.PreviousState == Gesture.GestureState.Possible)
                        stateAutomatic();
                    break;
            }
        }

        private void frameFinishedHandler(object sender, EventArgs eventArgs)
        {
            update();
        }

        #endregion
    }
}