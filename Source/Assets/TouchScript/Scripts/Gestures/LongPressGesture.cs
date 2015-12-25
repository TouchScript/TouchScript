/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Gesture which recognizes a point cluster which didn't move for specified time since it appeared.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Long Press Gesture")]
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Gestures_LongPressGesture.htm")]
    public class LongPressGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string LONG_PRESS_MESSAGE = "OnLongPress";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> LongPressed
        {
            add { longPressedInvoker += value; }
            remove { longPressedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> longPressedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets total time in seconds required to hold touches still.
        /// </summary>
        /// <value> Time in seconds. </value>
        public float TimeToPress
        {
            get { return timeToPress; }
            set { timeToPress = value; }
        }

        /// <summary>
        /// Gets or sets maximum distance in cm touch points can move before gesture fails.
        /// </summary>
        /// <value> Distance in cm. </value>
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
        private float timeToPress = 1;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float distanceLimit = float.PositiveInfinity;

        private float distanceLimitInPixelsSquared;

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
        protected override void touchBegan(TouchPoint touch)
        {
            base.touchBegan(touch);

            if (touchesNumState == TouchesNumState.PassedMaxThreshold ||
                touchesNumState == TouchesNumState.PassedMinMaxThreshold)
            {
                setState(GestureState.Failed);
            }
            else if (touchesNumState == TouchesNumState.PassedMinThreshold)
            {
                StartCoroutine("wait");
            }
        }

        /// <inheritdoc />
        protected override void touchMoved(TouchPoint touch)
        {
            base.touchMoved(touch);

            if (distanceLimit < float.PositiveInfinity)
            {
                totalMovement += ScreenPosition - PreviousScreenPosition;
                if (totalMovement.sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchEnded(TouchPoint touch)
        {
            base.touchEnded(touch);

            if (touchesNumState == TouchesNumState.PassedMinThreshold)
            {
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (longPressedInvoker != null) longPressedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(LONG_PRESS_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            totalMovement = Vector2.zero;
            StopCoroutine("wait");
        }

        #endregion

        #region Private functions

        private IEnumerator wait()
        {
            // WaitForSeconds is affected by time scale!
            var targetTime = Time.unscaledTime + TimeToPress;
            while (targetTime > Time.unscaledTime) yield return null;

            if (State == GestureState.Possible)
            {
                if (base.GetTargetHitResult())
                {
                    setState(GestureState.Recognized);
                }
                else
                {
                    setState(GestureState.Failed);
                }
            }
        }

        #endregion
    }
}