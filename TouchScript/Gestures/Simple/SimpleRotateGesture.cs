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

        public const string ROTATE_STARTED_MESSAGE = "OnRotateStarted";
        public const string ROTATED_MESSAGE = "OnRotated";
        public const string ROTATE_STOPPED_MESSAGE = "OnRotateStopped";

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
            var oldWorldPos1 = ProjectionUtils.CameraToPlaneProjection(oldScreenPos1, projectionCamera, WorldTransformPlane);
            var oldWorldPos2 = ProjectionUtils.CameraToPlaneProjection(oldScreenPos2, projectionCamera, WorldTransformPlane);
            var newWorldPos1 = ProjectionUtils.CameraToPlaneProjection(newScreenPos1, projectionCamera, WorldTransformPlane);
            var newWorldPos2 = ProjectionUtils.CameraToPlaneProjection(newScreenPos2, projectionCamera, WorldTransformPlane);
            var newVector = newWorldPos2 - newWorldPos1;
            var oldVector = oldWorldPos2 - oldWorldPos1;

            Vector2 oldScreenCenter = (oldScreenPos1 + oldScreenPos2)*.5f;
            Vector2 newScreenCenter = (newScreenPos1 + newScreenPos2)*.5f;

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

            oldWorldCenter = ProjectionUtils.CameraToPlaneProjection(oldScreenCenter, projectionCamera, WorldTransformPlane);
            newWorldCenter = ProjectionUtils.CameraToPlaneProjection(newScreenCenter, projectionCamera, WorldTransformPlane);

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
                        PreviousWorldTransformCenter = oldWorldCenter;

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
        protected override void onBegan()
        {
            base.onBegan();
            if (UseSendMessage)
            {
                SendMessageTarget.SendMessage(ROTATE_STARTED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(ROTATED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            if (UseSendMessage) SendMessageTarget.SendMessage(ROTATED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (UseSendMessage) SendMessageTarget.SendMessage(ROTATE_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (UseSendMessage && PreviousState != GestureState.Possible) SendMessageTarget.SendMessage(ROTATE_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (UseSendMessage && PreviousState != GestureState.Possible) SendMessageTarget.SendMessage(ROTATE_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
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