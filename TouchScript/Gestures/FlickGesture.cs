/*
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
    public class FlickGesture : ClusterBasedGesture
    {
        #region Unity fields

        /// <summary>
        /// Direction of a flick.
        /// </summary>
        public enum GestureDirection
        {
            /// <summary>
            /// Direction doesn't matter.
            /// </summary>
            Any,
            /// <summary>
            /// Only horizontal.
            /// </summary>
            Horizontal,
            /// <summary>
            /// Only vertical.
            /// </summary>
            Vertical,
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float flickTime = .5f;

        [SerializeField]
        private float minDistance = 1f;

        [SerializeField]
        private float movementThreshold = .5f;

        [SerializeField]
        private GestureDirection direction = GestureDirection.Any;

        private bool moving = false;
        private Vector2 movementBuffer = Vector2.zero;
        private List<Vector2> positionDeltas = new List<Vector2>();
        private List<float> timeDeltas = new List<float>();
        private float previousTime;

        #endregion

        #region Public properties

        /// <summary>
        /// Time interval in seconds in which points cluster must move by <see cref="MinDistance"/>.
        /// </summary>
        public float FlickTime
        {
            get { return flickTime; }
            set { flickTime = value; }
        }

        /// <summary>
        /// Minimum distance in cm to move in <see cref="FlickTime"/> before ending gesture for it to be recognized.
        /// </summary>
        public float MinDistance
        {
            get { return minDistance; }
            set { minDistance = value; }
        }

        /// <summary>
        /// Minimum distance in cm for cluster to move to be considered as a possible gesture. 
        /// Prevents misinterpreting taps.
        /// </summary>
        public float MovementThreshold
        {
            get { return movementThreshold; }
            set { movementThreshold = value; }
        }

        /// <summary>
        /// Minimum distance in cm for cluster to move to be considered as a possible gesture. 
        /// Prevents misinterpreting taps.
        /// </summary>
        public GestureDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        /// <summary>
        /// Contains flick direction (not normalized) when gesture is recognized.
        /// </summary>
        public Vector2 ScreenFlickVector { get; private set; }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (activeTouches.Count == touches.Count)
            {
                previousTime = Time.time;
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            var delta = Clusters.Clusters.Get2DCenterPosition(touches) - Clusters.Clusters.GetPrevious2DCenterPosition(touches);
            if (!moving)
            {
                movementBuffer += delta;
                var dpiMovementThreshold = MovementThreshold*manager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude >= dpiMovementThreshold*dpiMovementThreshold)
                {
                    moving = true;
                }
            }

            positionDeltas.Add(delta);
            timeDeltas.Add(Time.time - previousTime);
            previousTime = Time.time;
        }

        /// <inheritdoc />
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

                switch (Direction)
                {
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

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            touchesEnded(touches);
        }

        /// <inheritdoc />
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