/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Layers;
using TouchScript.Utils;
using TouchScript.Utils.Geom;
using UnityEngine;

#if TOUCHSCRIPT_DEBUG
using System.Collections;
using TouchScript.Utils.Debug;
#endif

namespace TouchScript.Gestures.Base
{
    /// <summary>
    /// Abstract base class for Transform Gestures.
    /// </summary>
    public abstract class TransformGestureBase : Gesture
    {
        #region Constants

        /// <summary>
        /// Types of transformation.
        /// </summary>
        [Flags]
        public enum TransformType
        {
            /// <summary>
            /// Translation.
            /// </summary>
            Translation = 0x1,

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
        /// Gets or sets minimum distance between 2 points in cm for gesture to begin.
        /// </summary>
        /// <value> Minimum distance. </value>
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
        /// Gets delta position between this frame and the last frame in world coordinates.
        /// </summary>
        public Vector3 DeltaPosition
        {
            get { return deltaPosition; }
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
                if (NumTouches == 1) return activeTouches[0].Position;
                return (getPointScreenPosition(0) + getPointScreenPosition(1)) * .5f;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (NumTouches == 0) return TouchManager.INVALID_POSITION;
                if (NumTouches == 1) return activeTouches[0].PreviousPosition;
                return (getPointPreviousScreenPosition(0) + getPointPreviousScreenPosition(1)) * .5f;
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

        /// <summary>
        /// <see cref="ScreenTransformThreshold"/> in pixels.
        /// </summary>
        protected float screenTransformPixelThreshold;

        /// <summary>
        /// <see cref="ScreenTransformThreshold"/> in pixels squared.
        /// </summary>
        protected float screenTransformPixelThresholdSquared;

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

        /// <summary>
        /// Touches moved this frame.
        /// </summary>
        protected List<TouchPoint> movedTouches = new List<TouchPoint>(5);

        /// <summary>
        /// Layer projection parameters.
        /// </summary>
        protected ProjectionParams projectionParams;

        [SerializeField]
        private TransformType type = TransformType.Translation | TransformType.Scaling |
                                     TransformType.Rotation;

        [SerializeField]
        private float minScreenPointsDistance = 0.5f;

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
            debugTouchSize = Vector2.one*TouchManager.Instance.DotsPerCentimeter*1.1f;
        }
#endif

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            updateMinScreenPointsDistance();
            updateScreenTransformThreshold();

            TouchManager.Instance.FrameFinished += frameFinishedHandler;
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            if (TouchManager.Instance != null) TouchManager.Instance.FrameFinished -= frameFinishedHandler;
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchBegan(TouchPoint touch)
        {
            base.touchBegan(touch);

            if (NumTouches == 1) projectionParams = activeTouches[0].ProjectionParams;

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
#if TOUCHSCRIPT_DEBUG
            else drawDebugDelayed(getNumPoints());
#endif
        }

        /// <inheritdoc />
        protected override void touchMoved(TouchPoint touch)
        {
            base.touchMoved(touch);

            movedTouches.Add(touch);
        }

        /// <inheritdoc />
        protected override void touchEnded(TouchPoint touch)
        {
            base.touchEnded(touch);

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

#if TOUCHSCRIPT_DEBUG
            else drawDebugDelayed(getNumPoints());
#endif
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

            // need to clear moved touches updateMoved() wouldn't fire in a wrong state
            // if moved and released the same frame movement data will be lost
            movedTouches.Clear();
            if (transformCompletedInvoker != null)
                transformCompletedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(TRANSFORM_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();

            movedTouches.Clear();
        }

        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();

            movedTouches.Clear();
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

            movedTouches.Clear();
            isTransforming = false;

#if TOUCHSCRIPT_DEBUG
            clearDebug();
#endif
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Calculates rotation.
        /// </summary>
        /// <param name="oldScreenPos1"> Finger one old screen position. </param>
        /// <param name="oldScreenPos2"> Finger two old screen position. </param>
        /// <param name="newScreenPos1"> Finger one new screen position. </param>
        /// <param name="newScreenPos2"> Finger two new screen position. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Angle in degrees. </returns>
        protected virtual float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                           Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            return 0;
        }

        /// <summary>
        /// Calculates scaling.
        /// </summary>
        /// <param name="oldScreenPos1"> Finger one old screen position. </param>
        /// <param name="oldScreenPos2"> Finger two old screen position. </param>
        /// <param name="newScreenPos1"> Finger one new screen position. </param>
        /// <param name="newScreenPos2"> Finger two new screen position. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Multiplicative delta scaling. </returns>
        protected virtual float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                          Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            return 1;
        }

        /// <summary>
        /// Calculates single finger translation.
        /// </summary>
        /// <param name="oldScreenPos"> Finger old screen position. </param>
        /// <param name="newScreenPos"> Finger new screen position. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Delta translation vector. </returns>
        protected virtual Vector3 doOnePointTranslation(Vector2 oldScreenPos, Vector2 newScreenPos,
                                                        ProjectionParams projectionParams)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Calculated two finger translation with respect to rotation and scaling.
        /// </summary>
        /// <param name="oldScreenPos1"> Finger one old screen position. </param>
        /// <param name="oldScreenPos2"> Finger two old screen position. </param>
        /// <param name="newScreenPos1"> Finger one new screen position. </param>
        /// <param name="newScreenPos2"> Finger two new screen position. </param>
        /// <param name="dR"> Calculated delta rotation. </param>
        /// <param name="dS"> Calculated delta scaling. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Delta translation vector. </returns>
        protected virtual Vector3 doTwoPointTranslation(Vector2 oldScreenPos1, Vector2 oldScreenPos2,
                                                        Vector2 newScreenPos1, Vector2 newScreenPos2, float dR, float dS, ProjectionParams projectionParams)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Gets the number of points.
        /// </summary>
        /// <returns> Number of points. </returns>
        protected virtual int getNumPoints()
        {
            return NumTouches;
        }

