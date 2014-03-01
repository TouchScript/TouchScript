/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
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
            /// Use a plane parallel to camera viewport.
            /// </summary>
            Camera,

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
                if (projection == ProjectionType.Camera)
                {
                    return projectionCamera.transform.forward;
                } else
                {
                    return projectionNormal;
                }
            }
            set
            {
                if (projection == ProjectionType.Camera) projection = ProjectionType.Local;
                value.Normalize();
                if (projectionNormal == value) return;
                projectionNormal = value;
                if (Application.isPlaying) updateProjectionPlane();
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

        /// <summary>
        /// Plane where transformation occured.
        /// </summary>
        public Plane WorldTransformPlane
        {
            get { return new Plane(worldTransformPlane.normal, worldTransformPlane.distance); }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private ProjectionType projection = ProjectionType.Camera;

        [SerializeField]
        private Vector3 projectionNormal = Vector3.forward;

        private Plane worldTransformPlane;

        private Collider cachedCollider;

        /// <summary>
        /// Camera which is used to project touch points from screen space to a 3d plane.
        /// </summary>
        protected Camera projectionCamera;

        #endregion

        #region Unity methods

        protected override void Awake()
        {
            base.Awake();
            worldTransformPlane = new Plane();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            cachedCollider = collider;
            projectionCamera = Camera.main;
            updateProjectionPlane();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            if (touches.Count == activeTouches.Count)
            {
                projectionCamera = activeTouches[0].Layer.Camera;
                updateProjectionPlane();
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            updateProjectionPlane();
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

            WorldTransformCenter = TouchManager.INVALID_POSITION;
            PreviousWorldTransformCenter = TouchManager.INVALID_POSITION;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Updates projection plane based on options set.
        /// </summary>
        protected void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;

            Vector3 center;
            if (cachedCollider != null) center = cachedCollider.bounds.center;
            else center = transform.position;

            switch (projection)
            {
                case ProjectionType.Camera:
                    worldTransformPlane = new Plane(projectionCamera.transform.forward, center);
                    break;
                case ProjectionType.Local:
                    worldTransformPlane = new Plane(transform.TransformDirection(projectionNormal), center);
                    break;
                case ProjectionType.Global:
                    worldTransformPlane = new Plane(projectionNormal, center);
                    break;
            }
        }

        #endregion
    }
}