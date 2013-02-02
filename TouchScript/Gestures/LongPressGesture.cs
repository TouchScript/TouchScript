/**
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
        #region Unity fields

        /// <summary>
        /// Maximum number of simultaneous touch points.
        /// </summary>
        public int MaxTouches = int.MaxValue;

        /// <summary>
        /// Total time in seconds required to hold touches still.
        /// </summary>
        public float TimeToPress = 1;

        /// <summary>
        /// Maximum distance in cm cluster can move before gesture fails.
        /// </summary>
        public float DistanceLimit = float.PositiveInfinity;

        #endregion

        #region Private fields

        private Vector2 totalMovement;
        private Timer timer = new Timer();
        private bool fireRecognizedNextUpdate = false;

        #endregion

        #region Public properties

        #endregion

        #region Unity

        protected override void Awake()
        {
            base.Awake();
            timer.Elapsed += onTimerElapsed;
            timer.AutoReset = false;
        }

        protected void Update()
        {
            if (fireRecognizedNextUpdate)
            {
                var target = Manager.GetHitTarget(Cluster.Get2DCenterPosition(ActiveTouches)); //assuming ActiveTouches.length > 0
                if (target == null || !(transform == target || target.IsChildOf(transform)))
                {
                    setState(GestureState.Failed);
                } else
                {
                    setState(GestureState.Recognized);
                }
            }
        }

        #endregion

        #region Gesture callbacks

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
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            totalMovement += Cluster.Get2DCenterPosition(touches) - Cluster.GetPrevious2DCenterPosition(touches);
            if (totalMovement.magnitude/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit) setState(GestureState.Failed);
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == 0)
            {
				timer.Stop();
                setState(GestureState.Failed);
            }
        }

        protected override void onFailed()
        {
            reset();
        }

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