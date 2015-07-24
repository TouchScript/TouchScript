/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Layers;
using TouchScript.Utils;
using TouchScript.Utils.Geom;
using UnityEngine;

namespace TouchScript.Gestures
{
    [AddComponentMenu("TouchScript/Gestures/Transform Gesture")]
    public class TransformGesture : Gesture
    {

        #region Constants

        [Flags]
        public enum TransformType
        {
            Translate = 0x1,
            Rotate = 0x2,
            Scale = 0x4
        }

        /// <summary>
        /// Transform's projection type.
        /// </summary>
        public enum ProjectionType
        {
            /// <summary>
            /// Use a plane with normal vector defined by layer.
            /// </summary>
            Layer,

            /// <summary>
            /// Use a plane with certain normal vector in local coordinates.
            /// </summary>
            Local,

            /// <summary>
            /// Use a plane with certain normal vector in global coordinates.
            /// </summary>
            Global,

            None
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
        /// Transform's projection type.
        /// </summary>
        public ProjectionType Projection
        {
            get { return projection; }
            set
            {
                if (projection == value) return;
                projection = value;
                if (Application.isPlaying) updateProjectionPlane();
            }
        }

        /// <summary>
        /// Transform's projection plane normal.
        /// </summary>
        public Vector3 ProjectionNormal
        {
            get
            {
                if (projection == ProjectionType.Layer) return projectionLayer.WorldProjectionNormal;
                return projectionNormal;
            }
            set
            {
                if (projection == ProjectionType.Layer) projection = ProjectionType.Local;
                value.Normalize();
                if (projectionNormal == value) return;
                projectionNormal = value;
                if (Application.isPlaying) updateProjectionPlane();
            }
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
        /// Previous global transform center in 3D.
        /// </summary>
        //public Vector3 PreviousWorldTransformCenter { get; protected set; }

        /// <summary>
        /// Global transform center in 3D.
        /// </summary>
        //public Vector3 WorldTransformCenter { get; protected set; }

        /// <summary>
        /// Plane where transformation occured.
        /// </summary>
        public Plane TransformPlane
        {
            get { return transformPlane; }
        }

        /// <summary>
        /// Gets delta position in world coordinates.
        /// </summary>
        /// <value>Delta position between this frame and the last frame in world coordinates.</value>
        public Vector3 DeltaPosition { get; private set; }

        public float DeltaRotation { get; private set; }

        /// <summary>
        /// Contains local delta scale when gesture is recognized.
        /// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
        /// </summary>
        public float DeltaScale { get; private set; }

        /// <summary>
        /// Gets delta position in local coordinates.
        /// </summary>
        /// <value>Delta position between this frame and the last frame in local coordinates.</value>
        public Vector3 LocalDeltaPosition
        {
            get { return TransformUtils.GlobalToLocalVector(cachedTransform, DeltaPosition); }
        }

        /// <summary>
        /// Gets rotation axis of the gesture in world coordinates.
        /// </summary>
        /// <value>Rotation axis of the gesture in world coordinates.</value>
        public Vector3 RotationAxis
        {
            get
            {
                return transformPlane.normal;
            }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (TouchManager.IsInvalidPosition(screenPosition)) return base.ScreenPosition;
                return screenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (TouchManager.IsInvalidPosition(previousScreenPosition)) return base.PreviousScreenPosition;
                return previousScreenPosition;
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
        /// Transform's center point screen position.
        /// This is different from Gesture.ScreenPosition since it can be not just centroid of touch points. Also we calculate it anyway so why not cache it too.
        /// </summary>
        protected Vector2 screenPosition;

        /// <summary>
        /// Transform's center point previous screen position.
        /// This is different from Gesture.ScreenPosition since it can be not just centroid of touch points. Also we calculate it anyway so why not cache it too.
        /// </summary>
        protected Vector2 previousScreenPosition;

        protected float screenTransformPixelThreshold;
        protected float screenTransformPixelThresholdSquared;

        [SerializeField] private TransformType type = TransformType.Translate | TransformType.Scale |
                                                      TransformType.Rotate;

        [SerializeField] private float minScreenPointsDistance = .5f;
        [SerializeField] private float screenTransformThreshold = 0.5f;
        [SerializeField] private float rotationThreshold = 3f;
        [SerializeField] private float scalingThreshold = .5f;
        [SerializeField] private ProjectionType projection = ProjectionType.Layer;
        [SerializeField] private Vector3 projectionNormal = Vector3.forward;

        private Collider cachedCollider;
        private TouchLayer projectionLayer;
        private Plane transformPlane;

        private Vector2 screenPixelTranslationBuffer;
        private float screenPixelRotationBuffer;
        private float angleBuffer;
        private float screenPixelScalingBuffer;
        private float scaleBuffer;

        private bool isTransforming = false;

        #endregion

        #region Public methods

        public void ApplyTransform(Transform target)
        {
            if (DeltaPosition != Vector3.zero) target.position += DeltaPosition;
            if (!Mathf.Approximately(DeltaRotation, 0f)) target.rotation *= Quaternion.AngleAxis(DeltaRotation, RotationAxis);
            if (!Mathf.Approximately(DeltaScale, 1f)) target.localScale *= DeltaScale;
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            transformPlane = new Plane();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            cachedCollider = GetComponent<Collider>();
            updateMinScreenPointsDistance();
            updateScreenTransformThreshold();
            updateProjectionPlane();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

            if (touches.Count == NumTouches)
            {
                projectionLayer = activeTouches[0].Layer;
                updateProjectionPlane();
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            DeltaPosition = Vector3.zero;
            DeltaRotation = 0;
            DeltaScale = 1f;

            var activePoints = getNumPoints();
            // one touch or one cluster (points might be too close to each other for 2 clusters)
            if (activePoints == 1)
            {
                if ((Type & TransformType.Translate) == 0) return; // don't look for translates

                doTranslate(getPointPreviousScreenPosition(0), getPointScreenPosition(0));
            }
            else if (activePoints >= 2)
            {
                // Make sure that we actually care about the touches moved.
                if (!relevantTouches(touches)) return;

                var newScreenPos1 = getPointScreenPosition(0);
                var newScreenPos2 = getPointScreenPosition(1);

                var rotationEnabled = (Type & TransformType.Rotate) == TransformType.Rotate;
                var scalingEnabled = (Type & TransformType.Scale) == TransformType.Scale;
                if (rotationEnabled && scalingEnabled)
                {
                    var newScreenDelta = newScreenPos2 - newScreenPos1;
                    if (newScreenDelta.sqrMagnitude > minScreenPointsPixelDistanceSquared)
                    {
                        // Here we can't reuse last frame screen positions because points 0 and 1 can change.
                        // For example if the first of 3 fingers is lifted off.
                        var oldScreenPos1 = getPointPreviousScreenPosition(0);
                        var oldScreenPos2 = getPointPreviousScreenPosition(1);

                        if (rotationEnabled)
                        {
                            if (isTransforming)
                            {
                                if (projection == ProjectionType.None)
                                {
                                    var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
                                    DeltaRotation =
                                        (Mathf.Atan2(newScreenDelta.y, newScreenDelta.x) - Mathf.Atan2(oldScreenDelta.y, oldScreenDelta.x)) * Mathf.Rad2Deg;
                                }
                                else
                                {
                                    DeltaRotation = doProjectedRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2);
                                }
                            }
                            else
                            {
                                float d1, d2;
                                // Find how much we moved perpendicular to the line (oldScreenPos1, oldScreenPos2)
                                TwoD.PointToLineDistance2(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2,
                                    out d1, out d2);
                                screenPixelRotationBuffer += (d1 - d2);

                                if (projection == ProjectionType.None)
                                {
                                    var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
                                    angleBuffer +=
                                        (Mathf.Atan2(newScreenDelta.y, newScreenDelta.x) - Mathf.Atan2(oldScreenDelta.y, oldScreenDelta.x)) * Mathf.Rad2Deg;
                                }
                                else
                                {
                                    angleBuffer += doProjectedRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2);
                                }

                                if (screenPixelRotationBuffer * screenPixelRotationBuffer >= screenTransformPixelThresholdSquared)
                                {
                                    isTransforming = true;
                                    DeltaRotation = angleBuffer;
                                }
                            }
                        }

                        if (scalingEnabled)
                        {

                            if (isTransforming)
                            {
                                if (projection == ProjectionType.None)
                                {
                                    DeltaScale = newScreenDelta.magnitude / (oldScreenPos2 - oldScreenPos1).magnitude;
                                }
                                else
                                {
                                    DeltaScale = doProjectedScale(oldScreenPos1, oldScreenPos2, newScreenPos1,
                                        newScreenPos2);
                                }
                            }
                            else
                            {
                                var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
                                var newDistance = newScreenDelta.magnitude;
                                var oldDistance = oldScreenDelta.magnitude;
                                screenPixelScalingBuffer += newDistance - oldDistance;

                                if (projection == ProjectionType.None)
                                {
                                    scaleBuffer *= newDistance/oldDistance;
                                }
                                else
                                {
                                    scaleBuffer *= doProjectedScale(oldScreenPos1, oldScreenPos2, newScreenPos1,
                                        newScreenPos2);
                                }

                                if (screenPixelScalingBuffer * screenPixelScalingBuffer >= screenTransformPixelThresholdSquared)
                                {
                                    isTransforming = true;
                                    DeltaScale = scaleBuffer;
                                }
                            }
                        }
                    }
                }
                if ((Type & TransformType.Translate) == TransformType.Translate)
                {
                    doTranslate((getPointPreviousScreenPosition(0) + getPointPreviousScreenPosition(1)) / 2, (newScreenPos1 + newScreenPos2) / 2);
                }
            }

            if (DeltaPosition != Vector3.zero || DeltaRotation != 0 || DeltaScale != 1)
            {
                switch (State)
                {
                    case GestureState.Possible:
                    case GestureState.Began:
                    case GestureState.Changed:
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

        private void doTranslate(Vector2 oldScreenCenter, Vector2 newScreenCenter)
        {
            if (isTransforming)
            {
                if (projection == ProjectionType.None)
                    DeltaPosition = new Vector3(newScreenCenter.x - oldScreenCenter.x, newScreenCenter.y - oldScreenCenter.y,
                        0);
                else
                    DeltaPosition = projectionLayer.ProjectTo(newScreenCenter, TransformPlane) - projectionLayer.ProjectTo(oldScreenCenter, TransformPlane);
            }
            else
            {
                screenPixelTranslationBuffer += newScreenCenter - oldScreenCenter;
                if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
                {
                    isTransforming = true;
                    if (projection == ProjectionType.None)
                        DeltaPosition = screenPixelTranslationBuffer;
                    else
                        DeltaPosition = projectionLayer.ProjectTo(newScreenCenter, TransformPlane) - projectionLayer.ProjectTo(newScreenCenter - screenPixelTranslationBuffer, TransformPlane);
                }
            }
        }

        private float doProjectedRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1, Vector2 newScreenPos2)
        {
            var newVector = projectionLayer.ProjectTo(newScreenPos2, TransformPlane) - projectionLayer.ProjectTo(newScreenPos1, TransformPlane);
            var oldVector = projectionLayer.ProjectTo(oldScreenPos2, TransformPlane) - projectionLayer.ProjectTo(oldScreenPos1, TransformPlane);
            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), TransformPlane.normal) < 0)
                angle = -angle;
            return angle;
        }

