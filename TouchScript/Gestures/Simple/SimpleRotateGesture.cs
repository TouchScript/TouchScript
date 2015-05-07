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
        #region Constants

        /// <summary>
        /// Message name when gesture starts
        /// </summary>
        public const string ROTATE_START_MESSAGE = "OnRotateStart";

        /// <summary>
        /// Message name when gesture updates
        /// </summary>
        public const string ROTATE_MESSAGE = "OnRotate";

        /// <summary>
        /// Message name when gesture ends
        /// </summary>
        public const string ROTATE_COMPLETE_MESSAGE = "OnRotateComplete";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture starts.
        /// </summary>
        public event EventHandler<EventArgs> RotateStarted
        {
            add { rotateStartedInvoker += value; }
            remove { rotateStartedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when gesture updates.
        /// </summary>
        public event EventHandler<EventArgs> Rotated
        {
            add { rotatedInvoker += value; }
            remove { rotatedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when gesture ends.
        /// </summary>
        public event EventHandler<EventArgs> RotateCompleted
        {
            add { rotateCompletedInvoker += value; }
            remove { rotateCompletedInvoker -= value; }
        }

        // iOS Events AOT hack
        private EventHandler<EventArgs> rotateStartedInvoker, rotatedInvoker, rotateCompletedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets minimum rotation in degrees for gesture to begin.
        /// </summary>
        /// <value>Minimum degrees to turn for gesture to begin.</value>
        public float RotationThreshold
        {
            get { return rotationThreshold; }
            set { rotationThreshold = value; }
        }

        /// <summary>
        /// Gets delta rotation in degrees around <see cref="RotationAxis"/>. Changes every Begin or Changed state.
        /// </summary>
        /// <value>Delta rotation around <see cref="RotationAxis"/> since the last frame in degrees.</value>
        public float DeltaRotation { get; private set; }

        /// <summary>
        /// Gets rotation axis of the gesture in world coordinates.
        /// </summary>
        /// <value>Rotation axis of the gesture in world coordinates.</value>
        public Vector3 RotationAxis
        {
            get { return worldTransformPlane.normal; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float rotationThreshold = 3f;

        private float rotationBuffer;
        private bool isRotating = false;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            if (!gotEnoughTouches()) return;
            if (!relevantTouches(touches)) return;

            Vector3 oldWorldCenter, newWorldCenter;
            var deltaRotation = 0f;

            var newScreenPos1 = getPointScreenPosition(0);
            var newScreenPos2 = getPointScreenPosition(1);
            var newScreenDelta = newScreenPos2 - newScreenPos1;
            if (newScreenDelta.sqrMagnitude < minPixelDistanceSquared) return;

            base.touchesMoved(touches);

            var oldScreenPos1 = getPointPreviousScreenPosition(0);
            var oldScreenPos2 = getPointPreviousScreenPosition(1);
            var oldWorldPos1 = projectionLayer.ProjectTo(oldScreenPos1, WorldTransformPlane);
            var oldWorldPos2 = projectionLayer.ProjectTo(oldScreenPos2, WorldTransformPlane);
            var newWorldPos1 = projectionLayer.ProjectTo(newScreenPos1, WorldTransformPlane);
            var newWorldPos2 = projectionLayer.ProjectTo(newScreenPos2, WorldTransformPlane);
            var newVector = newWorldPos2 - newWorldPos1;
            var oldVector = oldWorldPos2 - oldWorldPos1;

            Vector2 oldScreenCenter = (oldScreenPos1 + oldScreenPos2) * .5f;
            Vector2 newScreenCenter = (newScreenPos1 + newScreenPos2) * .5f;

            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), WorldTransformPlane.normal) < 0) angle = -angle;
            if (isRotating)
            {
                deltaRotation = angle;
            }
            else
            {
                rotationBuffer += angle;
                if (rotationBuffer * rotationBuffer >= RotationThreshold * RotationThreshold)
                {
                    isRotating = true;
                    deltaRotation = rotationBuffer;
                }
            }

            oldWorldCenter = projectionLayer.ProjectTo(oldScreenCenter, WorldTransformPlane);
            newWorldCenter = projectionLayer.ProjectTo(newScreenCenter, WorldTransformPlane);

            if (Math.Abs(deltaRotation) > 0.00001)
            {
                switch (State)
                {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
                        screenPosition = newScreenCenter;
                        previousScreenPosition = oldScreenCenter;
                        PreviousWorldTransformCenter = oldWorldCenter;
                        WorldTransformCenter = newWorldCenter;

                        DeltaRotation = deltaRotation;

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
        protected override void reset()
        {
            base.reset();

            rotationBuffer = 0f;
            isRotating = false;
        }

        /// <inheritdoc />
        protected override void onBegan()
        {
            base.onBegan();
            rotateStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            rotatedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
            {
                SendMessageTarget.SendMessage(ROTATE_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(ROTATE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            rotatedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(ROTATE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            rotateCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(ROTATE_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (PreviousState != GestureState.Possible)
            {
                rotateCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(ROTATE_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (PreviousState != GestureState.Possible)
            {
                rotateCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(ROTATE_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void restart()
        {
            base.restart();

            DeltaRotation = 0f;
        }

        #endregion
    }
}
