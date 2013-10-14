/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Base class for transform gestures involving two points/clusters.
    /// </summary>
    public abstract class TwoPointTransform2DGestureBase : Transform2DGestureBase
    {
        #region Public properties

        /// <summary>
        /// Minimum distance between 2 points in cm for gesture to begin.
        /// </summary>
        public virtual float MinPointsDistance
        {
            get { return minPointsDistance; }
            set
            {
                minPointsDistance = value;
                minPointsDistanceInPixels = value*TouchManager.Instance.DotsPerCentimeter;
            }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (TouchPoint.IsInvalidPosition(screenPosition)) return base.ScreenPosition;
                return screenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (TouchPoint.IsInvalidPosition(previousScreenPosition)) return base.PreviousScreenPosition;
                return previousScreenPosition;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float minPointsDistance = .5f;

        /// <summary>
        /// <see cref="MinPointsDistance"/> in pixels for internal use.
        /// </summary>
        protected float minPointsDistanceInPixels;

        /// <summary>
        /// Transform's center point screen position.
        /// </summary>
        protected Vector2 screenPosition;

        /// <summary>
        /// Transform's center point previous screen position.
        /// </summary>
        protected Vector2 previousScreenPosition;

        #endregion

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            minPointsDistanceInPixels = minPointsDistance*TouchManager.Instance.DotsPerCentimeter;
        }

        /// <summary>
        /// Checks if gesture has enough touch points to be recognized.
        /// </summary>
        /// <returns>True if there are two or more active touch points, False otherwise.</returns>
        protected virtual bool gotEnoughTouchPoints()
        {
            return activeTouches.Count >= 2;
        }

        /// <summary>
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches">List of touch points</param>
        /// <returns>True if there are relevant touch points, False otherwise.</returns>
        protected virtual bool relevantTouchPoints(IList<TouchPoint> touches)
        {
            var result = false;
            // We care only about the first and the second touch points
            foreach (var touchPoint in touches)
            {
                if (touchPoint == activeTouches[0] || touchPoint == activeTouches[1])
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return activeTouches[index].Position;
        }

        /// <summary>
        /// Returns previous screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return activeTouches[index].PreviousPosition;
        }

        /// <summary>
        /// Restarts the gesture when it continues after being left only with one finger.
        /// </summary>
        protected virtual void restart()
        {
            screenPosition = TouchPoint.InvalidPosition;
            previousScreenPosition = TouchPoint.InvalidPosition;
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 1 && (State == GestureState.Began || State == GestureState.Changed))
            {
                restart();
            }
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            restart();
        }
    }
}