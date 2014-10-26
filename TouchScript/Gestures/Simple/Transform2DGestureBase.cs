/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Base class for transform gestures.
    /// </summary>
    public abstract class Transform2DGestureBase : Gesture
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
            Local,

            /// <summary>
            /// Use a plane with certain normal vector in global coordinates.
            /// </summary>
            Global
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
                switch (projection)
                {
                    case ProjectionType.Layer:
                        if (value == ProjectionType.Local)
                        {
                            localProjectionNormal = cachedTransform.InverseTransformDirection(projectionLayer.WorldProjectionNormal);
                        }
                        else
                        {
                            projectionNormal = projectionLayer.WorldProjectionNormal;
                        }
                        break;
                    case ProjectionType.Local:
                        if (value == ProjectionType.Global) projectionNormal = cachedTransform.TransformDirection(localProjectionNormal);
                        break;
                    case ProjectionType.Global:
                        if (value == ProjectionType.Local) localProjectionNormal = cachedTransform.InverseTransformDirection(projectionNormal);
                        break;
                }
                projection = value;
            }
        }

        /// <summary>
        /// Transform's projection plane normal.
        /// </summary>
        public Vector3 WorldProjectionNormal
        {
            get
            {
                switch (projection)
                {
                    case ProjectionType.Layer:
                        return projectionLayer.WorldProjectionNormal;
                    case ProjectionType.Local:
                        return cachedTransform.TransformDirection(localProjectionNormal);
                    default:
                        return projectionNormal;
                }
            }
            set
            {
                if (projection == ProjectionType.Layer) projection = ProjectionType.Local;
                value.Normalize();
                if (projection == ProjectionType.Local) localProjectionNormal = cachedTransform.InverseTransformDirection(value);
                else projectionNormal = value;
            }
        }

        /// <summary>
        /// Previous global transform center in 3D.
        /// </summary>
        public Vector3 PreviousWorldTransformCenter { get; protected set; }

        /// <summary>
        /// Global transform center in 3D.
        /// </summary>
        public Vector3 WorldTransformCenter { get; protected set; }

        #endregion

        #region Private variables

        [SerializeField]
        private ProjectionType projection = ProjectionType.Layer;

        // world projection normal
        // don't rename! or many people lose data
        [SerializeField]
        private Vector3 projectionNormal = Vector3.forward;

        // Need both in case object is turned by another script or user.
        [SerializeField]
        private Vector3 localProjectionNormal = Vector3.forward;

        private Collider cachedCollider;

        /// <summary>
        /// Touch layer used in projection.
        /// </summary>
        protected TouchLayer projectionLayer;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            cachedCollider = collider;
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            if (touches.Count == activeTouches.Count)
            {
                projectionLayer = activeTouches[0].Layer;
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouch> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
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
        protected override void reset()
        {
            base.reset();

            WorldTransformCenter = TouchManager.INVALID_2D_POSITION;
            PreviousWorldTransformCenter = TouchManager.INVALID_2D_POSITION;
        }

        #endregion

        #region Protected functions

        protected Vector3 projectTo(Vector2 screenPosition)
        {
            switch (projection)
            {
                case ProjectionType.Layer:
                    return projectionLayer.ProjectTo(screenPosition);
                case ProjectionType.Local:
                    return projectionLayer.ProjectTo(screenPosition, getCenter(), cachedTransform.TransformDirection(localProjectionNormal));
                default:
                    return projectionLayer.ProjectTo(screenPosition, getCenter(), projectionNormal);
            }
        }

        protected Vector3 getCenter()
        {
            if (cachedCollider != null) return cachedCollider.bounds.center;
            return cachedTransform.position;
        }

        #endregion
    }
}