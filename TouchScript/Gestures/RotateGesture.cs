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
    /// Recognizes rotation gesture.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Rotate Gesture")]
    public class RotateGesture : SimpleRotateGesture
    {

        /// <summary>
        /// Cluster object
        /// </summary>
        protected Clusters2 clusters = new Clusters2();

        /// <inheritdoc />
        public override float MinPointDistance
        {
            get { return base.MinPointDistance; }
            set
            {
                base.MinPointDistance = value;
                if (Application.isPlaying)
                {
                    clusters.MinPointsDistance = value * TouchManager.Instance.DotsPerCentimeter;
                }
            }
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            clusters.MinPointsDistance = MinPointDistance * TouchManager.Instance.DotsPerCentimeter;
        }

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            clusters.AddPoints(touches);
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            clusters.Invalidate();
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            clusters.RemovePoints(touches);
            if ((State == GestureState.Began || State == GestureState.Changed) && !clusters.HasClusters)
            {
                setState(GestureState.Ended);
            }
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            clusters.RemoveAllPoints();
        }

        /// <inheritdoc />
        protected override bool gotEnoughTouchPoints()
        {
            return clusters.HasClusters;
        }

        protected override bool relevantTouchPoints(IList<TouchPoint> touches)
        {
            return true;
        }

        /// <inheritdoc />
        protected override Vector2 getPointScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return clusters.GetCenterPosition(index);
        }

        /// <inheritdoc />
        protected override Vector2 getPointPreviousScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return clusters.GetPreviousCenterPosition(index);
        }

    }
}