/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Simple Scale gesture which takes into account only the first two touch points.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Simple Scale Gesture")]
    public class SimpleScaleGesture : TwoPointTransform2DGestureBase
    {

        #region Constants

        public const string SCALE_STARTED_MESSAGE = "OnScaleStarted";
        public const string SCALED_MESSAGE = "OnScaled";
        public const string SCALE_STOPPED_MESSAGE = "OnScaleStopped";

        #endregion

        #region Private variables

        [SerializeField]
        private float scalingThreshold = .5f;

        private float scalingBuffer;
        private bool isScaling = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Minimum distance in cm between touch points for gesture to begin.
        /// </summary>
        public float ScalingThreshold
        {
            get { return scalingThreshold; }
            set { scalingThreshold = value; }
        }

        /// <summary>
        /// Contains local delta scale when gesture is recognized.
        /// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
        /// </summary>
        public float LocalDeltaScale { get; private set; }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            if (!gotEnoughTouches()) return;
            if (!relevantTouches(touches)) return;

            Vector3 oldWorldCenter, newWorldCenter;
            var deltaScale = 1f;

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

            Vector2 oldScreenCenter = (oldScreenPos1 + oldScreenPos2) * .5f;
            Vector2 newScreenCenter = (newScreenPos1 + newScreenPos2) * .5f;

            if (isScaling)
            {
                deltaScale = newVector.magnitude/Vector3.Distance(oldWorldPos2, oldWorldPos1);
            } else
            {
                var oldScreenDistance = Vector2.Distance(oldScreenPos1, oldScreenPos2);
                var newScreenDistance = newScreenDelta.magnitude;
                var screenDeltaDistance = newScreenDistance - oldScreenDistance;
                scalingBuffer += screenDeltaDistance;
                var dpiScalingThreshold = ScalingThreshold*touchManager.DotsPerCentimeter;
                if (scalingBuffer*scalingBuffer >= dpiScalingThreshold*dpiScalingThreshold)
                {
                    isScaling = true;
                    var oldScreenDirection = (oldScreenPos2 - oldScreenPos1).normalized;
                    var startScale = (newScreenDistance - scalingBuffer)*.5f;
                    var startVector = oldScreenDirection*startScale;
                    deltaScale = newVector.magnitude/(ProjectionUtils.CameraToPlaneProjection(oldScreenCenter + startVector, projectionCamera, WorldTransformPlane) - ProjectionUtils.CameraToPlaneProjection(oldScreenCenter - startVector, projectionCamera, WorldTransformPlane)).magnitude;
                }
            }

            oldWorldCenter = ProjectionUtils.CameraToPlaneProjection(oldScreenCenter, projectionCamera, WorldTransformPlane);
            newWorldCenter = ProjectionUtils.CameraToPlaneProjection(newScreenCenter, projectionCamera, WorldTransformPlane);

            if (Mathf.Abs(deltaScale - 1f) > 0.00001)
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

                        LocalDeltaScale = deltaScale;

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
                SendMessageTarget.SendMessage(SCALE_STARTED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(SCALED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            if (UseSendMessage) SendMessageTarget.SendMessage(SCALED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (UseSendMessage) SendMessageTarget.SendMessage(SCALE_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (UseSendMessage && PreviousState != GestureState.Possible) SendMessageTarget.SendMessage(SCALE_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (UseSendMessage && PreviousState != GestureState.Possible) SendMessageTarget.SendMessage(SCALE_STOPPED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            scalingBuffer = 0f;
            isScaling = false;
        }

        /// <inheritdoc />
        protected override void restart()
        {
            base.restart();

            LocalDeltaScale = 1f;
        }

        #endregion
    }
}