/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Base class for transform gestures involving two clusters.
    /// </summary>
    public abstract class TwoPointTransform2DGestureBase : Transform2DGestureBase
    {
        #region Public properties

        /// <summary>
        /// Minimum distance between 2 points in cm for gesture to be recognized.
        /// </summary>
        public virtual float MinPointDistance
        {
            get { return minPointDistance; }
            set
            {
                minPointDistance = value;
            }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get { return screenPosition; }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get { return previousScreenPosition; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float minPointDistance = .5f;

        /// <summary>
        /// Transform's center point screen position.
        /// </summary>
        protected Vector2 screenPosition;
        /// <summary>
        /// Transform's center point previous screen position.
        /// </summary>
        protected Vector3 previousScreenPosition;

        #endregion

        protected virtual bool gotEnoughTouchPoints()
        {
            return activeTouches.Count >= 2;
        }

        protected virtual Vector2 getPointScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return activeTouches[index].Position;
        }

        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return activeTouches[index].PreviousPosition;
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);
            if (activeTouches.Count < 2 && (State == GestureState.Began || State == GestureState.Changed))
            {
                setState(GestureState.Ended);
            }
        }

    }
}