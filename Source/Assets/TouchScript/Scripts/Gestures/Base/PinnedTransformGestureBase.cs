/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Layers;
using TouchScript.Utils;
using UnityEngine;

#if TOUCHSCRIPT_DEBUG
using System.Collections;
using TouchScript.Utils.Debug;
#endif

namespace TouchScript.Gestures.Base
{
    /// <summary>
    /// Abstract base class for Pinned Transform Gestures.
    /// </summary>
    public abstract class PinnedTrasformGestureBase : Gesture
    {
        #region Constants

        /// <summary>
        /// Types of transformation.
        /// </summary>
        [Flags]
        public enum TransformType
        {
            /// <summary>
            /// Rotation.
            /// </summary>
            Rotation = 0x2,

            /// <summary>
            /// Scaling.
            /// </summary>
            Scaling = 0x4
        }

        /// <summary>
        /// Message name when gesture starts
        /// </summary>
        public const string TRANSFORM_START_MESSAGE = "OnTransformStart";

        /// <summary>
        /// Message name when gesture updates
        /// </summary>
        public const string TRANSFORM_MESSAGE = "OnTransform";

        /// <summary>
        /// Message name when gesture ends
        /// </summary>
        public const string TRANSFORM_COMPLETE_MESSAGE = "OnTransformComplete";

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<EventArgs> TransformStarted
        {
            add { transformStartedInvoker += value; }
            remove { transformStartedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> Transformed
        {
            add { transformedInvoker += value; }
            remove { transformedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> TransformCompleted
        {
            add { transformCompletedInvoker += value; }
            remove { transformCompletedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> transformStartedInvoker, transformedInvoker, transformCompletedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets types of transformation this gesture supports.
        /// </summary>
        /// <value> Type flags. </value>
        public TransformType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets or sets minimum distance in cm for touch points to move for gesture to begin. 
        /// </summary>
        /// <value> Minimum value in cm user must move their fingers to start this gesture. </value>
        public float ScreenTransformThreshold
        {
            get { return screenTransformThreshold; }
            set
            {
                screenTransformThreshold = value;
                updateScreenTransformThreshold();
            }
        }

        /// <summary>
        /// Gets delta rotation between this frame and last frame in degrees.
        /// </summary>
        public float DeltaRotation
        {
            get { return deltaRotation; }
        }

        /// <summary>
        /// Contains local delta scale when gesture is recognized.
        /// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
        /// </summary>
        public float DeltaScale
        {
            get { return deltaScale; }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (NumTouches == 0) return TouchManager.INVALID_POSITION;
                return activeTouches[0].Position;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (NumTouches == 0) return TouchManager.INVALID_POSITION;
                return activeTouches[0].PreviousPosition;
            }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// <see cref="ScreenTransformThreshold"/> in pixels.
        /// </summary>
        protected float screenTransformPixelThreshold;

        /// <summary>
        /// <see cref="ScreenTransformThreshold"/> in pixels squared.
        /// </summary>
        protected float screenTransformPixelThresholdSquared;

        /// <summary>
        /// The cached collider.
        /// </summary>
        protected Collider cachedCollider;

        /// <summary>
        /// Calculated delta rotation.
        /// </summary>
        protected float deltaRotation;

        /// <summary>
        /// Calculated delta scale.
        /// </summary>
        protected float deltaScale;

        /// <summary>
        /// Translation buffer.
        /// </summary>
        protected Vector2 screenPixelTranslationBuffer;

        /// <summary>
        /// Rotation buffer.
        /// </summary>
        protected float screenPixelRotationBuffer;

        /// <summary>
        /// Angle buffer.
        /// </summary>
        protected float angleBuffer;

        /// <summary>
        /// Screen space scaling buffer.
        /// </summary>
        protected float screenPixelScalingBuffer;

        /// <summary>
        /// Scaling buffer.
        /// </summary>
        protected float scaleBuffer;

        /// <summary>
        /// Indicates whether transformation started;
        /// </summary>
        protected bool isTransforming = false;

        [SerializeField]
        private TransformType type = TransformType.Scaling | TransformType.Rotation;

        [SerializeField]
        private float screenTransformThreshold = 0.1f;

        #endregion

        #region Unity methods

#if TOUCHSCRIPT_DEBUG
    /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            debugID = DebugHelper.GetDebugId(this);
            debugTouchSize = Vector2.one * TouchManager.Instance.DotsPerCentimeter * 1.1f;
        }
#endif

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            cachedCollider = GetComponent<Collider>();
            updateScreenTransformThreshold();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (touchesNumState == TouchesNumState.PassedMaxThreshold ||
                touchesNumState == TouchesNumState.PassedMinMaxThreshold)
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        setState(GestureState.Ended);
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (touchesNumState == TouchesNumState.PassedMinThreshold)
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        setState(GestureState.Ended);
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void onBegan()
        {
            base.onBegan();
            if (transformStartedInvoker != null) transformStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
            {
                SendMessageTarget.SendMessage(TRANSFORM_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            if (transformedInvoker != null) transformedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();

            if (transformCompletedInvoker != null)
                transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            deltaRotation = 0f;
            deltaScale = 1f;

            screenPixelTranslationBuffer = Vector2.zero;
            screenPixelRotationBuffer = 0f;
            angleBuffer = 0;
            screenPixelScalingBuffer = 0f;
            scaleBuffer = 1f;

            isTransforming = false;

#if TOUCHSCRIPT_DEBUG
            clearDebug();
#endif
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches"> List of touch points </param>
        /// <returns> <c>true</c> if there are relevant touch points; <c>false</c> otherwise.</returns>
        protected virtual bool relevantTouches(IList<TouchPoint> touches)
        {
            // We care only about the first touch point
            var count = touches.Count;
            for (var i = 0; i < count; i++)
            {
                if (touches[i] == activeTouches[0]) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns screen position of a point with index 0.
        /// </summary>
        protected virtual Vector2 getPointScreenPosition()
        {
            return activeTouches[0].Position;
        }

        /// <summary>
        /// Returns previous screen position of a point with index 0.
        /// </summary>
        protected virtual Vector2 getPointPreviousScreenPosition()
        {
            return activeTouches[0].PreviousPosition;
        }

#if TOUCHSCRIPT_DEBUG
        protected int debugID;
        protected Coroutine debugCoroutine;
        protected Vector2 debugTouchSize;

        protected virtual void clearDebug()
        {
            GLDebug.RemoveFigure(debugID);
            GLDebug.RemoveFigure(debugID + 1);
            GLDebug.RemoveFigure(debugID + 2);

            if (debugCoroutine != null) StopCoroutine(debugCoroutine);
            debugCoroutine = null;
        }

        protected void drawDebugDelayed(Vector2 point1, Vector2 point2)
        {
            if (debugCoroutine != null) StopCoroutine(debugCoroutine);
            debugCoroutine = StartCoroutine(doDrawDebug(point1, point2));
        }

        protected virtual void drawDebug(Vector2 point1, Vector2 point2)
        {
            var color = State == GestureState.Possible ? Color.red : Color.green;
            GLDebug.DrawSquareScreenSpace(debugID + 1, point2, 0f, debugTouchSize, color, float.PositiveInfinity);
            GLDebug.DrawLineScreenSpace(debugID + 2, point1, point2, color, float.PositiveInfinity);
        }

        private IEnumerator doDrawDebug(Vector2 point1, Vector2 point2)
        {
            yield return new WaitForEndOfFrame();

            drawDebug(point1, point2);
        }
#endif

        #endregion

        #region Private functions

        private void updateScreenTransformThreshold()
        {
            screenTransformPixelThreshold = screenTransformThreshold * touchManager.DotsPerCentimeter;
            screenTransformPixelThresholdSquared = screenTransformPixelThreshold * screenTransformPixelThreshold;
        }

        #endregion
    }
}