        private float doProjectedScale(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1, Vector2 newScreenPos2)
        {
            var newVector = projectionLayer.ProjectTo(newScreenPos2, TransformPlane) - projectionLayer.ProjectTo(newScreenPos1, TransformPlane);
            var oldVector = projectionLayer.ProjectTo(oldScreenPos2, TransformPlane) - projectionLayer.ProjectTo(oldScreenPos1, TransformPlane);
            return newVector.magnitude / oldVector.magnitude;
        }

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
            transformedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
            {
                SendMessageTarget.SendMessage(TRANSFORM_START_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
                SendMessageTarget.SendMessage(TRANSFORM_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
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

            //WorldTransformCenter = TouchManager.INVALID_POSITION;
            //PreviousWorldTransformCenter = TouchManager.INVALID_POSITION;
            screenPosition = TouchManager.INVALID_POSITION;
            previousScreenPosition = TouchManager.INVALID_POSITION;

            DeltaPosition = Vector3.zero;
            DeltaRotation = 0f;
            DeltaScale = 1f;

            screenPixelTranslationBuffer = Vector2.zero;
            screenPixelRotationBuffer = 0f;
            angleBuffer = 0;
            screenPixelScalingBuffer = 0f;
            scaleBuffer = 1f;

            isTransforming = false;
        }

        #endregion

        #region Protected methods

        protected virtual int getNumPoints()
        {
            return NumTouches;
        }

        /// <summary>
        /// Checks if there are touch points in the list which matter for the gesture.
        /// </summary>
        /// <param name="touches">List of touch points</param>
        /// <returns>True if there are relevant touch points, False otherwise.</returns>
        protected virtual bool relevantTouches(IList<ITouch> touches)
        {
            var result = false;
            // We care only about the first and the second touch points
            foreach (var touch in touches)
            {
                if (touch == activeTouches[0] || touch == activeTouches[1])
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return activeTouches[index].Position;
        }

        /// <summary>
        /// Returns previous screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            if (index < 0) index = 0;
            else if (index > 1) index = 1;
            return activeTouches[index].PreviousPosition;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Updates projection plane based on options set.
        /// </summary>
        private void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;

            if (projection == ProjectionType.None)
            {
                transformPlane = new Plane(Vector3.forward, 0);
                return;
            }

            Vector3 center;
            if (cachedCollider != null) center = cachedCollider.bounds.center;
            else center = cachedTransform.position;


            switch (projection)
            {
                case ProjectionType.Layer:
                    if (projectionLayer == null)
                        transformPlane = new Plane(cachedTransform.TransformDirection(Vector3.forward), center);
                    else transformPlane = new Plane(projectionLayer.WorldProjectionNormal, center);
                    break;
                case ProjectionType.Local:
                    transformPlane = new Plane(cachedTransform.TransformDirection(projectionNormal), center);
                    break;
                case ProjectionType.Global:
                    transformPlane = new Plane(projectionNormal, center);
                    break;
            }
        }

        private void updateMinScreenPointsDistance()
        {
            minScreenPointsPixelDistance = minScreenPointsDistance*touchManager.DotsPerCentimeter;
            minScreenPointsPixelDistanceSquared = minScreenPointsPixelDistance * minScreenPointsPixelDistance;
        }

        private void updateScreenTransformThreshold()
        {
            screenTransformPixelThreshold = screenTransformThreshold * touchManager.DotsPerCentimeter;
            screenTransformPixelThresholdSquared = screenTransformPixelThreshold * screenTransformPixelThreshold;
        }

    #endregion

    }
}
