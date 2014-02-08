/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : Gesture
    {

        #region Constants

        public const string TAPPED_MESSAGE = "OnTapped";

        #endregion

        #region Public properties

        /// <summary>
        /// Maximum time to hold touches until gesture fails.
        /// </summary>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture fails.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float timeLimit = float.PositiveInfinity;

        [SerializeField]
        private float distanceLimit = float.PositiveInfinity;

        private Vector2 totalMovement = Vector2.zero;
        private float startTime;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count == touches.Count)
            {
                startTime = Time.time;
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            totalMovement += ScreenPosition - PreviousScreenPosition;
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
            {
                if (totalMovement.magnitude / touchManager.DotsPerCentimeter >= DistanceLimit || Time.time - startTime > TimeLimit)
                {
                    setState(GestureState.Failed);
                    return;
                }

                if (TouchPoint.IsInvalidPosition(ScreenPosition))
                {
                    setState(GestureState.Failed);
                } else
                {
                    setState(GestureState.Recognized);
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            base.touchesCancelled(touches);

            setState(GestureState.Failed);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (UseSendMessage) SendMessageTarget.SendMessage(TAPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            totalMovement = Vector2.zero;
        }

        /// <inheritdoc />
        protected override bool shouldCacheTouchPointPosition(TouchPoint value)
        {
            // Points must be over target when released
            return GetTargetHitResult(value.Position);
        }

        #endregion
    }
}