        /// <summary>
        /// Checks if there are touch points in moved list which matter for the gesture.
        /// </summary>
        /// <returns> <c>true</c> if there are relevant touch points; <c>false</c> otherwise.</returns>
        protected virtual bool relevantTouches1()
        {
            // We care only about the first touch point
            var count = movedTouches.Count;
            for (var i = 0; i < count; i++)
            {
                if (movedTouches[i] == activeTouches[0]) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if there are touch points in moved list which matter for the gesture.
        /// </summary>
        /// <returns> <c>true</c> if there are relevant touch points; <c>false</c> otherwise.</returns>
        protected virtual bool relevantTouches2()
        {
            // We care only about the first and the second touch points
            var count = movedTouches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = movedTouches[i];
                if (touch == activeTouches[0] || touch == activeTouches[1]) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index"> The index. </param>
        protected virtual Vector2 getPointScreenPosition(int index)
        {
            return activeTouches[index].Position;
        }

        /// <summary>
        /// Returns previous screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index"> The index. </param>
        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            return activeTouches[index].PreviousPosition;
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

        private void updateMoved()
        {
#if TOUCHSCRIPT_DEBUG
            drawDebugDelayed(getNumPoints());
#endif

            var numPoints = getNumPoints();
            if (numPoints == 0) return;

            var translationEnabled = (Type & TransformType.Translation) == TransformType.Translation;
            var rotationEnabled = (Type & TransformType.Rotation) == TransformType.Rotation;
            var scalingEnabled = (Type & TransformType.Scaling) == TransformType.Scaling;

            var dP = deltaPosition = Vector3.zero;
            var dR = deltaRotation = 0;
            var dS = deltaScale = 1f;

            // one touch or one cluster (points might be too close to each other for 2 clusters)

            if (numPoints == 1 || (!rotationEnabled && !scalingEnabled))
            {
                if (!translationEnabled) return; // don't look for translates
                if (!relevantTouches1()) return;

                // translate using one point
                dP = doOnePointTranslation(getPointPreviousScreenPosition(0), getPointScreenPosition(0), projectionParams);
            }
            else if (numPoints >= 2)
            {
                // Make sure that we actually care about the touch moved.
                if (!relevantTouches2()) return;

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
                            dR = doRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);
                        }
                        else
                        {
                            float d1, d2;
                            // Find how much we moved perpendicular to the line (oldScreenPos1, oldScreenPos2)
                            TwoD.PointToLineDistance2(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2,
                                out d1, out d2);
                            screenPixelRotationBuffer += (d1 - d2);
                            angleBuffer += doRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);

                            if (screenPixelRotationBuffer * screenPixelRotationBuffer >=
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
                            dS *= doScaling(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);
                        }
                        else
                        {
                            var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
                            var newDistance = newScreenDelta.magnitude;
                            var oldDistance = oldScreenDelta.magnitude;
                            screenPixelScalingBuffer += newDistance - oldDistance;
                            scaleBuffer *= doScaling(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);

                            if (screenPixelScalingBuffer * screenPixelScalingBuffer >=
                                screenTransformPixelThresholdSquared)
                            {
                                isTransforming = true;
                                dS = scaleBuffer;
                            }
                        }
                    }

                    if (translationEnabled)
                    {
                        if (dR == 0 && dS == 1) dP = doOnePointTranslation(oldScreenPos1, newScreenPos1, projectionParams);
                        else
                            dP = doTwoPointTranslation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, dR, dS, projectionParams);
                    }
                }
                else if (translationEnabled)
                {
                    // points are too close, translate using one point
                    dP = doOnePointTranslation(oldScreenPos1, newScreenPos1, projectionParams);
                }
            }

            if (dP != Vector3.zero || dR != 0 || dS != 1)
            {
                if (State == GestureState.Possible)
                {
                    if (touchesNumState == TouchesNumState.InRange) setState(GestureState.Began);
                    else
                    {
                        // Wrong number of touches!
                        setState(GestureState.Failed);
                        return;
                    }
                }
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

        private void updateMinScreenPointsDistance()
        {
            minScreenPointsPixelDistance = minScreenPointsDistance * touchManager.DotsPerCentimeter;
            minScreenPointsPixelDistanceSquared = minScreenPointsPixelDistance * minScreenPointsPixelDistance;
        }

        private void updateScreenTransformThreshold()
        {
            screenTransformPixelThreshold = screenTransformThreshold * touchManager.DotsPerCentimeter;
            screenTransformPixelThresholdSquared = screenTransformPixelThreshold * screenTransformPixelThreshold;
        }

        private void frameFinishedHandler(object sender, EventArgs eventArgs)
        {
            if (movedTouches.Count > 0)
            {
                updateMoved();
                movedTouches.Clear();
            }
        }

        #endregion
    }
}