/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Clustered
{
    /// <summary>
    /// ScreenTransformGesture which splits all touch points into 2 clusters and works with them.
    /// Should be used for large touch surfaces.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Clustered/Screen Transform Gesture (Clustered)")]
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Gestures_Clustered_ClusteredScreenTransformGesture.htm")]
    public class ClusteredScreenTransformGesture : ScreenTransformGesture
    {
        #region Private variables

        private Clusters.Clusters clusters = new Clusters.Clusters();

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchBegan(TouchPoint touch)
        {
            clusters.AddPoint(touch);

            base.touchBegan(touch);
        }

        /// <inheritdoc />
        protected override void touchMoved(TouchPoint touch)
        {
            clusters.Invalidate();

            base.touchMoved(touch);
        }

        /// <inheritdoc />
        protected override void touchEnded(TouchPoint touch)
        {
            clusters.RemovePoint(touch);

            base.touchEnded(touch);
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
        protected override bool relevantTouches1()
        {
            return true;
        }

        /// <inheritdoc />
        protected override bool relevantTouches2()
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