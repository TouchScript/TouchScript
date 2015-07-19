/*
 * @author Valentin Simonov / http://va.lent.in/
 * @author Antoni Pacciani / http://vertic.al/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Recognizes multiple fingers panning 
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Multi Point Pan Gesture")]
    public class MultiPointPanGesture : MultiPointTransform2DGestureBase
    {
        #region Constants
        
        /// <summary>
        /// Message name when gesture starts
        /// </summary>
        public const string MULTIPAN_START_MESSAGE = "OnMultiPanStart";
        
        /// <summary>
        /// Message name when gesture updates
        /// </summary>
        public const string MULTIPAN_MESSAGE = "OnMultiPan";
        
        /// <summary>
        /// Message name when gesture ends
        /// </summary>
        public const string MULTIPAN_COMPLETE_MESSAGE = "OnMultiPanComplete";
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Occurs when gesture starts.
        /// </summary>
        public event EventHandler<EventArgs> MultiPanStarted
        {
            add { multiPanStartedInvoker += value; }
            remove { multiPanStartedInvoker -= value; }
        }
        
        /// <summary>
        /// Occurs when gesture updates.
        /// </summary>
        public event EventHandler<EventArgs> MultiPanned
        {
            add { multiPannedInvoker += value; }
            remove { multiPannedInvoker -= value; }
        }
        
        /// <summary>
        /// Occurs when gesture ends.
        /// </summary>
        public event EventHandler<EventArgs> MultiPanCompleted
        {
            add { multiPanCompletedInvoker += value; }
            remove { multiPanCompletedInvoker -= value; }
        }
        
        // iOS Events AOT hack
        private EventHandler<EventArgs> multiPanStartedInvoker, multiPannedInvoker, multiPanCompletedInvoker;
        
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
        /// Gets or sets minimum alignment of touch points move direction for gesture to begin. 
        /// </summary>
        /// <value>Minimum alignment of touch points move direction to begin this gesture. -1 = can move in any direction, 1 = must move in the exact same direction</value>
        public float MinAlignment
        {
            get { return minAlignment; }
            set { minAlignment = value; }
        }
        
        /// <summary>
        /// Gets delta position in screen coordinates.
        /// </summary>
        /// <value>Delta position between this frame and the last frame in screen coordinates.</value>
        public Vector2 ScreenDeltaPosition { get; private set; }
        
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
        
        #endregion
        
        #region Private variables
        
        [SerializeField]
        private float movementThreshold = .5f;
        
        [SerializeField]
        private float minAlignment = .8f;
        
        [SerializeField]
        private float minMoveRepartition = .7f;
        
        private Vector2[] movementBuffer;
        private bool oneTouchMovedEnough;
        private bool aligned;
        private bool isMoving = false;
        
        #endregion
        
        #region Gesture callbacks
        
        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            
            if (!gotEnoughTouches()) return;
            if (!relevantTouches(touches)) return;

            base.touchesMoved(touches);

            Vector3 oldWorldCenter, newWorldCenter;
            Vector3 worldDelta = Vector3.zero;
            Vector2[] newScreenPos = new Vector2[MinPointsCount];
            Vector2[] oldScreenPos = new Vector2[MinPointsCount];
            Vector2[] screenPosDelta = new Vector2[MinPointsCount];

            Vector3[] newWorldPos = new Vector3[MinPointsCount];
            Vector3[] oldWorldPos = new Vector3[MinPointsCount];

            Vector2 newScreenMin = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 newScreenMax = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
            Vector2 oldScreenMin = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 oldScreenMax = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

            for(int i = 0; i < MinPointsCount; i++) {
                newScreenPos[i] = getPointScreenPosition(i);
                oldScreenPos[i] = getPointPreviousScreenPosition(i);
                screenPosDelta[i] = newScreenPos[i] - oldScreenPos[i];

                if(newScreenPos[i].x < newScreenMin.x) newScreenMin.x = newScreenPos[i].x;
                if(newScreenPos[i].y < newScreenMin.y) newScreenMin.y = newScreenPos[i].y;
                if(oldScreenPos[i].x < oldScreenMin.x) oldScreenMin.x = oldScreenPos[i].x;
                if(oldScreenPos[i].y < oldScreenMin.y) oldScreenMin.y = oldScreenPos[i].y;

                if(newScreenPos[i].x > newScreenMax.x) newScreenMax.x = newScreenPos[i].x;
                if(newScreenPos[i].y > newScreenMax.y) newScreenMax.y = newScreenPos[i].y;
                if(oldScreenPos[i].x > oldScreenMax.x) oldScreenMax.x = oldScreenPos[i].x;
                if(oldScreenPos[i].y > oldScreenMax.y) oldScreenMax.y = oldScreenPos[i].y;

                newWorldPos[i] = projectionLayer.ProjectTo(newScreenPos[i], WorldTransformPlane);
                oldWorldPos[i] = projectionLayer.ProjectTo(oldScreenPos[i], WorldTransformPlane);
            }

            Vector2 newScreenCenter = (newScreenMin + newScreenMax) * .5f;
            Vector2 oldScreenCenter = (oldScreenMin + oldScreenMax) * .5f;
            Vector2 screenCenterDelta = newScreenCenter - oldScreenCenter;

            if (isMoving)
            {
                oldWorldCenter = projectionLayer.ProjectTo(oldScreenCenter, WorldTransformPlane);
                newWorldCenter = projectionLayer.ProjectTo(newScreenCenter, WorldTransformPlane);
                worldDelta = newWorldCenter - oldWorldCenter;
            }
            else
            {
                var dpiMovementThreshold = MovementThreshold * touchManager.DotsPerCentimeter;

                for(int i = 0; i < MinPointsCount; i++) {
                    movementBuffer[i] += screenPosDelta[i];
                    if(movementBuffer[i].sqrMagnitude > dpiMovementThreshold * dpiMovementThreshold) {
                        oneTouchMovedEnough = true;
                    }
                }

                aligned = true;
                foreach(Vector2 posDelta in screenPosDelta) {
                    if(Vector2.Dot(posDelta.normalized, screenCenterDelta.normalized) < MinAlignment) {
                        aligned = false;
                        continue;
                    }
                }

                if (oneTouchMovedEnough && aligned)
                {
                    isMoving = true;
                    oldWorldCenter = projectionLayer.ProjectTo(oldScreenCenter - screenCenterDelta, WorldTransformPlane);
                    newWorldCenter = projectionLayer.ProjectTo(newScreenCenter, WorldTransformPlane);
                    worldDelta = newWorldCenter - oldWorldCenter;
                }
                else
                {
                    newWorldCenter = projectionLayer.ProjectTo(newScreenCenter - screenCenterDelta, WorldTransformPlane);
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
                        screenPosition = newScreenCenter;
                        previousScreenPosition = oldScreenCenter;
                        PreviousWorldTransformCenter = oldWorldCenter;
                        WorldTransformCenter = newWorldCenter;
                        
                        ScreenDeltaPosition = screenPosition - previousScreenPosition;
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
            if (multiPanStartedInvoker != null) multiPanStartedInvoker(this, EventArgs.Empty);
            if (multiPannedInvoker != null) multiPannedInvoker(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
            {
                SendMessageTarget.SendMessage(MULTIPAN_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(MULTIPAN_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        /// <inheritdoc />
        protected override void onChanged()
        {
            base.onChanged();
            if (multiPannedInvoker != null) multiPannedInvoker(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(MULTIPAN_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }
        
        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (multiPanCompletedInvoker != null) multiPanCompletedInvoker(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(MULTIPAN_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }
        
        /// <inheritdoc />
        protected override void onFailed()
        {
            base.onFailed();
            if (PreviousState != GestureState.Possible)
            {
                if (multiPanCompletedInvoker != null) multiPanCompletedInvoker(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(MULTIPAN_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        /// <inheritdoc />
        protected override void onCancelled()
        {
            base.onCancelled();
            if (PreviousState != GestureState.Possible)
            {
                if (multiPanCompletedInvoker != null) multiPanCompletedInvoker(this, EventArgs.Empty);
                if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(MULTIPAN_COMPLETE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();
            
            WorldDeltaPosition = Vector3.zero;
            movementBuffer = new Vector2[MinPointsCount];
            oneTouchMovedEnough = false;
            isMoving = false;
        }
        
        #endregion
    }
}