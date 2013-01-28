/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;
using TouchScript.Clusters;

namespace TouchScript.Gestures {
    /// <summary>
    /// Recognizes cluster movement.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Pan Gesture")]
    public class PanGesture : Transform2DGestureBase {
        #region Private variables

        private Vector2 movementBuffer;
        private bool isMoving = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Minimum distance in cm for cluster to move to be considered as a possible gesture. 
        /// </summary>
        [SerializeField]
        public float MovementThreshold { get; set; }

        /// <summary>
        /// 3D delta position in global coordinates.
        /// </summary>
        public Vector3 GlobalDeltaPosition { get; private set; }

        /// <summary>
        /// 3D delta position in local coordinates.
        /// </summary>
        public Vector3 LocalDeltaPosition { get; private set; }

        #endregion

        public PanGesture() : base() {
            MovementThreshold = .5f;
        }

        #region Gesture callbacks

        protected override void touchesMoved(IList<TouchPoint> touches) {
            base.touchesMoved(touches);

            var globalDelta3DPos = Vector3.zero;
            var localDelta3DPos = Vector3.zero;
            Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;

            Vector2 oldCenter2DPos = Cluster.GetPrevious2DCenterPosition(activeTouches);
            Vector2 newCenter2DPos = Cluster.Get2DCenterPosition(activeTouches);

            updateProjectionPlane(Cluster.GetClusterCamera(activeTouches));

            if (isMoving) {
                oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, GlobalTransformPlane);
                newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, GlobalTransformPlane);
                globalDelta3DPos = newGlobalCenter3DPos - oldGlobalCenter3DPos;
                oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
                newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                localDelta3DPos = newLocalCenter3DPos - globalToLocalPosition(oldGlobalCenter3DPos);
            } else {
                movementBuffer += newCenter2DPos - oldCenter2DPos;
                var dpiMovementThreshold = MovementThreshold*Manager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude > dpiMovementThreshold*dpiMovementThreshold) {
                    isMoving = true;
                    oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos - movementBuffer, projectionCamera, GlobalTransformPlane);
                    newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, GlobalTransformPlane);
                    globalDelta3DPos = newGlobalCenter3DPos - oldGlobalCenter3DPos;
                    oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
                    newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                    localDelta3DPos = newLocalCenter3DPos - globalToLocalPosition(oldGlobalCenter3DPos);
                } else {
                    newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos - movementBuffer, projectionCamera, GlobalTransformPlane);
                    newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                    oldGlobalCenter3DPos = newGlobalCenter3DPos;
                    oldLocalCenter3DPos = newLocalCenter3DPos;
                }
            }

            if (globalDelta3DPos != Vector3.zero) {
                switch (State) {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        ScreenTransformCenter = newCenter2DPos;
                        PreviousGlobalTransformCenter = oldGlobalCenter3DPos;
                        GlobalTransformCenter = newGlobalCenter3DPos;
                        GlobalDeltaPosition = globalDelta3DPos;
                        PreviousGlobalTransformCenter = oldGlobalCenter3DPos;
                        LocalTransformCenter = newLocalCenter3DPos;
                        LocalDeltaPosition = localDelta3DPos;
                        PreviousLocalTransformCenter = oldLocalCenter3DPos;

                        if (State == GestureState.Possible) {
                            setState(GestureState.Began);
                        } else {
                            setState(GestureState.Changed);
                        }
                        break;
                }
            }
        }

        protected override void reset() {
            base.reset();
            resetMovement();
        }

        #endregion

        #region Private functions

        private void resetMovement() {
            movementBuffer = Vector2.zero;
            isMoving = false;
        }

        protected override void resetGestureProperties() {
            base.resetGestureProperties();
            GlobalDeltaPosition = Vector3.zero;
            LocalDeltaPosition = Vector3.zero;
        }

        #endregion
    }
}