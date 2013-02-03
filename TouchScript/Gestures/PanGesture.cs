/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;
using TouchScript.Clusters;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes cluster movement.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Pan Gesture")]
    public class PanGesture : Transform2DGestureBase
    {
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

        public PanGesture() : base()
        {
            MovementThreshold = .5f;
        }

        #region Gesture callbacks

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            var globalDelta3DPos = Vector3.zero;
            var localDelta3DPos = Vector3.zero;
            Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;

            Vector2 oldCenter2DPos = PreviousScreenPosition;
            Vector2 newCenter2DPos = ScreenPosition;

            if (isMoving)
            {
                oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, WorldTransformPlane);
                newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, WorldTransformPlane);
                globalDelta3DPos = newGlobalCenter3DPos - oldGlobalCenter3DPos;
                oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
                newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                localDelta3DPos = newLocalCenter3DPos - globalToLocalPosition(oldGlobalCenter3DPos);
            } else
            {
                movementBuffer += newCenter2DPos - oldCenter2DPos;
                var dpiMovementThreshold = MovementThreshold*Manager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude > dpiMovementThreshold*dpiMovementThreshold)
                {
                    isMoving = true;
                    oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos - movementBuffer, projectionCamera, WorldTransformPlane);
                    newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, WorldTransformPlane);
                    globalDelta3DPos = newGlobalCenter3DPos - oldGlobalCenter3DPos;
                    oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
                    newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                    localDelta3DPos = newLocalCenter3DPos - globalToLocalPosition(oldGlobalCenter3DPos);
                } else
                {
                    newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos - movementBuffer, projectionCamera, WorldTransformPlane);
                    newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);
                    oldGlobalCenter3DPos = newGlobalCenter3DPos;
                    oldLocalCenter3DPos = newLocalCenter3DPos;
                }
            }

            if (globalDelta3DPos != Vector3.zero)
            {
                switch (State)
                {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        PreviousWorldTransformCenter = oldGlobalCenter3DPos;
                        WorldTransformCenter = newGlobalCenter3DPos;
                        GlobalDeltaPosition = globalDelta3DPos;
                        PreviousWorldTransformCenter = oldGlobalCenter3DPos;
                        LocalTransformCenter = newLocalCenter3DPos;
                        LocalDeltaPosition = localDelta3DPos;
                        PreviousLocalTransformCenter = oldLocalCenter3DPos;

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
            GlobalDeltaPosition = Vector3.zero;
            LocalDeltaPosition = Vector3.zero;
            movementBuffer = Vector2.zero;
            isMoving = false;
        }

        #endregion
    }
}