/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Behaviors;
using TouchScript.Gestures.Abstract;
using TouchScript.Layers;
using TouchScript.Utils;
#if DEBUG
using TouchScript.Utils.Debug;
#endif
using UnityEngine;

namespace TouchScript.Gestures
{
    [AddComponentMenu("TouchScript/Gestures/Transform Gesture")]
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

        [SerializeField] private ProjectionType projection = ProjectionType.Layer;
        [SerializeField] private Vector3 projectionPlaneNormal = Vector3.forward;

        private TouchLayer projectionLayer;
        private Plane transformPlane;

        #endregion

        #region Public methods

        public void ApplyTransform(Transform target)
        {
            if (!Mathf.Approximately(DeltaScale, 1f)) target.localScale *= DeltaScale;
            if (!Mathf.Approximately(DeltaRotation, 0f))
                target.rotation = Quaternion.AngleAxis(DeltaRotation, RotationAxis) * target.rotation;
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
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

            if (touches.Count == NumTouches)
            {
                projectionLayer = activeTouches[0].Layer;
                updateProjectionPlane();
            }
        }

        #endregion

        #region Protected methods

        protected override float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
            Vector2 newScreenPos2)
        {
            var newVector = projectionLayer.ProjectTo(newScreenPos2, TransformPlane) -
                            projectionLayer.ProjectTo(newScreenPos1, TransformPlane);
            var oldVector = projectionLayer.ProjectTo(oldScreenPos2, TransformPlane) -
                            projectionLayer.ProjectTo(oldScreenPos1, TransformPlane);
            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), TransformPlane.normal) < 0)
                angle = -angle;
            return angle;
        }

        protected override float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
            Vector2 newScreenPos2)
        {
            var newVector = projectionLayer.ProjectTo(newScreenPos2, TransformPlane) -
                            projectionLayer.ProjectTo(newScreenPos1, TransformPlane);
            var oldVector = projectionLayer.ProjectTo(oldScreenPos2, TransformPlane) -
                            projectionLayer.ProjectTo(oldScreenPos1, TransformPlane);
            return newVector.magnitude/oldVector.magnitude;
        }

        protected override Vector3 doTranslation(Vector2 oldScreenCenter, Vector2 newScreenCenter)
        {
            if (isTransforming)
            {
                return projectionLayer.ProjectTo(newScreenCenter, TransformPlane) -
                       projectionLayer.ProjectTo(oldScreenCenter, TransformPlane);
            }

            screenPixelTranslationBuffer += newScreenCenter - oldScreenCenter;
            if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
            {
                isTransforming = true;
                return projectionLayer.ProjectTo(newScreenCenter, TransformPlane) -
                       projectionLayer.ProjectTo(newScreenCenter - screenPixelTranslationBuffer, TransformPlane);
            }

            return Vector3.zero;
        }

#if DEBUG
        protected override void drawDebug(int touchPoints)
        {
            base.drawDebug(touchPoints);

            switch (touchPoints)
            {
                case 0:
                    GLDebug.RemoveFigure(debugID + 3);
                    break;
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

            Vector3 center;
            // TODO: revert to transform center?
            if (cachedCollider != null) center = cachedCollider.bounds.center;
            else center = cachedTransform.position;

            switch (projection)
            {
                case ProjectionType.Layer:
                    if (projectionLayer == null)
                        transformPlane = new Plane(cachedTransform.TransformDirection(Vector3.forward), center);
                    else transformPlane = new Plane(projectionLayer.WorldProjectionNormal, center);
                    break;
                case ProjectionType.Object:
                    transformPlane = new Plane(cachedTransform.TransformDirection(projectionPlaneNormal), center);
                    break;
                case ProjectionType.Global:
                    transformPlane = new Plane(projectionPlaneNormal, center);
                    break;
            }
        }

        #endregion
    }
}