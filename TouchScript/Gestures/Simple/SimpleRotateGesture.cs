/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Simple Rotate gesture which takes into account only the first two touch points.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Simple Rotate Gesture")]
    public class SimpleRotateGesture : TwoPointTransform2DGestureBase
    {
        #region Private variables

        [SerializeField]
        private float rotationThreshold = 3f;

        private float rotationBuffer;
        private bool isRotating = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Minimum rotation in degrees for gesture to begin.
        /// </summary>
        public float RotationThreshold
        {
            get { return rotationThreshold; }
            set { rotationThreshold = value; }
        }

        /// <summary>
        /// Local delta rotation in degrees. Changes every Begin or Changed state.
        /// </summary>
        public float LocalDeltaRotation { get; private set; }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            if (!gotEnoughTouchPoints()) return;
            if (!relevantTouchPoints(touches)) return;

            Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;
            var deltaRotation = 0f;
            
            var new2DPos1 = getPointScreenPosition(0);
            var new2DPos2 = getPointScreenPosition(1);
            if (Vector2.Distance(new2DPos1, new2DPos2) < minPointsDistanceInPixels) return;

            base.touchesMoved(touches);

            var old2DPos1 = getPointPreviousScreenPosition(0);
            var old2DPos2 = getPointPreviousScreenPosition(1);
            var old3DPos1 = ProjectionUtils.CameraToPlaneProjection(old2DPos1, projectionCamera, WorldTransformPlane);
            var old3DPos2 = ProjectionUtils.CameraToPlaneProjection(old2DPos2, projectionCamera, WorldTransformPlane);
            var new3DPos1 = ProjectionUtils.CameraToPlaneProjection(new2DPos1, projectionCamera, WorldTransformPlane);
            var new3DPos2 = ProjectionUtils.CameraToPlaneProjection(new2DPos2, projectionCamera, WorldTransformPlane);
            var newVector = new3DPos2 - new3DPos1;
            var oldVector = old3DPos2 - old3DPos1;

            Vector2 oldCenter2DPos = (old2DPos1 + old2DPos2)*.5f;
            Vector2 newCenter2DPos = (new2DPos1 + new2DPos2)*.5f;

            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), WorldTransformPlane.normal) < 0) angle = -angle;
            if (isRotating)
            {
                deltaRotation = angle;
            } else
            {
                rotationBuffer += angle;
                if (rotationBuffer*rotationBuffer >= RotationThreshold*RotationThreshold)
                {
                    isRotating = true;
                    deltaRotation = rotationBuffer;
                }
            }

            oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, WorldTransformPlane);
            newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, WorldTransformPlane);
            oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
            newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);

            if (Math.Abs(deltaRotation) > 0.00001)
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

                        LocalDeltaRotation = deltaRotation;

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

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            rotationBuffer = 0f;
            isRotating = false;
        }

        /// <inheritdoc />
        protected override void restart()
        {
            base.restart();

            LocalDeltaRotation = 0f;
        }

        #endregion
    }
}