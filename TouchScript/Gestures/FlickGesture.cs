/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes fast movement before releasing touches.
    /// Doesn't care how much time touch points were on surface and how much they moved.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Flick Gesture")]
    public class FlickGesture : Gesture
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
        private float flickTime = .1f;

        [SerializeField]
        private float minDistance = 1f;

        [SerializeField]
        private float movementThreshold = .5f;

        [SerializeField]
        private GestureDirection direction = GestureDirection.Any;

        private bool moving = false;
        private Vector2 movementBuffer = Vector2.zero;
        private bool isActive = false;

        private List<Vector2> positionDeltas = new List<Vector2>();
        private List<float> timeDeltas = new List<float>();

        #endregion

        #region Public properties

        /// <summary>
        /// Time interval in seconds in which touch points must move by <see cref="MinDistance"/>.
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
        /// Minimum distance in cm for touch points to move for gesture to be recognized. 
        /// Prevents misinterpreting taps.
        /// </summary>
        public float MovementThreshold
        {
            get { return movementThreshold; }
            set { movementThreshold = value; }
        }

        /// <summary>
        /// Direction to look for.
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

        public float ScreenFlickTime { get; private set; }

        #endregion

        protected void LateUpdate()
        {
            if (!isActive) return;

            positionDeltas.Add(ScreenPosition - PreviousScreenPosition);
            timeDeltas.Add(Time.deltaTime);
        }

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count == touches.Count)
            {
                isActive = true;
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (!moving)
            {
                movementBuffer += ScreenPosition - PreviousScreenPosition;
                var dpiMovementThreshold = MovementThreshold*touchManager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude >= dpiMovementThreshold*dpiMovementThreshold)
                {
                    moving = true;
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
            {
                isActive = false;

                if (!moving)
                {
                    setState(GestureState.Failed);
                    return;
                }

                positionDeltas.Add(Cluster.Get2DCenterPosition(touches) - Cluster.GetPrevious2DCenterPosition(touches));
                timeDeltas.Add(Time.deltaTime);

                var totalTime = 0f;
                var totalMovement = Vector2.zero;
                var i = timeDeltas.Count - 1;
                while (i >= 0 && totalTime < FlickTime)
                {
                    totalTime += timeDeltas[i];
                    totalMovement += positionDeltas[i];
                    i--;
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

                if (totalMovement.magnitude < MinDistance * TouchManager.Instance.DotsPerCentimeter)
                {
                    setState(GestureState.Failed);
                }
                else
                {
                    ScreenFlickVector = totalMovement;
                    ScreenFlickTime = totalTime;
                    setState(GestureState.Recognized);
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            base.touchesCancelled(touches);

            touchesEnded(touches);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            isActive = false;
            moving = false;
            movementBuffer = Vector2.zero;
            positionDeltas.Clear();
            timeDeltas.Clear();
        }

        #endregion
    }
}