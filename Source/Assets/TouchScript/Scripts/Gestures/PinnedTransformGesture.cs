/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Gestures.Base;
using TouchScript.Layers;
using TouchScript.Utils.Geom;
#if TOUCHSCRIPT_DEBUG
using TouchScript.Utils.Debug;
#endif
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a transform gesture around center of the object, i.e. one finger rotation, scaling or a combination of these.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Pinned Transform Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_PinnedTransformGesture.htm")]
    public class PinnedTransformGesture : PinnedTrasformGestureBase, ITransformGesture
    {
        #region Constants

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
            Object,

            /// <summary>
            /// Use a plane with certain normal vector in global coordinates.
            /// </summary>
            Global
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets transform's projection type.
        /// </summary>
        /// <value> Projection type. </value>
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
        /// Gets or sets transform's projection plane normal.
        /// </summary>
        /// <value> Projection plane normal. </value>
        public Vector3 ProjectionPlaneNormal
        {
            get
            {
                if (projection == ProjectionType.Layer) return projectionLayer.WorldProjectionNormal;
                return projectionPlaneNormal;
            }
            set
            {
                if (projection == ProjectionType.Layer) projection = ProjectionType.Object;
                value.Normalize();
                if (projectionPlaneNormal == value) return;
                projectionPlaneNormal = value;
                if (Application.isPlaying) updateProjectionPlane();
            }
        }

        /// <summary>
        /// Plane where transformation occured.
        /// </summary>
        public Plane TransformPlane
        {
            get { return transformPlane; }
        }

        /// <summary>
        /// Gets rotation axis of the gesture in world coordinates.
        /// </summary>
        /// <value> Rotation axis of the gesture in world coordinates. </value>
        public Vector3 RotationAxis
        {
            get { return transformPlane.normal; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private ProjectionType projection = ProjectionType.Layer;

        [SerializeField]
        private Vector3 projectionPlaneNormal = Vector3.forward;

        private TouchLayer projectionLayer;
        private Plane transformPlane;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public void ApplyTransform(Transform target)
        {
            if (!Mathf.Approximately(DeltaRotation, 0f))
                target.rotation = Quaternion.AngleAxis(DeltaRotation, RotationAxis) * target.rotation;
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

            updateProjectionPlane();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (State != GestureState.Possible) return;
            if (NumTouches == touches.Count)
            {
                projectionLayer = activeTouches[0].Layer;
                updateProjectionPlane();

#if TOUCHSCRIPT_DEBUG
                drawDebug(activeTouches[0].ProjectionParams.ProjectFrom(cachedTransform.position), activeTouches[0].Position);
#endif
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            var projectionParams = activeTouches[0].ProjectionParams;
            var dR = deltaRotation = 0;
            var dS = deltaScale = 1f;

#if TOUCHSCRIPT_DEBUG
            var worldCenter = cachedTransform.position;
            var screenCenter = projectionParams.ProjectFrom(worldCenter);
            var newScreenPos = getPointScreenPosition();
            drawDebug(screenCenter, newScreenPos);
#endif

            if (touchesNumState != TouchesNumState.InRange) return;

            var rotationEnabled = (Type & TransformType.Rotation) == TransformType.Rotation;
            var scalingEnabled = (Type & TransformType.Scaling) == TransformType.Scaling;
            if (!rotationEnabled && !scalingEnabled) return;
            if (!relevantTouches(touches)) return;

#if !TOUCHSCRIPT_DEBUG
            var theTouch = activeTouches[0];
            var worldCenter = cachedTransform.position;
            var screenCenter = projectionParams.ProjectFrom(worldCenter);
            var newScreenPos = theTouch.Position;
#endif

            // Here we can't reuse last frame screen positions because points 0 and 1 can change.
            // For example if the first of 3 fingers is lifted off.
            var oldScreenPos = getPointPreviousScreenPosition();

            if (rotationEnabled)
            {
                if (isTransforming)
                {
                    dR = doRotation(worldCenter, oldScreenPos, newScreenPos, projectionParams);
                }
                else
                {
                    // Find how much we moved perpendicular to the line (center, oldScreenPos)
                    screenPixelRotationBuffer += TwoD.PointToLineDistance(screenCenter, oldScreenPos, newScreenPos);
                    angleBuffer += doRotation(worldCenter, oldScreenPos, newScreenPos, projectionParams);

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
                    dS *= doScaling(worldCenter, oldScreenPos, newScreenPos, projectionParams);
                }
                else
                {
                    screenPixelScalingBuffer += (newScreenPos - screenCenter).magnitude -
                                                (oldScreenPos - screenCenter).magnitude;
                    scaleBuffer *= doScaling(worldCenter, oldScreenPos, newScreenPos, projectionParams);

                    if (screenPixelScalingBuffer * screenPixelScalingBuffer >=
                        screenTransformPixelThresholdSquared)
                    {
                        isTransforming = true;
                        dS = scaleBuffer;
                    }
                }
            }

            if (dR != 0 || dS != 1)
            {
                if (State == GestureState.Possible) setState(GestureState.Began);
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        deltaRotation = dR;
                        deltaScale = dS;
                        setState(GestureState.Changed);
                        break;
                }
            }
        }

#if TOUCHSCRIPT_DEBUG
    /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (NumTouches == 0) return;
            drawDebug(activeTouches[0].ProjectionParams.ProjectFrom(cachedTransform.position), activeTouches[0].Position);
        }
#endif

        #endregion

        #region Protected methods

#if TOUCHSCRIPT_DEBUG
        protected override void clearDebug()
        {
            base.clearDebug();

            GLDebug.RemoveFigure(debugID + 3);
        }

        protected override void drawDebug(Vector2 point1, Vector2 point2)
        {
            base.drawDebug(point1, point2);

            GLDebug.DrawPlaneWithNormal(debugID + 3, cachedTransform.position, RotationAxis, 1f, GLDebug.MULTIPLY, float.PositiveInfinity);
        }
#endif

        #endregion

        #region Private functions

        private float doRotation(Vector3 center, Vector2 oldScreenPos, Vector2 newScreenPos,
                                 ProjectionParams projectionParams)
        {
            var newVector = projectionParams.ProjectTo(newScreenPos, TransformPlane) - center;
            var oldVector = projectionParams.ProjectTo(oldScreenPos, TransformPlane) - center;
            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), TransformPlane.normal) < 0)
                angle = -angle;
            return angle;
        }

        private float doScaling(Vector3 center, Vector2 oldScreenPos, Vector2 newScreenPos,
                                ProjectionParams projectionParams)
        {
            var newVector = projectionParams.ProjectTo(newScreenPos, TransformPlane) - center;
            var oldVector = projectionParams.ProjectTo(oldScreenPos, TransformPlane) - center;
            return newVector.magnitude / oldVector.magnitude;
        }

        private void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;

            switch (projection)
            {
                case ProjectionType.Layer:
                    if (projectionLayer == null)
                        transformPlane = new Plane(cachedTransform.TransformDirection(Vector3.forward),
                            cachedTransform.position);
                    else transformPlane = new Plane(projectionLayer.WorldProjectionNormal, cachedTransform.position);
                    break;
                case ProjectionType.Object:
                    transformPlane = new Plane(cachedTransform.TransformDirection(projectionPlaneNormal),
                        cachedTransform.position);
                    break;
                case ProjectionType.Global:
                    transformPlane = new Plane(projectionPlaneNormal, cachedTransform.position);
                    break;
            }
        }

        #endregion
    }
}