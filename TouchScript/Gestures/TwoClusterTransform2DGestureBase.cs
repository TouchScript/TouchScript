/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Base class for transform gestures involving two clusters.
    /// </summary>
    public abstract class TwoClusterTransform2DGestureBase : Transform2DGestureBase
    {
        #region Private variables

        [SerializeField]
        private float minClusterDistance = .5f;

        /// <summary>
        /// Clusters object
        /// </summary>
        protected Clusters2 clusters = new Clusters2();
        /// <summary>
        /// Transform's center point screen position.
        /// </summary>
        protected Vector2 screenPosition;
        /// <summary>
        /// Transform's center point previous screen position.
        /// </summary>
        protected Vector3 previousScreenPosition;

        #endregion

        /// <summary>
        /// Minimum distance between clusters in cm for gesture to be recognized.
        /// </summary>
        public float MinClusterDistance
        {
            get { return minClusterDistance; }
            set { minClusterDistance = value; }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get { return screenPosition; }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get { return previousScreenPosition; }
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
            clusters.MinPointsDistance = MinClusterDistance*TouchManager.Instance.DotsPerCentimeter;
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
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
    }
}