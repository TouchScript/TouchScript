/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Timers;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Gesture which recognizes a point cluster which didn't move for specified time since it appeared.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Long Press Gesture")]
    public class LongPressGesture : Gesture
    {
        #region Private fields

        [SerializeField]
        private int maxTouches = int.MaxValue;

        [SerializeField]
        private float timeToPress = 1;

        [SerializeField]
        private float distanceLimit = float.PositiveInfinity;

        private Vector2 totalMovement;
        private Timer timer = new Timer();
        private bool fireRecognizedNextUpdate = false;

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
        /// Maximum distance in cm cluster can move before gesture fails.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            timer.Elapsed += onTimerElapsed;
            timer.AutoReset = false;
        }

        /// <inheritdoc />
        protected void Update()
        {
            if (fireRecognizedNextUpdate)
            {
                RaycastHit hit;
                if (base.GetTargetHitResult(out hit))
                {
                    setState(GestureState.Ended);
                } else
                {
                    setState(GestureState.Failed);
                }
            }
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (activeTouches.Count > MaxTouches)
            {
                setState(GestureState.Failed);
                return;
            }
            if (ActiveTouches.Count == touches.Count)
            {
                timer.Interval = TimeToPress*1000;
                timer.Start();
                setState(GestureState.Began);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            totalMovement += Clusters.Clusters.Get2DCenterPosition(touches) - Clusters.Clusters.GetPrevious2DCenterPosition(touches);
            if (totalMovement.magnitude/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit)
            {
                setState(GestureState.Failed);
            } else
            {
                setState(GestureState.Changed);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == 0)
            {
                timer.Stop();
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            reset();
        }

        /// <inheritdoc />
        protected override void reset()
        {
            fireRecognizedNextUpdate = false;
            timer.Stop();
        }

        #endregion

        #region Event handlers

        private void onTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            fireRecognizedNextUpdate = true;
        }

        #endregion
    }
}