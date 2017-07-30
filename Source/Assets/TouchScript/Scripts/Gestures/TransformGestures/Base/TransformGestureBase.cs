/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;

#if TOUCHSCRIPT_DEBUG
using TouchScript.Debugging.GL;
#endif

namespace TouchScript.Gestures.TransformGestures.Base
{
    /// <summary>
    /// Abstract base class for Transform Gestures.
    /// </summary>
    /// <remarks>
    /// <para>Relationship with <see cref="Behaviors.Transformer"/> component requires that if current object position is not exactly the one acquired by transformation events from this gesture (i.e. when smoothing is applied current transform is lagging a bit behind target transform), the gesture has to know about this to calculate translation properly. This is where <see cref="OverrideTargetPosition"/> method comes into play. <see cref="Behaviors.Transformer"/> has to call it after every transform event.</para>
    /// </remarks>
    public abstract class TransformGestureBase : Gesture, ITransformGesture
    {
        #region Constants

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

        /// <summary>
        /// Unity event, occurs when the gesture starts.
        /// </summary>
		public GestureEvent OnTransformStart = new GestureEvent();

        /// <summary>
        /// Unity event, occurs when the gesture is updated.
        /// </summary>
		public GestureEvent OnTransform = new GestureEvent();

        /// <summary>
        /// Unity event, occurs when the gesture ends.
        /// </summary>
		public GestureEvent OnTransformComplete = new GestureEvent();

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets types of transformation this gesture supports.
        /// </summary>
        /// <value> Type flags. </value>
        public TransformGesture.TransformType Type
        {
            get { return type; }
            set
            {
                type = value;
                updateType();
            }
        }

        /// <summary>
        /// Gets or sets minimum distance in cm for pointers to move for gesture to begin. 
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

        /// <inheritdoc />
        public TransformGesture.TransformType TransformMask
        {
            get { return transformMask; }
        }

        /// <inheritdoc />
        public Vector3 DeltaPosition
        {
            get { return deltaPosition; }
        }

        /// <inheritdoc />
        public float DeltaRotation
        {
            get { return deltaRotation; }
        }

        /// <inheritdoc />
        public float DeltaScale
        {
            get { return deltaScale; }
        }

        /// <inheritdoc />
        public Vector3 RotationAxis
        {
            get { return rotationAxis; }
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
        /// The bit mask of what transform operations happened this frame.
        /// </summary>
        protected TransformGesture.TransformType transformMask;

        /// <summary>
        /// Calculated delta position.
        /// </summary>
        protected Vector3 deltaPosition;

        /// <summary>
        /// Calculated delta rotation.
        /// </summary>
        protected float deltaRotation;

        /// <summary>
        /// Calculated delta scale.
        /// </summary>
        protected float deltaScale;

        /// <summary>
        /// Rotation axis to use with deltaRotation.
        /// </summary>
        protected Vector3 rotationAxis = new Vector3(0, 0, 1);

        /// <summary>
        /// Indicates whether transformation started;
        /// </summary>
        protected bool isTransforming = false;

        /// <summary>
        /// Indicates if current position is being overridden for the next frame. <see cref="OverrideTargetPosition"/>.
        /// </summary>
        protected bool targetPositionOverridden = false;


        /// <summary>
        /// Target overridden position. <see cref="OverrideTargetPosition"/>.
        /// </summary>
        protected Vector3 targetPosition;

        /// <summary>
        /// The type of the transforms this gesture can dispatch.
        /// </summary>
        [SerializeField]
        protected TransformGesture.TransformType type = TransformGesture.TransformType.Translation | TransformGesture.TransformType.Scaling |
                                                        TransformGesture.TransformType.Rotation;

        [SerializeField]
        private float screenTransformThreshold = 0.1f;

        #endregion

        #region Public methods

        /// <summary>
        /// Overrides the target position used in calculations this frame. If used, has to be set after every transform event. <see cref="TransformGestureBase"/>.
        /// </summary>
        /// <param name="position">Target position.</param>
        public void OverrideTargetPosition(Vector3 position)
        {
            targetPositionOverridden = true;
            targetPosition = position;
        }

        #endregion

        #region Unity methods

#if TOUCHSCRIPT_DEBUG
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            debugID = DebugHelper.GetDebugId(this);
            debugPointerSize = Vector2.one*TouchManager.Instance.DotsPerCentimeter*1.1f;
        }
#endif

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            updateScreenTransformThreshold();
            updateType();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void pointersPressed(IList<Pointer> pointers)
        {
            base.pointersPressed(pointers);

            if (pointersNumState == PointersNumState.PassedMaxThreshold ||
                pointersNumState == PointersNumState.PassedMinMaxThreshold)
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        setState(GestureState.Ended);
                        break;
                }
            } else if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                setState(GestureState.Possible);
            }
        }

        /// <inheritdoc />
        protected override void pointersReleased(IList<Pointer> pointers)
        {
            base.pointersReleased(pointers);

            if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        setState(GestureState.Ended);
                        break;
                    case GestureState.Possible:
                        setState(GestureState.Idle);
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
                SendMessageTarget.SendMessage(TRANSFORM_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
			if (UseUnityEvents) OnTransformStart.Invoke(this);
        }

        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();

            targetPositionOverridden = false;

            if (transformedInvoker != null) transformedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
			if (UseUnityEvents) OnTransform.Invoke(this);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();

            if (transformCompletedInvoker != null)
                transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
			if (UseUnityEvents) OnTransformComplete.Invoke(this);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            resetValues();
            isTransforming = false;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Updates the type of the gesture.
        /// </summary>
        protected virtual void updateType() {}

        /// <summary>
        /// Resets the frame delta values.
        /// </summary>
        protected void resetValues()
		{
			deltaPosition = Vector3.zero;
			deltaRotation = 0f;
			deltaScale = 1f;
			transformMask = 0;
		}

#if TOUCHSCRIPT_DEBUG
        protected int debugID;
        protected Coroutine debugCoroutine;
        protected Vector2 debugPointerSize;
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