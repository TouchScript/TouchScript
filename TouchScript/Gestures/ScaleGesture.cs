/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Gestures.Simple;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes scaling gesture.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Scale Gesture")]
    public class ScaleGesture : SimpleScaleGesture
    {
        #region Public properties

        /// <inheritdoc />
        public override float MinPointsDistance
        {
            get { return base.MinPointsDistance; }
            set
            {
                base.MinPointsDistance = value;
                if (Application.isPlaying)
                {
                    clusters.MinPointsDistance = minPixelDistance;
                }
            }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// Cluster object
        /// </summary>
        protected Clusters.Clusters clusters = new Clusters.Clusters();

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            clusters.MinPointsDistance = minPixelDistance;
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            clusters.AddPoints(touches);

            base.touchesBegan(touches);
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            clusters.Invalidate();

            base.touchesMoved(touches);
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouch> touches)
        {
            clusters.RemovePoints(touches);

            base.touchesEnded(touches);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            clusters.RemoveAllPoints();
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override bool gotEnoughTouches()
        {
            return clusters.HasClusters;
        }

        /// <inheritdoc />
        protected override bool relevantTouches(IList<ITouch> touches)
        {
            // every touch point is relevant for us
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

        #endregion
    }
}
