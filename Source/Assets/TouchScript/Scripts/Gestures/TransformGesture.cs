/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Gestures.Base;
using TouchScript.Layers;
using TouchScript.Utils;
#if TOUCHSCRIPT_DEBUG
using TouchScript.Utils.Debug;
#endif
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a transform gesture, i.e. translation, rotation, scaling or a combination of these.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Transform Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_TransformGesture.htm")]
    public class TransformGesture : TransformGestureBase, ITransformGesture
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
            Global,
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
            if (!Mathf.Approximately(DeltaScale, 1f)) target.localScale *= DeltaScale;
            if (!Mathf.Approximately(DeltaRotation, 0f)) target.rotation = Quaternion.AngleAxis(DeltaRotation, RotationAxis) * target.rotation;
            if (DeltaPosition != Vector3.zero) target.position += DeltaPosition;
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
            }
        }

        #endregion

        #region Protected methods

        protected Vector3 projectScaledRotated(Vector2 point, float dR, float dS, ProjectionParams projectionParams)
        {
            var delta = projectionParams.ProjectTo(point, transformPlane) - cachedTransform.position;
            if (dR != 0) delta = Quaternion.AngleAxis(dR, RotationAxis) * delta;
            if (dS != 0) delta = delta * dS;
            return cachedTransform.position + delta;
        }

        /// <inheritdoc />
        protected override float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                            Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            var newVector = projectionParams.ProjectTo(newScreenPos2, TransformPlane) -
                            projectionParams.ProjectTo(newScreenPos1, TransformPlane);
            var oldVector = projectionParams.ProjectTo(oldScreenPos2, TransformPlane) -
                            projectionParams.ProjectTo(oldScreenPos1, TransformPlane);
            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), TransformPlane.normal) < 0)
                angle = -angle;
            return angle;
        }

        /// <inheritdoc />
        protected override float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                           Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            var newVector = projectionParams.ProjectTo(newScreenPos2, TransformPlane) -
                            projectionParams.ProjectTo(newScreenPos1, TransformPlane);
            var oldVector = projectionParams.ProjectTo(oldScreenPos2, TransformPlane) -
                            projectionParams.ProjectTo(oldScreenPos1, TransformPlane);
            return newVector.magnitude / oldVector.magnitude;
        }

        /// <inheritdoc />
        protected override Vector3 doOnePointTranslation(Vector2 oldScreenPos, Vector2 newScreenPos,
                                                         ProjectionParams projectionParams)
        {
            if (isTransforming)
            {
                return projectionParams.ProjectTo(newScreenPos, TransformPlane) -
                       projectionParams.ProjectTo(oldScreenPos, TransformPlane);
            }

            screenPixelTranslationBuffer += newScreenPos - oldScreenPos;
            if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
            {
                isTransforming = true;
                return projectionParams.ProjectTo(newScreenPos, TransformPlane) -
                       projectionParams.ProjectTo(newScreenPos - screenPixelTranslationBuffer, TransformPlane);
            }

            return Vector3.zero;
        }

        /// <inheritdoc />
        protected override Vector3 doTwoPointTranslation(Vector2 oldScreenPos1, Vector2 oldScreenPos2,
                                                         Vector2 newScreenPos1, Vector2 newScreenPos2, float dR, float dS, ProjectionParams projectionParams)
        {
            if (isTransforming)
            {
                return projectionParams.ProjectTo(newScreenPos1, TransformPlane) - projectScaledRotated(oldScreenPos1, dR, dS, projectionParams);
            }

            screenPixelTranslationBuffer += newScreenPos1 - oldScreenPos1;
            if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
            {
                isTransforming = true;
                return projectionParams.ProjectTo(newScreenPos1, TransformPlane) -
                       projectScaledRotated(newScreenPos1 - screenPixelTranslationBuffer, dR, dS, projectionParams);
            }

            return Vector3.zero;
        }

#if TOUCHSCRIPT_DEBUG
        protected override void clearDebug()
        {
            base.clearDebug();

            GLDebug.RemoveFigure(debugID + 3);
        }

        protected override void drawDebug(int touchPoints)
        {
            base.drawDebug(touchPoints);

            if (!DebugMode) return;
            switch (touchPoints)
            {
                case 1:
                    if (projection == ProjectionType.Global || projection == ProjectionType.Object)
                    {
                        GLDebug.DrawPlaneWithNormal(debugID + 3, cachedTransform.position, RotationAxis, 4f, GLDebug.MULTIPLY, float.PositiveInfinity);
                    }
                    break;
                default:
                    if (projection == ProjectionType.Global || projection == ProjectionType.Object)
                    {
                        GLDebug.DrawPlaneWithNormal(debugID + 3, cachedTransform.position, RotationAxis, 4f, GLDebug.MULTIPLY, float.PositiveInfinity);
                    }
                    break;
            }
        }
#endif

        #endregion

        #region Private functions

        /// <summary>
        /// Updates projection plane based on options set.
        /// </summary>
        private void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;

            switch (projection)
            {
                case ProjectionType.Layer:
                    if (projectionLayer == null)
                        transformPlane = new Plane(cachedTransform.TransformDirection(Vector3.forward), cachedTransform.position);
                    else transformPlane = new Plane(projectionLayer.WorldProjectionNormal, cachedTransform.position);
                    break;
                case ProjectionType.Object:
                    transformPlane = new Plane(cachedTransform.TransformDirection(projectionPlaneNormal), cachedTransform.position);
                    break;
                case ProjectionType.Global:
                    transformPlane = new Plane(projectionPlaneNormal, cachedTransform.position);
                    break;
            }
        }

        #endregion
    }
}