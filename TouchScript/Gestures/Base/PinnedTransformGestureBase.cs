/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
#if DEBUG
using TouchScript.Utils.Debug;
#endif
using UnityEngine;

namespace TouchScript.Gestures.Base
{
    public abstract class PinnedTrasformGestureBase : Gesture
    {
        #region Constants

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum TransformType
        {
            /// <summary>
            /// 
            /// </summary>
            Rotation = 0x2,

            /// <summary>
            /// 
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

        /// <summary>
        /// Occurs when gesture starts.
        /// </summary>
        public event EventHandler<EventArgs> TransformStarted
        {
            add { transformStartedInvoker += value; }
            remove { transformStartedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when gesture updates.
        /// </summary>
        public event EventHandler<EventArgs> Transformed
        {
            add { transformedInvoker += value; }
            remove { transformedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when gesture ends.
        /// </summary>
        public event EventHandler<EventArgs> TransformCompleted
        {
            add { transformCompletedInvoker += value; }
            remove { transformCompletedInvoker -= value; }
        }

        // iOS Events AOT hack
        private EventHandler<EventArgs> transformStartedInvoker, transformedInvoker, transformCompletedInvoker;

        #endregion

        #region Public properties

        public TransformType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets or sets minimum distance in cm for touch points to move for gesture to begin. 
        /// </summary>
        /// <value>Minimum value in cm user must move their fingers to start this gesture.</value>
        public float ScreenTransformThreshold
        {
            get { return screenTransformThreshold; }
            set
            {
                screenTransformThreshold = value;
                updateScreenTransformThreshold();
            }
        }

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

        protected float screenTransformPixelThreshold;
        protected float screenTransformPixelThresholdSquared;
        protected Collider cachedCollider;

        protected float deltaRotation;
        protected float deltaScale;

        protected Vector2 screenPixelTranslationBuffer;
        protected float screenPixelRotationBuffer;
        protected float angleBuffer;
        protected float screenPixelScalingBuffer;
        protected float scaleBuffer;
        protected bool isTransforming = false;

        [SerializeField]
        private TransformType type = TransformType.Scaling | TransformType.Rotation;

        [SerializeField]
        private float screenTransformThreshold = 0.1f;

#if DEBUG
        protected int debugID;
        protected Vector2 debugTouchSize;
#endif

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

#if DEBUG
            debugID = DebugHelper.GetDebugId(this);
            debugTouchSize = Vector2.one * TouchManager.Instance.DotsPerCentimeter * 1.1f;
#endif
        }

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
        protected override void touchesEnded(IList<ITouch> touches)
        {
            base.touchesEnded(touches);

            if (NumTouches == 0)
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        setState(GestureState.Ended);
                        break;
                    default:
                        SetState(GestureState.Failed);
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<ITouch> touches)
        {
            base.touchesCancelled(touches);

            touchesEnded(touches);
        }

        /// <inheritdoc />
        protected override void onBegan()
        {
            base.onBegan();
            transformStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
            {
                SendMessageTarget.SendMessage(TRANSFORM_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            transformedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (PreviousState != GestureState.Possible)
            {
                transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null)
                    SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this,
                        SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (PreviousState != GestureState.Possible)
            {
                transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null)
                    SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this,
                        SendMessageOptions.DontRequireReceiver);
            }
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

#if DEBUG
            clearDebug();
#endif
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches">List of touch points</param>
        /// <returns>True if there are relevant touch points, False otherwise.</returns>
        protected virtual bool relevantTouches(IList<ITouch> touches)
        {
            // We care only about the first touch point
            foreach (var touch in touches)
            {
                if (touch == activeTouches[0]) return true;
            }
            return false;
        }

#if DEBUG
        protected virtual void clearDebug()
        {
            GLDebug.RemoveFigure(debugID);
            GLDebug.RemoveFigure(debugID + 1);
            GLDebug.RemoveFigure(debugID + 2);
        }

        protected virtual void drawDebug(Vector2 point1, Vector2 point2)
        {
            var color = State == GestureState.Possible ? Color.red : Color.green;
            GLDebug.DrawSquareScreenSpace(debugID + 1, point2, 0f, debugTouchSize, color, float.PositiveInfinity);
            GLDebug.DrawLineScreenSpace(debugID + 2, point1, point2, color, float.PositiveInfinity);
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