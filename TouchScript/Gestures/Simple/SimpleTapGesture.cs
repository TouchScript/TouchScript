/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{

    /// <summary>
    /// Simple Tap gesture which is only concerned about one finger
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Simple Tap Gesture")]
    public class SimpleTapGesture : Gesture
    {

        #region Private variables

        [SerializeField]
        private float timeLimit = float.PositiveInfinity;

        [SerializeField]
        private float distanceLimit = float.PositiveInfinity;

        /// <summary>
        /// The cached screen position
        /// </summary>
        protected Vector2 cachedScreenPosition;
        /// <summary>
        /// The cached previous screen position
        /// </summary>
        protected Vector2 cachedPreviousScreenPosition;
        /// <summary>
        /// The cached target hit result
        /// </summary>
        protected RaycastHit cachedTargetHitResult;

        private Vector2 totalMovement = Vector2.zero;
        private float startTime;

        #endregion

        #region Public properties

        /// <summary>
        /// Maximum time to hold touches until gesture is considered to be failed.
        /// </summary>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture is considered to be failed.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (cachedScreenPosition == TouchPoint.InvalidPosition) return base.ScreenPosition;
                return cachedScreenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (cachedScreenPosition == TouchPoint.InvalidPosition) return base.PreviousScreenPosition;
                return cachedPreviousScreenPosition;
            }
        }

        #endregion

        /// <inheritdoc />
        public override bool GetTargetHitResult(out RaycastHit hit)
        {
            if (State == GestureState.Ended)
            {
                hit = cachedTargetHitResult;
                return true;
            }

            return base.GetTargetHitResult(out hit);
        }

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == touches.Count)
            {
                startTime = Time.time;
                setState(GestureState.Began);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            totalMovement += ScreenPosition - PreviousScreenPosition;
            setState(GestureState.Changed);
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == 0)
            {
                if (totalMovement.magnitude / TouchManager.Instance.DotsPerCentimeter >= DistanceLimit || Time.time - startTime > TimeLimit)
                {
                    setState(GestureState.Failed);
                    return;
                }

                updateCachedScreenPosition(touches);

                if (base.GetTargetHitResult(out cachedTargetHitResult))
                {
                    setState(GestureState.Ended);
                }
                else
                {
                    setState(GestureState.Failed);
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            setState(GestureState.Failed);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            totalMovement = Vector2.zero;
            cachedScreenPosition = TouchPoint.InvalidPosition;
            cachedPreviousScreenPosition = TouchPoint.InvalidPosition;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Updates the cached screen position.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void updateCachedScreenPosition(IList<TouchPoint> touches)
        {
            TouchPoint point = touches[touches.Count - 1];
            cachedScreenPosition = point.Position;
            cachedPreviousScreenPosition = point.PreviousPosition;
        }

        #endregion

    }
}
