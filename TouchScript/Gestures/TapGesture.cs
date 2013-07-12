/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using TouchScript.Gestures.Simple;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : SimpleTapGesture
    {

        [SerializeField]
        private float clusterExistenceTime = .3f;

        private List<TouchPoint> cluster = new List<TouchPoint>(); 
        private List<TouchPoint> removedPoints = new List<TouchPoint>();
        private List<float> removedPointsTimes = new List<float>();

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (TouchPoint.IsInvalidPosition(cachedScreenPosition))
                {
                    if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                    return Cluster.Get2DCenterPosition(activeTouches);
                }
                return cachedScreenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (TouchPoint.IsInvalidPosition(cachedScreenPosition))
                {
                    if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                    return Cluster.GetPrevious2DCenterPosition(activeTouches);
                }
                return cachedScreenPosition;
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            foreach (var touch in touches)
            {
                removedPoints.Add(touch);
                removedPointsTimes.Add(Time.time);
            }
            base.touchesEnded(touches);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();
            removedPoints.Clear();
            removedPointsTimes.Clear();
        }

        /// <inheritdoc />
        protected override void updateCachedScreenPosition(IList<TouchPoint> touches)
        {
            TouchPoint point;
            if (removedPoints.Count == 1)
            {
                point = removedPoints[0];
                cachedScreenPosition = point.Position;
                cachedPreviousScreenPosition = point.PreviousPosition;
                return;
            }

            cluster.Clear();
            cluster.Add(removedPoints[removedPoints.Count - 1]);
            var minTime = Time.time - clusterExistenceTime;
            for (var i = removedPoints.Count - 2; i >= 0; i--)
            {
                var time = removedPointsTimes[i];
                point = removedPoints[i];
                if (time > minTime)
                {
                    cluster.Add(point);
                }
                else
                {
                    break;
                }
            }

            cachedScreenPosition = Cluster.Get2DCenterPosition(cluster);
            cachedPreviousScreenPosition = Cluster.GetPrevious2DCenterPosition(cluster);
        }

    }
}