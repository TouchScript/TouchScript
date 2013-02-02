/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : Gesture
    {
        #region Unity fields

        /// <summary>
        /// Maximum time to hold touches until gesture is considered to be failed.
        /// </summary>
        public float TimeLimit = float.PositiveInfinity;

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture is considered to be failed.
        /// </summary>
        public float DistanceLimit = float.PositiveInfinity;

        #endregion

        #region Private variables

        private float totalMovement = 0f;
        private float startTime;

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == touches.Count)
            {
                startTime = Time.time;
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            totalMovement += (Cluster.Get2DCenterPosition(touches) - Cluster.GetPrevious2DCenterPosition(touches)).magnitude;
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == 0)
            {
                if (totalMovement/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit || Time.time - startTime > TimeLimit)
                {
                    setState(GestureState.Failed);
                    return;
                }

                var target = Manager.GetHitTarget(touches[0].Position);
                if (target == null || !(transform == target || target.IsChildOf(transform)))
                {
                    setState(GestureState.Failed);
                } else
                {
                    setState(GestureState.Recognized);
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            setState(GestureState.Failed);
        }

        protected override void reset()
        {
            totalMovement = 0f;
        }

        #endregion
    }
}