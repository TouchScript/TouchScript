/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    public abstract class TwoClusterTransform2DGestureBase : Transform2DGestureBase
    {
        protected Cluster2 clusters = new Cluster2();
        protected Vector2 screenPosition;
        protected Vector3 previousScreenPosition;

        /// <summary>
        /// Minimum distance between clusters in cm for gesture to be recognized.
        /// </summary>
        [SerializeField]
        public float MinClusterDistance { get; set; }

        public override Vector2 ScreenPosition
        {
            get { return screenPosition; }
        }

        public override Vector2 PreviousScreenPosition
        {
            get { return previousScreenPosition; }
        }

        public TwoClusterTransform2DGestureBase() : base()
        {
            MinClusterDistance = .5f;
        }

        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);
            clusters.AddPoints(touches);
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            clusters.Invalidate();
            clusters.MinPointsDistance = MinClusterDistance*TouchManager.Instance.DotsPerCentimeter;
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            clusters.RemovePoints(touches);
            if ((State == GestureState.Began || State == GestureState.Changed) && !clusters.HasClusters)
            {
                setState(GestureState.Ended);
            }
        }

        protected override void reset()
        {
            base.reset();
            clusters.RemoveAllPoints();
        }
    }
}