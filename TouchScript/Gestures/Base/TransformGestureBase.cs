/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Utils;
#if DEBUG
using TouchScript.Utils.Debug;
#endif
using TouchScript.Utils.Geom;
using UnityEngine;

namespace TouchScript.Gestures.Base
{
    public abstract class TransformGestureBase : Gesture
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
            Translation = 0x1,

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
        /// Minimum distance between 2 points in cm for gesture to begin.
        /// </summary>
        public virtual float MinScreenPointsDistance
        {
            get { return minScreenPointsDistance; }
            set
            {
                minScreenPointsDistance = value;
                updateMinScreenPointsDistance();
            }
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

        /// <summary>
        /// Gets delta position in world coordinates.
        /// </summary>
        /// <value>Delta position between this frame and the last frame in world coordinates.</value>
        public Vector3 DeltaPosition
        {
            get { return deltaPosition; }
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
                if (NumTouches == 1) return activeTouches[0].Position;
                return (getPointScreenPosition(0) + getPointScreenPosition(1))*.5f;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (NumTouches == 0) return TouchManager.INVALID_POSITION;
                if (NumTouches == 1) return activeTouches[0].PreviousPosition;
                return (getPointPreviousScreenPosition(0) + getPointPreviousScreenPosition(1))*.5f;
            }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// <see cref="MinScreenPointsDistance"/> in pixels for internal use.
        /// </summary>
        protected float minScreenPointsPixelDistance;

        /// <summary>
        /// <see cref="MinScreenPointsDistance"/> squared in pixels for internal use.
        /// </summary>
        protected float minScreenPointsPixelDistanceSquared;

        protected float screenTransformPixelThreshold;
        protected float screenTransformPixelThresholdSquared;

        protected Vector3 deltaPosition;
        protected float deltaRotation;
        protected float deltaScale;

        protected Vector2 screenPixelTranslationBuffer;
        protected float screenPixelRotationBuffer;
        protected float angleBuffer;
        protected float screenPixelScalingBuffer;
        protected float scaleBuffer;
        protected bool isTransforming = false;

        [SerializeField] private TransformType type = TransformType.Translation | TransformType.Scaling |
                                                      TransformType.Rotation;

        [SerializeField] private float minScreenPointsDistance = 0.5f;
        [SerializeField] private float screenTransformThreshold = 0.1f;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

#if DEBUG
            debugID = DebugHelper.GetDebugId(this);
            debugTouchSize = Vector2.one*TouchManager.Instance.DotsPerCentimeter*1.1f;
#endif
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            updateMinScreenPointsDistance();
            updateScreenTransformThreshold();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

#if DEBUG
            drawDebugDelayed(getNumPoints());
#endif
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            var dP = deltaPosition = Vector3.zero;
            var dR = deltaRotation = 0;
            var dS = deltaScale = 1f;

            var activePoints = getNumPoints();

#if DEBUG
            drawDebugDelayed(activePoints);
#endif

            var translationEnabled = (Type & TransformType.Translation) == TransformType.Translation;
            var rotationEnabled = (Type & TransformType.Rotation) == TransformType.Rotation;
            var scalingEnabled = (Type & TransformType.Scaling) == TransformType.Scaling;

            // one touch or one cluster (points might be too close to each other for 2 clusters)
            if (activePoints == 1 || (!rotationEnabled && !scalingEnabled))
            {
                if (!translationEnabled) return; // don't look for translates
                if (!relevantTouches1(touches)) return;

                // translate using one point
                dP = doOnePointTranslation(getPointPreviousScreenPosition(0), getPointScreenPosition(0));
            }
            else
            {
                // Make sure that we actually care about the touches moved.
                if (!relevantTouches2(touches)) return;

                var newScreenPos1 = getPointScreenPosition(0);
                var newScreenPos2 = getPointScreenPosition(1);

                // Here we can't reuse last frame screen positions because points 0 and 1 can change.
                // For example if the first of 3 fingers is lifted off.
                var oldScreenPos1 = getPointPreviousScreenPosition(0);
                var oldScreenPos2 = getPointPreviousScreenPosition(1);

                var newScreenDelta = newScreenPos2 - newScreenPos1;
                if (newScreenDelta.sqrMagnitude > minScreenPointsPixelDistanceSquared)
                {
                    if (rotationEnabled)
                    {
                        if (isTransforming)
                        {
                            dR = doRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2);
                        }
                        else
                        {
                            float d1, d2;
                            // Find how much we moved perpendicular to the line (oldScreenPos1, oldScreenPos2)
                            TwoD.PointToLineDistance2(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2,
                                out d1, out d2);
                            screenPixelRotationBuffer += (d1 - d2);
                            angleBuffer += doRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2);

                            if (screenPixelRotationBuffer*screenPixelRotationBuffer >=
                                screenTransformPixelThresholdSquared)
                            {
                                isTransforming = true;
                                dR = angleBuffer;
                            }
                        }
                    }

                    if (scalingEnabled)
                    {
                        if (isTransforming)
                        {
                            dS *= doScaling(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2);
                        }
                        else
                        {
                            var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
                            var newDistance = newScreenDelta.magnitude;
                            var oldDistance = oldScreenDelta.magnitude;
                            screenPixelScalingBuffer += newDistance - oldDistance;
                            scaleBuffer *= doScaling(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2);

                            if (screenPixelScalingBuffer*screenPixelScalingBuffer >=
                                screenTransformPixelThresholdSquared)
                            {
                                isTransforming = true;
                                dS = scaleBuffer;
                            }
                        }
                    }

                    if (translationEnabled)
                    {
                        if (dR == 0 && dS == 1) dP = doOnePointTranslation(oldScreenPos1, newScreenPos1);
                        else dP = doTwoPointTranslation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, dR, dS);
                    }
                }
                else if (translationEnabled)
                {
                    // points are too close, translate using one point
                    dP = doOnePointTranslation(oldScreenPos1, newScreenPos1);
                }
            }

            if (dP != Vector3.zero || dR != 0 || dS != 1)
            {
                if (State == GestureState.Possible) setState(GestureState.Began);
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        deltaPosition = dP;
                        deltaRotation = dR;
                        deltaScale = dS;
                        setState(GestureState.Changed);
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouch> touches)
        {
            base.touchesEnded(touches);

#if DEBUG
            drawDebugDelayed(getNumPoints());
#endif

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
            if (transformCompletedInvoker != null) transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (PreviousState != GestureState.Possible)
            {
                if (transformCompletedInvoker != null) transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
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
                if (transformCompletedInvoker != null) transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null)
                    SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this,
                        SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            deltaPosition = Vector3.zero;
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
        /// </summary>
        protected virtual float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
            Vector2 newScreenPos2)
        {
            return 0;
        }

        /// <summary>
        /// </summary>
        protected virtual float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
            Vector2 newScreenPos2)
        {
            return 1;
        }

        /// <summary>
        /// </summary>
        protected virtual Vector3 doOnePointTranslation(Vector2 oldScreenPos, Vector2 newScreenPos)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// </summary>
        protected virtual Vector3 doTwoPointTranslation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1, Vector2 newScreenPos2, float dR, float dS)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// </summary>
        protected virtual int getNumPoints()
        {
            return NumTouches;
        }

        protected virtual bool relevantTouches1(IList<ITouch> touches)
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
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches">List of touch points</param>
        /// <returns>True if there are relevant touch points, False otherwise.</returns>
        protected virtual bool relevantTouches2(IList<ITouch> touches)
        {
            // We care only about the first and the second touch points
            var count = touches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = touches[i];
                if (touch == activeTouches[0] || touch == activeTouches[1]) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointScreenPosition(int index)
        {
            return activeTouches[index].Position;
        }

        /// <summary>
        /// Returns previous screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            return activeTouches[index].PreviousPosition;
        }

