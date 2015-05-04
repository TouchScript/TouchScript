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
    /// Gesture which recognizes a point cluster which didn't move for specified time since it appeared.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Long Press Gesture")]
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

        // iOS Events AOT hack
        private EventHandler<EventArgs> longPressedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Maximum number of simultaneous touch points.
        /// </summary>
        public int MaxTouches
        {
            get { return maxTouches; }
            set { maxTouches = value; }
        }

        /// <summary>
        /// Total time in seconds required to hold touches still.
        /// </summary>
        public float TimeToPress
        {
            get { return timeToPress; }
            set { timeToPress = value; }
        }

        /// <summary>
        /// Maximum distance in cm touch points can move before gesture fails.
        /// </summary>
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
        [NullToggle(NullIntValue = int.MaxValue)]
        private int maxTouches = int.MaxValue;

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
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count > MaxTouches)
            {
                setState(GestureState.Failed);
                return;
            }

            if (activeTouches.Count == touches.Count)
            {
                StartCoroutine("wait");
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            if (distanceLimit < float.PositiveInfinity)
            {
                totalMovement += ScreenPosition - PreviousScreenPosition;
                if (totalMovement.sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouch> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
            {
                StopCoroutine("wait");
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            longPressedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
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
