/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Clustered
{

    [AddComponentMenu("TouchScript/Gestures/Clustered/Transform Gesture (Clustered)")]
    public class ClusteredTransformGesture : TransformGesture
    {

        #region Private variables

        private Clusters.Clusters clusters = new Clusters.Clusters();

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

        #region Protected methods

        /// <inheritdoc />
        protected override int getNumPoints()
        {
            if (clusters.HasClusters) return 2;
            if (NumTouches > 0) return 1;
            return 0;
        }

        /// <inheritdoc />
        protected override bool relevantTouches1(IList<ITouch> touches)
        {
            return true;
        }

        /// <inheritdoc />
        protected override bool relevantTouches2(IList<ITouch> touches)
        {
            return true;
        }

        /// <inheritdoc />
        protected override Vector2 getPointScreenPosition(int index)
        {
            if (!clusters.HasClusters) return ClusterUtils.Get2DCenterPosition(activeTouches);

            return clusters.GetCenterPosition(index);
        }

        /// <inheritdoc />
        protected override Vector2 getPointPreviousScreenPosition(int index)
        {
            if (!clusters.HasClusters) return ClusterUtils.GetPrevious2DCenterPosition(activeTouches);

            return clusters.GetPreviousCenterPosition(index);
        }

        #endregion

    }
}
