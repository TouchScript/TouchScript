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
        /// Gets or sets minimum distance in cm for touch points to move for gesture to begin. 
        /// </summary>
        /// <value>Minimum value in cm user must move their fingers to start this gesture.</value>
        public float MovementThreshold
        {
            get { return movementThreshold; }
            set { movementThreshold = value; }
        }

        /// <summary>
        /// Gets delta position in world coordinates.
        /// </summary>
        /// <value>Delta position between this frame and the last frame in world coordinates.</value>
        public Vector3 WorldDeltaPosition { get; private set; }

        /// <summary>
        /// Gets delta position in local coordinates.
        /// </summary>
        /// <value>Delta position between this frame and the last frame in local coordinates.</value>
        public Vector3 LocalDeltaPosition
        {
            get { return TransformUtils.GlobalToLocalDirection(transform, WorldDeltaPosition); }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchManager.INVALID_POSITION;
                if (activeTouches.Count == 1) return activeTouches[0].Position;
                return (activeTouches[0].Position + activeTouches[1].Position)*.5f;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchManager.INVALID_POSITION;
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
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            var worldDelta = Vector3.zero;
            Vector3 oldWorldCenter, newWorldCenter;

            Vector2 oldScreenCenter = PreviousScreenPosition;
            Vector2 newScreenCenter = ScreenPosition;

            if (isMoving)
            {
                oldWorldCenter = ProjectionUtils.CameraToPlaneProjection(oldScreenCenter, projectionCamera, WorldTransformPlane);
                newWorldCenter = ProjectionUtils.CameraToPlaneProjection(newScreenCenter, projectionCamera, WorldTransformPlane);
                worldDelta = newWorldCenter - oldWorldCenter;
            } else
            {
                movementBuffer += newScreenCenter - oldScreenCenter;
                var dpiMovementThreshold = MovementThreshold*touchManager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude > dpiMovementThreshold*dpiMovementThreshold)
                {
                    isMoving = true;
                    oldWorldCenter = ProjectionUtils.CameraToPlaneProjection(oldScreenCenter - movementBuffer, projectionCamera, WorldTransformPlane);
                    newWorldCenter = ProjectionUtils.CameraToPlaneProjection(newScreenCenter, projectionCamera, WorldTransformPlane);
                    worldDelta = newWorldCenter - oldWorldCenter;
                } else
                {
                    newWorldCenter = ProjectionUtils.CameraToPlaneProjection(newScreenCenter - movementBuffer, projectionCamera, WorldTransformPlane);
                    oldWorldCenter = newWorldCenter;
                }
            }

            if (worldDelta != Vector3.zero)
            {
                switch (State)
                {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        PreviousWorldTransformCenter = oldWorldCenter;
                        WorldTransformCenter = newWorldCenter;
                        WorldDeltaPosition = worldDelta;

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
            movementBuffer = Vector2.zero;
            isMoving = false;
        }

        #endregion
    }
}