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
    /// Simple Pan gesture which only relies on the first touch.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Simple Pan Gesture")]
    public class SimplePanGesture : Transform2DGestureBase
    {
        #region Constants

        /// <summary>
        /// Message name when gesture starts
        /// </summary>
        public const string PAN_START_MESSAGE = "OnPanStart";

        /// <summary>
        /// Message name when gesture updates
        /// </summary>
        public const string PAN_MESSAGE = "OnPan";

        /// <summary>
        /// Message name when gesture ends
        /// </summary>
        public const string PAN_COMPLETE_MESSAGE = "OnPanComplete";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture starts.
        /// </summary>
        public event EventHandler<EventArgs> PanStarted
        {
            add { panStartedInvoker += value; }
            remove { panStartedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when gesture updates.
        /// </summary>
        public event EventHandler<EventArgs> Panned
        {
            add { pannedInvoker += value; }
            remove { pannedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when gesture ends.
        /// </summary>
        public event EventHandler<EventArgs> PanCompleted
        {
            add { panCompletedInvoker += value; }
            remove { panCompletedInvoker -= value; }
        }

        // iOS Events AOT hack
        private EventHandler<EventArgs> panStartedInvoker, pannedInvoker, panCompletedInvoker;

        #endregion

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
        /// Gets or sets the number of touches limited for the gesture to recognize and fail.
        /// </summary>
        /// <value>The number of touches limited for this gesture to recognize and fail. <c>0</c> — no limit, <c>1</c> — single touch, <c>2</c> — double touches.</value>
        public int NumberOfTouchesLimited
        {
            get { return numberOfTouchesLimited; }
            set
            {
                if (value < 0) numberOfTouchesLimited = 0;
                else numberOfTouchesLimited = value;
            }
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
            get { return TransformUtils.GlobalToLocalVector(cachedTransform, WorldDeltaPosition); }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (activeTouches.Count < 2) return base.ScreenPosition;
                return (activeTouches[0].Position + activeTouches[1].Position) * .5f;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (activeTouches.Count < 2) return base.PreviousScreenPosition;
                return (activeTouches[0].PreviousPosition + activeTouches[1].PreviousPosition) * .5f;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float movementThreshold = 0.5f;
        [SerializeField]
        private int numberOfTouchesLimited = 0;

        private Vector2 movementBuffer;
        private bool isMoving = false;

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);
            if (numberOfTouchesLimited > 0 && ActiveTouches.Count > numberOfTouchesLimited)
            {
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            if (numberOfTouchesLimited > 0 && ActiveTouches.Count < numberOfTouchesLimited) return;

            var worldDelta = Vector3.zero;
            Vector3 oldWorldCenter, newWorldCenter;

            Vector2 oldScreenCenter = PreviousScreenPosition;
            Vector2 newScreenCenter = ScreenPosition;

            if (isMoving)
            {
                oldWorldCenter = projectionLayer.ProjectTo(oldScreenCenter, WorldTransformPlane);
                newWorldCenter = projectionLayer.ProjectTo(newScreenCenter, WorldTransformPlane);
                worldDelta = newWorldCenter - oldWorldCenter;
            }
            else
            {
                movementBuffer += newScreenCenter - oldScreenCenter;
                var dpiMovementThreshold = MovementThreshold * touchManager.DotsPerCentimeter;
                if (movementBuffer.sqrMagnitude > dpiMovementThreshold * dpiMovementThreshold)
                {
                    isMoving = true;
                    oldWorldCenter = projectionLayer.ProjectTo(oldScreenCenter - movementBuffer, WorldTransformPlane);
                    newWorldCenter = projectionLayer.ProjectTo(newScreenCenter, WorldTransformPlane);
                    worldDelta = newWorldCenter - oldWorldCenter;
                }
                else
                {
                    newWorldCenter = projectionLayer.ProjectTo(newScreenCenter - movementBuffer, WorldTransformPlane);
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
                        }
                        else
                        {
                            setState(GestureState.Changed);
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void onBegan()
        {
            base.onBegan();
            if (panStartedInvoker != null) panStartedInvoker(this, EventArgs.Empty);
            if (pannedInvoker != null) pannedInvoker(this, EventArgs.Empty);
            if (UseSendMessage)
            {
                SendMessageTarget.SendMessage(PAN_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(PAN_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            if (pannedInvoker != null) pannedInvoker(this, EventArgs.Empty);
            if (UseSendMessage) SendMessageTarget.SendMessage(PAN_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (panCompletedInvoker != null) panCompletedInvoker(this, EventArgs.Empty);
            if (UseSendMessage) SendMessageTarget.SendMessage(PAN_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (PreviousState != GestureState.Possible)
            {
                if (panCompletedInvoker != null) panCompletedInvoker(this, EventArgs.Empty);
                if (UseSendMessage) SendMessageTarget.SendMessage(PAN_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (PreviousState != GestureState.Possible)
            {
                if (panCompletedInvoker != null) panCompletedInvoker(this, EventArgs.Empty);
                if (UseSendMessage) SendMessageTarget.SendMessage(PAN_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
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
