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
                minPixelDistance = minPointsDistance * touchManager.DotsPerCentimeter;
                minPixelDistanceSquared = Mathf.Pow(minPixelDistance, 2);
            }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (TouchManager.IsInvalidPosition(screenPosition)) return base.ScreenPosition;
                return screenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (TouchManager.IsInvalidPosition(previousScreenPosition)) return base.PreviousScreenPosition;
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
        protected float minPixelDistance;

        /// <summary>
        /// <see cref="MinPointsDistance"/> squared in pixels for internal use.
        /// </summary>
        protected float minPixelDistanceSquared;

        /// <summary>
        /// Transform's center point screen position.
        /// </summary>
        protected Vector2 screenPosition;

        /// <summary>
        /// Transform's center point previous screen position.
        /// </summary>
        protected Vector2 previousScreenPosition;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            minPixelDistance = minPointsDistance * touchManager.DotsPerCentimeter;
            minPixelDistanceSquared = Mathf.Pow(minPixelDistance, 2);
        }

        #endregion

        #region Protected functions

        /// <summary>
        /// Checks if gesture has enough touch points to be recognized.
        /// </summary>
        /// <returns>True if there are two or more active touch points, False otherwise.</returns>
        protected virtual bool gotEnoughTouches()
        {
            return activeTouches.Count >= 2;
        }

        /// <summary>
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches">List of touch points</param>
        /// <returns>True if there are relevant touch points, False otherwise.</returns>
        protected virtual bool relevantTouches(IList<ITouch> touches)
        {
            var result = false;
            // We care only about the first and the second touch points
            foreach (var touch in touches)
            {
                if (touch == activeTouches[0] || touch == activeTouches[1])
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
            screenPosition = TouchManager.INVALID_POSITION;
            previousScreenPosition = TouchManager.INVALID_POSITION;
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouch> touches)
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

        #endregion
    }
}
