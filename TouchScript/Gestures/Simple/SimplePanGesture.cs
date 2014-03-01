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

        #region Constants

        public const string PAN_STARTED_MESSAGE = "OnPanStarted";
        public const string PANNED_MESSAGE = "OnPanned";
        public const string PAN_STOPPED_MESSAGE = "OnPanStopped";

        #endregion

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
        protected override void onBegan()        {
            base.onBegan();
            if (UseSendMessage)
            {
                SendMessageTarget.SendMessage(PAN_STARTED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(PANNED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            if (UseSendMessage) SendMessageTarget.SendMessage(PANNED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (UseSendMessage) SendMessageTarget.SendMessage(PAN_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (UseSendMessage && PreviousState != GestureState.Possible) SendMessageTarget.SendMessage(PAN_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (UseSendMessage && PreviousState != GestureState.Possible) SendMessageTarget.SendMessage(PAN_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
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