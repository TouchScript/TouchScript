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
using UnityEngine;

namespace TouchScript.Gestures {
    /// <summary>
    /// Recognizes rotation gesture.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Rotate Gesture")]
    public class RotateGesture : Transform2DGestureBase {
        #region Unity fields

        /// <summary>
        /// Minimum rotation in degrees to be considered as a possible gesture.
        /// </summary>
        public float RotationThreshold = 3;

        /// <summary>
        /// Minimum distance between clusters in cm for gesture to be recognized.
        /// </summary>
        public float MinClusterDistance = .5f;

        #endregion

        #region Private variables

        private Cluster2 cluster2 = new Cluster2();
        private float rotationBuffer;
        private bool isRotating = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Contains local rotation when gesture is recognized.
        /// </summary>
        public float LocalDeltaRotation { get; private set; }

        #endregion

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
            if (Camera.mainCamera == null) {
                print("Camera.mainCamera is not set!");
                return;
            }

            if (!cluster2.HasClusters) return;

            Vector3 oldGlobalCenter3DPos;
            Vector3 oldLocalCenter3DPos;
            Vector3 newGlobalCenter3DPos;
            Vector3 newLocalCenter3DPos;
            var deltaRotation = 0f;

            Vector3 globalPlaneNormal;
            if (NormalVector == Vector3.zero) {
                globalPlaneNormal = Camera.mainCamera.transform.forward;
            } else if (IsLocal) {
                globalPlaneNormal = transform.localToWorldMatrix.MultiplyVector(NormalVector).normalized;
            } else {
                globalPlaneNormal = NormalVector.normalized;
            }
            var globalPlane = new Plane(globalPlaneNormal, transform.position);

            Vector2 oldCenter2DPos;
            Vector2 newCenter2DPos;

            var old2DPos1 = cluster2.GetPreviousCenterPosition(Cluster2.CLUSTER1);
            var old2DPos2 = cluster2.GetPreviousCenterPosition(Cluster2.CLUSTER2);
            var new2DPos1 = cluster2.GetCenterPosition(Cluster2.CLUSTER1);
            var new2DPos2 = cluster2.GetCenterPosition(Cluster2.CLUSTER2);
            var old3DPos1 = get3DPosition(globalPlane, cluster2.Camera, old2DPos1);
            var old3DPos2 = get3DPosition(globalPlane, cluster2.Camera, old2DPos2);
            var new3DPos1 = get3DPosition(globalPlane, cluster2.Camera, new2DPos1);
            var new3DPos2 = get3DPosition(globalPlane, cluster2.Camera, new2DPos2);
            var newVector = new3DPos2 - new3DPos1;
            var oldVector = old3DPos2 - old3DPos1;

            oldCenter2DPos = (old2DPos1 + old2DPos2)*.5f;
            newCenter2DPos = (new2DPos1 + new2DPos2)*.5f;

            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), globalPlaneNormal) < 0) angle = -angle;
            if (isRotating) {
                deltaRotation = angle;
            } else {
                rotationBuffer += angle;
                if (rotationBuffer*rotationBuffer >= RotationThreshold*RotationThreshold) {
                    isRotating = true;
                    deltaRotation = rotationBuffer;
                }
            }

            oldGlobalCenter3DPos = get3DPosition(globalPlane, cluster2.Camera, oldCenter2DPos);
            newGlobalCenter3DPos = get3DPosition(globalPlane, cluster2.Camera, newCenter2DPos);
            oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
            newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);

            if (Math.Abs(deltaRotation) > 0.00001) {
                switch (State) {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        ScreenTransformCenter = newCenter2DPos;
                        PreviousGlobalTransformCenter = oldGlobalCenter3DPos;
                        GlobalTransformCenter = newGlobalCenter3DPos;
                        GlobalTransformPlane = globalPlane;
                        PreviousGlobalTransformCenter = oldGlobalCenter3DPos;
                        LocalTransformCenter = newLocalCenter3DPos;
                        PreviousLocalTransformCenter = oldLocalCenter3DPos;

                        LocalDeltaRotation = deltaRotation;

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
                resetRotation();
            }
            base.touchesEnded(touches);
        }

        protected override void reset() {
            base.reset();
            cluster2.RemoveAllPoints();
            resetRotation();
        }

        #endregion

        #region Private functions

        private void resetRotation() {
            rotationBuffer = 0f;
            isRotating = false;
        }

        protected override void resetGestureProperties() {
            base.resetGestureProperties();
            LocalDeltaRotation = 0f;
        }

        #endregion
    }
}