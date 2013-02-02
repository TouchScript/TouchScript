/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Flick gesture.
    /// Recognizes fast movement before releasing touches.
    /// Doesn't care how much time touch points were on surface and how much they moved.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Flick Gesture")]
    public class FlickGesture : Gesture
    {
        #region Unity fields

        /// <summary>
        /// Time interval in seconds in which points cluster must move by <see cref="MinDistance"/>.
        /// </summary>
        public float FlickTime = .5f;

        /// <summary>
        /// Minimum distance in cm to move in <see cref="FlickTime"/> before ending gesture for it to be recognized.
        /// </summary>
        public float MinDistance = 1f;

        /// <summary>
        /// Minimum distance in cm for cluster to move to be considered as a possible gesture. 
        /// Prevents misinterpreting taps.
        /// </summary>
        public float MovementThreshold = 0.5f;
		
		public enum GestureDirection {
			Any,
			Horizontal,
			Vertical,
		}
		public GestureDirection Direction;

        #endregion

        #region Private variables

        private bool moving = false;
        private Vector2 movementBuffer = Vector2.zero;
        private List<Vector2> positionDeltas = new List<Vector2>();
        private List<float> timeDeltas = new List<float>();
        private float previousTime;

        #endregion

        #region Public properties

        /// <summary>
        /// Contains flick direction (not normalized) when gesture is recognized.
        /// </summary>
        public Vector2 ScreenFlickVector { get; private set; }

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (activeTouches.Count == touches.Count)
            {
                previousTime = Time.time;
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            var delta = Cluster.Get2DCenterPosition(touches) - Cluster.GetPrevious2DCenterPosition(touches);
            if (!moving)
            {
                movementBuffer += delta;
                var dpiMovementThreshold = MovementThreshold*Manager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude >= dpiMovementThreshold*dpiMovementThreshold)
                {
                    moving = true;
                }
            }

            positionDeltas.Add(delta);
            timeDeltas.Add(Time.time - previousTime);
            previousTime = Time.time;
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (activeTouches.Count == 0)
            {
                if (!moving)
                {
                    setState(GestureState.Failed);
                    return;
                }

                var totalTime = 0f;
                var totalMovement = Vector2.zero;
                var i = timeDeltas.Count - 1;
                while (i >= 0 && totalTime < FlickTime)
                {
                    if (totalTime + timeDeltas[i] < FlickTime)
                    {
                        totalTime += timeDeltas[i];
                        totalMovement += positionDeltas[i];
                        i--;
                    } else
                    {
                        break;
                    }
                }

                switch(Direction) {
				case GestureDirection.Horizontal:
					totalMovement.y = 0;
					break;
				case GestureDirection.Vertical:
					totalMovement.x = 0;
					break;
				}

                if (totalMovement.magnitude < MinDistance*TouchManager.Instance.DotsPerCentimeter)
                {
                    setState(GestureState.Failed);
                } else
                {
                    ScreenFlickVector = totalMovement;
                    setState(GestureState.Recognized);
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            touchesEnded(touches);
        }

        protected override void reset()
        {
            moving = false;
            movementBuffer = Vector2.zero;
            timeDeltas.Clear();
            positionDeltas.Clear();
        }

        #endregion
    }
}