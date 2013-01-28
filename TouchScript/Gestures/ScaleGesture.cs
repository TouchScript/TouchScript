/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using TouchScript.Clusters;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures {
    /// <summary>
    /// Recognizes scaling gesture.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Scale Gesture")]
    public class ScaleGesture : Transform2DGestureBase {
        #region Private variables

        private Cluster2 cluster2 = new Cluster2();
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
        /// Minimum distance between clusters in cm for gesture to be recognized.
        /// </summary>
        [SerializeField]
        public float MinClusterDistance { get; set; }

        /// <summary>
        /// Contains local delta scale when gesture is recognized.
        /// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
        /// </summary>
        public float LocalDeltaScale { get; private set; }

        #endregion

        public ScaleGesture() : base() {
            ScalingThreshold = .5f;
            MinClusterDistance = .5f;
        }

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches) {
            base.touchesBegan(touches);
            foreach (var touch in touches) {
                cluster2.AddPoint(touch);
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches) {
            base.touchesMoved(touches);

            cluster2.Invalidate();
            cluster2.MinPointsDistance = MinClusterDistance*TouchManager.Instance.DotsPerCentimeter;
            if (!cluster2.HasClusters) return;

            Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;
            var deltaScale = 1f;

            updateProjectionPlane(Cluster.GetClusterCamera(activeTouches));

            var old2DPos1 = cluster2.GetPreviousCenterPosition(Cluster2.CLUSTER1);
            var old2DPos2 = cluster2.GetPreviousCenterPosition(Cluster2.CLUSTER2);
            var new2DPos1 = cluster2.GetCenterPosition(Cluster2.CLUSTER1);
            var new2DPos2 = cluster2.GetCenterPosition(Cluster2.CLUSTER2);
            var old3DPos1 = ProjectionUtils.CameraToPlaneProjection(old2DPos1, projectionCamera, GlobalTransformPlane);
            var old3DPos2 = ProjectionUtils.CameraToPlaneProjection(old2DPos2, projectionCamera, GlobalTransformPlane);
            var new3DPos1 = ProjectionUtils.CameraToPlaneProjection(new2DPos1, projectionCamera, GlobalTransformPlane);
            var new3DPos2 = ProjectionUtils.CameraToPlaneProjection(new2DPos2, projectionCamera, GlobalTransformPlane);
            var newVector = new3DPos2 - new3DPos1;

            Vector2 oldCenter2DPos = (old2DPos1 + old2DPos2) * .5f;
            Vector2 newCenter2DPos = (new2DPos1 + new2DPos2) * .5f;

            if (isScaling) {
                deltaScale = newVector.magnitude/Vector3.Distance(old3DPos2, old3DPos1);
            } else {
                var old2DDist = Vector2.Distance(old2DPos1, old2DPos2);
                var new2DDist = Vector2.Distance(new2DPos1, new2DPos2);
                var delta2DDist = new2DDist - old2DDist;
                scalingBuffer += delta2DDist;
                var dpiScalingThreshold = ScalingThreshold*Manager.DotsPerCentimeter;
                if (scalingBuffer*scalingBuffer >= dpiScalingThreshold*dpiScalingThreshold) {
                    isScaling = true;
                    var oldVector2D = (old2DPos2 - old2DPos1).normalized;
                    var startScale = (new2DDist - scalingBuffer)*.5f;
                    var startVector = oldVector2D*startScale;
                    deltaScale = newVector.magnitude / (ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos + startVector, projectionCamera, GlobalTransformPlane) - ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos - startVector, projectionCamera, GlobalTransformPlane)).magnitude;
                }
            }

            oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, GlobalTransformPlane);
            newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, GlobalTransformPlane);
            oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
            newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);

            if (Math.Abs(deltaScale - 1f) > 0.00001) {
                switch (State) {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        ScreenTransformCenter = newCenter2DPos;
                        PreviousGlobalTransformCenter = oldGlobalCenter3DPos;
                        GlobalTransformCenter = newGlobalCenter3DPos;
                        PreviousGlobalTransformCenter = oldGlobalCenter3DPos;
                        LocalTransformCenter = newLocalCenter3DPos;
                        PreviousLocalTransformCenter = oldLocalCenter3DPos;

                        LocalDeltaScale = deltaScale;

                        if (State == GestureState.Possible) {
                            setState(GestureState.Began);
                        } else {
                            setState(GestureState.Changed);
                        }
                        break;
                }
            }
        }

        protected override void touchesEnded(IList<TouchPoint> touches) {
            foreach (var touch in touches) {
                cluster2.RemovePoint(touch);
            }
            if (!cluster2.HasClusters) {
                resetScaling();
            }
            base.touchesEnded(touches);
        }

        protected override void reset() {
            base.reset();
            cluster2.RemoveAllPoints();
            resetScaling();
        }

        protected override void resetGestureProperties() {
            base.resetGestureProperties();
            LocalDeltaScale = 1f;
        }

        #endregion

        #region Private functions

        private void resetScaling() {
            scalingBuffer = 0f;
            isScaling = false;
        }

        #endregion
    }
}