/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Clusters;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes scaling gesture.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Scale Gesture")]
    public class ScaleGesture : TwoClusterTransform2DGestureBase
    {
        #region Private variables

        private float scalingBuffer;
        private bool isScaling = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Minimum distance in cm between clusters for gesture to be considered as a possible.
        /// </summary>
        [SerializeField]
        public float ScalingThreshold { get; set; }

        /// <summary>
        /// Contains local delta scale when gesture is recognized.
        /// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
        /// </summary>
        public float LocalDeltaScale { get; private set; }

        #endregion

        public ScaleGesture() : base()
        {
            ScalingThreshold = .5f;
        }

        #region Gesture callbacks

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (!clusters.HasClusters) return;

            Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;
            var deltaScale = 1f;

            var old2DPos1 = clusters.GetPreviousCenterPosition(Cluster2.CLUSTER1);
            var old2DPos2 = clusters.GetPreviousCenterPosition(Cluster2.CLUSTER2);
            var new2DPos1 = clusters.GetCenterPosition(Cluster2.CLUSTER1);
            var new2DPos2 = clusters.GetCenterPosition(Cluster2.CLUSTER2);
            var old3DPos1 = ProjectionUtils.CameraToPlaneProjection(old2DPos1, projectionCamera, WorldTransformPlane);
            var old3DPos2 = ProjectionUtils.CameraToPlaneProjection(old2DPos2, projectionCamera, WorldTransformPlane);
            var new3DPos1 = ProjectionUtils.CameraToPlaneProjection(new2DPos1, projectionCamera, WorldTransformPlane);
            var new3DPos2 = ProjectionUtils.CameraToPlaneProjection(new2DPos2, projectionCamera, WorldTransformPlane);
            var newVector = new3DPos2 - new3DPos1;

            Vector2 oldCenter2DPos = (old2DPos1 + old2DPos2)*.5f;
            Vector2 newCenter2DPos = (new2DPos1 + new2DPos2)*.5f;

            if (isScaling)
            {
                deltaScale = newVector.magnitude/Vector3.Distance(old3DPos2, old3DPos1);
            } else
            {
                var old2DDist = Vector2.Distance(old2DPos1, old2DPos2);
                var new2DDist = Vector2.Distance(new2DPos1, new2DPos2);
                var delta2DDist = new2DDist - old2DDist;
                scalingBuffer += delta2DDist;
                var dpiScalingThreshold = ScalingThreshold*Manager.DotsPerCentimeter;
                if (scalingBuffer*scalingBuffer >= dpiScalingThreshold*dpiScalingThreshold)
                {
                    isScaling = true;
                    var oldVector2D = (old2DPos2 - old2DPos1).normalized;
                    var startScale = (new2DDist - scalingBuffer)*.5f;
                    var startVector = oldVector2D*startScale;
                    deltaScale = newVector.magnitude/(ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos + startVector, projectionCamera, WorldTransformPlane) - ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos - startVector, projectionCamera, WorldTransformPlane)).magnitude;
                }
            }

            oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, WorldTransformPlane);
            newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, WorldTransformPlane);
            oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
            newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);

            if (Math.Abs(deltaScale - 1f) > 0.00001)
            {
                switch (State)
                {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        screenPosition = newCenter2DPos;
                        previousScreenPosition = oldCenter2DPos;
                        PreviousWorldTransformCenter = oldGlobalCenter3DPos;
                        WorldTransformCenter = newGlobalCenter3DPos;
                        PreviousWorldTransformCenter = oldGlobalCenter3DPos;
                        LocalTransformCenter = newLocalCenter3DPos;
                        PreviousLocalTransformCenter = oldLocalCenter3DPos;

                        LocalDeltaScale = deltaScale;

                        if (State == GestureState.Possible)
                        {
                            setState(GestureState.Began);
                        } else
                        {
                            setState(GestureState.Changed);
                        }
                        break;
                }
            }
        }

        protected override void reset()
        {
            base.reset();
            LocalDeltaScale = 1f;
            scalingBuffer = 0f;
            isScaling = false;
        }

        #endregion
    }
}