#if DEBUG
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

        protected void drawDebugDelayed(int touchPoints)
        {
            if (debugCoroutine != null) StopCoroutine(debugCoroutine);
            debugCoroutine = StartCoroutine(doDrawDebug(touchPoints));
        }

        protected virtual void drawDebug(int touchPoints)
        {
            if (!DebugMode) return;

            var color = State == GestureState.Possible ? Color.red : Color.green;
            switch (touchPoints)
            {
                case 1:
                    GLDebug.DrawSquareScreenSpace(debugID, getPointScreenPosition(0), 0f, debugTouchSize, color,
                        float.PositiveInfinity);
                    GLDebug.RemoveFigure(debugID + 1);
                    GLDebug.RemoveFigure(debugID + 2);
                    break;
                default:
                    var newScreenPos1 = getPointScreenPosition(0);
                    var newScreenPos2 = getPointScreenPosition(1);
                    GLDebug.DrawSquareScreenSpace(debugID, newScreenPos1, 0f, debugTouchSize, color,
                        float.PositiveInfinity);
                    GLDebug.DrawSquareScreenSpace(debugID + 1, newScreenPos2, 0f, debugTouchSize, color,
                        float.PositiveInfinity);
                    GLDebug.DrawLineWithCrossScreenSpace(debugID + 2, newScreenPos1, newScreenPos2, .5f,
                        debugTouchSize * .3f, color, float.PositiveInfinity);
                    break;
            }
        }

        private IEnumerator doDrawDebug(int touchPoints)
        {
            yield return new WaitForEndOfFrame();

            drawDebug(touchPoints);
        }
#endif

        #endregion

        #region Private functions

        private void updateMinScreenPointsDistance()
        {
            minScreenPointsPixelDistance = minScreenPointsDistance*touchManager.DotsPerCentimeter;
            minScreenPointsPixelDistanceSquared = minScreenPointsPixelDistance*minScreenPointsPixelDistance;
        }

        private void updateScreenTransformThreshold()
        {
            screenTransformPixelThreshold = screenTransformThreshold*touchManager.DotsPerCentimeter;
            screenTransformPixelThresholdSquared = screenTransformPixelThreshold*screenTransformPixelThreshold;
        }

        #endregion
    }
}