/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections;
using System.Collections.Generic;
using TouchScript.Hit;
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
        private float recognizeTime;
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
        /// Maximum distance in cm touch points can move before gesture fails.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        protected void Update()
        {
            if (fireRecognizedNextUpdate)
            {
                if (base.GetTargetHitResult())
                {
                    setState(GestureState.Recognized);
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
            base.touchesBegan(touches);

            if (activeTouches.Count > MaxTouches)
            {
                setState(GestureState.Failed);
                return;
            }
            if (activeTouches.Count == touches.Count)
            {
                StartCoroutine("wait");
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            totalMovement += ScreenPosition - PreviousScreenPosition;
            if (totalMovement.magnitude/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit)
            {
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
            {
                StopCoroutine("wait");
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();

            reset();
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            fireRecognizedNextUpdate = false;
            StopCoroutine("wait");
        }

        #endregion

        #region Private functions

        private IEnumerator wait()
        {
            recognizeTime = Time.time + TimeToPress;
            while (Time.time < recognizeTime)
            {
                yield return null;
            }
            fireRecognizedNextUpdate = true;
        }

        #endregion
    }
}