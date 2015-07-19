/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Base class for transform gestures involving multiple points/clusters.
    /// </summary>
    public abstract class MultiPointTransform2DGestureBase : Transform2DGestureBase
    {
        #region Public properties

        /// <summary>
        /// Minimum points count for gesture to begin.
        /// </summary>
        public virtual int MinPointsCount
        {
            get { return minPointsCount; }
            set { minPointsCount = value; }
        }

        /// <summary>
        /// Minimum distance between each point in cm for gesture to begin.
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
        protected int minPointsCount = 2;

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
        /// <returns>True if there are enough active touch points, False otherwise.</returns>
        protected virtual bool gotEnoughTouches()
        {
            return activeTouches.Count >= minPointsCount;
        }
        
        /// <summary>
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches">List of touch points</param>
        /// <returns>True if there are relevant touch points, False otherwise.</returns>
        protected virtual bool relevantTouches(IList<ITouch> touches)
        {
            var result = false;
 
            foreach (var touch in touches)
            {
                foreach(var activeTouch in activeTouches)
                {
                    if (touch == activeTouch)
                    {
                        result = true;
                        break;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns screen position of a point with specified index
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointScreenPosition(int index)
        {
            if (index < 0) index = 0;
            return activeTouches[index].Position;
        }
        
        /// <summary>
        /// Returns previous screen position of a point with specified index
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            if (index < 0) index = 0;
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
            
            if (activeTouches.Count < minPointsCount && (State == GestureState.Began || State == GestureState.Changed))
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
