/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_TapGesture.htm")]
    public class TapGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string TAP_MESSAGE = "OnTap";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> Tapped
        {
            add { tappedInvoker += value; }
            remove { tappedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> tappedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the number of taps required for the gesture to recognize.
        /// </summary>
        /// <value> The number of taps required for this gesture to recognize. <c>1</c> — dingle tap, <c>2</c> — double tap. </value>
        public int NumberOfTapsRequired
        {
            get { return numberOfTapsRequired; }
            set
            {
                if (value <= 0) numberOfTapsRequired = 1;
                else numberOfTapsRequired = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum hold time before gesture fails.
        /// </summary>
        /// <value> Number of seconds a user should hold their fingers before gesture fails. </value>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Gets or sets maximum distance for touch cluster must move for the gesture to fail.
        /// </summary>
        /// <value> Distance in cm touches must move before gesture fails. </value>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set
            {
                distanceLimit = value;
                distanceLimitInPixelsSquared = Mathf.Pow(distanceLimit * touchManager.DotsPerCentimeter, 2);
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private int numberOfTapsRequired = 1;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float timeLimit =
            float.PositiveInfinity;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float distanceLimit =
            float.PositiveInfinity;

        private float distanceLimitInPixelsSquared;

        private bool isActive = false;
        private int tapsDone;
        private Vector2 startPosition;
        private Vector2 totalMovement;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            distanceLimitInPixelsSquared = Mathf.Pow(distanceLimit * touchManager.DotsPerCentimeter, 2);
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (touchesNumState == TouchesNumState.PassedMaxThreshold ||
                touchesNumState == TouchesNumState.PassedMinMaxThreshold)
            {
                setState(GestureState.Failed);
                return;
            }

            if (NumTouches == touches.Count)
            {
                // the first ever touch
                if (tapsDone == 0)
                {
                    startPosition = touches[0].Position;
                    if (timeLimit < float.PositiveInfinity) StartCoroutine("wait");
                }
                else if (tapsDone >= numberOfTapsRequired) // Might be delayed and retapped while waiting
                {
                    setState(GestureState.Possible);
                    reset();
                    startPosition = touches[0].Position;
                    if (timeLimit < float.PositiveInfinity) StartCoroutine("wait");
                }
                else
                {
                    if (distanceLimit < float.PositiveInfinity)
                    {
                        if ((touches[0].Position - startPosition).sqrMagnitude > distanceLimitInPixelsSquared)
                        {
                            setState(GestureState.Failed);
                            return;
                        }
                    }
                }
            }
            if (touchesNumState == TouchesNumState.PassedMinThreshold)
            {
                // Starting the gesture when it is already active? => we released one finger and pressed again
                if (isActive) setState(GestureState.Failed);
                else isActive = true;
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (distanceLimit < float.PositiveInfinity)
            {
                totalMovement += touches[0].Position - touches[0].PreviousPosition;
                if (totalMovement.sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (NumTouches == 0)
            {
                if (!isActive)
                {
                    setState(GestureState.Failed);
                    return;
                }

                // touches outside of gesture target are ignored in shouldCacheTouchPosition()
                // if all touches are outside ScreenPosition will be invalid
                if (TouchManager.IsInvalidPosition(ScreenPosition))
                {
                    setState(GestureState.Failed);
                }
                else
                {
                    tapsDone++;
                    isActive = false;
                    if (tapsDone >= numberOfTapsRequired) setState(GestureState.Recognized);
                }
            }
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();

            StopCoroutine("wait");
            if (tappedInvoker != null) tappedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(TAP_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            isActive = false;
            totalMovement = Vector2.zero;
            StopCoroutine("wait");
            tapsDone = 0;
        }

        /// <inheritdoc />
        protected override bool shouldCacheTouchPosition(TouchPoint value)
        {
            // Points must be over target when released
            return GetTargetHitResult(value.Position);
        }

        #endregion

        #region private functions

        private IEnumerator wait()
        {
            // WaitForSeconds is affected by time scale!
            var targetTime = Time.unscaledTime + TimeLimit;
            while (targetTime > Time.unscaledTime) yield return null;

            if (State == GestureState.Possible) setState(GestureState.Failed);
        }

        #endregion
    }
}