/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : ClusterBasedGesture
    {
        #region Private variables

        [SerializeField]
        private float timeLimit = float.PositiveInfinity;

        [SerializeField]
        private float distanceLimit = float.PositiveInfinity;

        [SerializeField]
        private float clusterExistenceTime = .3f;

        private List<TouchPoint> removedPoints = new List<TouchPoint>();
        private List<float> removedPointsTimes = new List<float>();
        private Vector2 cachedScreenPosition, cachedPreviousScreenPosition;
        private RaycastHit cachedCentroidHitResult;

        private float totalMovement = 0f;
        private float startTime;

        #endregion

        #region Public properties

        /// <summary>
        /// Maximum time to hold touches until gesture is considered to be failed.
        /// </summary>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture is considered to be failed.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (cachedScreenPosition == TouchPoint.InvalidPosition)  return base.ScreenPosition;
                return cachedScreenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (cachedScreenPosition == TouchPoint.InvalidPosition)  return base.PreviousScreenPosition;
                return cachedPreviousScreenPosition;
            }
        }

        #endregion

        /// <inheritdoc />
        public override bool GetTargetHitResult(out RaycastHit hit)
        {
            if (State == GestureState.Ended)
            {
                hit = cachedCentroidHitResult;
                return true;
            }

            return base.GetTargetHitResult(out hit);
        }

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == touches.Count)
            {
                startTime = Time.time;
                setState(GestureState.Began);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            totalMovement += (Cluster.Get2DCenterPosition(touches) - Cluster.GetPrevious2DCenterPosition(touches)).magnitude;
            setState(GestureState.Changed);
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            foreach (var touch in touches)
            {
                removedPoints.Add(touch);
                removedPointsTimes.Add(Time.time);
            }

            if (ActiveTouches.Count == 0)
            {
                if (totalMovement/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit || Time.time - startTime > TimeLimit)
                {
                    setState(GestureState.Failed);
                    return;
                }

                updateCachedScreenPosition();

                if (base.GetTargetHitResult(out cachedCentroidHitResult))
                {
                    setState(GestureState.Ended);
                } else
                {
                    setState(GestureState.Failed);
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            setState(GestureState.Failed);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            totalMovement = 0f;
            removedPoints.Clear();
            removedPointsTimes.Clear();
            cachedScreenPosition = TouchPoint.InvalidPosition;
            cachedPreviousScreenPosition = TouchPoint.InvalidPosition;
            //cachedCentroidHitResult = new RaycastHit();
        }

        #endregion

        #region Private functions

        private void updateCachedScreenPosition()
        {
            TouchPoint point;
            if (removedPoints.Count == 1)
            {
                point = removedPoints[0];
                cachedScreenPosition = point.Position;
                cachedPreviousScreenPosition = point.PreviousPosition;
                return;
            }

            point = removedPoints[removedPoints.Count - 1];
            var position = point.Position;
            var previousPosition = point.PreviousPosition;
            var minTime = Time.time - clusterExistenceTime;
            var i = 1;
            for (; i <= removedPoints.Count - 1; i++)
            {
                var index = removedPoints.Count - 1 - i;
                var time = removedPointsTimes[index];
                point = removedPoints[index];
                if (time > minTime)
                {
                    position += point.Position;
                    previousPosition += point.PreviousPosition;
                }
                else
                {
                    break;
                }
            }

            cachedScreenPosition = position/i;
            cachedPreviousScreenPosition = previousPosition/i;
        }

        #endregion

    }
}