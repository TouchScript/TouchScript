/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Simple Pan gesture which only relies on the first touch.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Simple Pan Gesture")]
    public class SimplePanGesture : Transform2DGestureBase
    {
        #region Public properties

        /// <summary>
        /// Minimum distance in cm for touch points to move for gesture to begin. 
        /// </summary>
        public float MovementThreshold
        {
            get { return movementThreshold; }
            set { movementThreshold = value; }
        }

        /// <summary>
        /// 3D delta position in global coordinates.
        /// </summary>
        public Vector3 WorldDeltaPosition { get; private set; }

        /// <summary>
        /// 3D delta position in local coordinates.
        /// </summary>
        public Vector3 LocalDeltaPosition { get; private set; }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                if (activeTouches.Count == 1) return activeTouches[0].Position;
                return (activeTouches[0].Position + activeTouches[1].Position)*.5f;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                if (activeTouches.Count == 1) return activeTouches[0].PreviousPosition;
                return (activeTouches[0].PreviousPosition + activeTouches[1].PreviousPosition)*.5f;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float movementThreshold = 0.5f;

        private Vector2 movementBuffer;
        private bool isMoving = false;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
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
                var dpiMovementThreshold = MovementThreshold*touchManager.DotsPerCentimeter;
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
                        WorldDeltaPosition = globalDelta3DPos;
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

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            WorldDeltaPosition = Vector3.zero;
            LocalDeltaPosition = Vector3.zero;
            movementBuffer = Vector2.zero;
            isMoving = false;
        }

        #endregion
    }
}