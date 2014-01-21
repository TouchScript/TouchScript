/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Base class for transform gestures.
    /// </summary>
    public abstract class Transform2DGestureBase : Gesture
    {
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
        /// Previous local transform center in 3D.
        /// </summary>
        public Vector3 PreviousLocalTransformCenter { get; protected set; }

        /// <summary>
        /// Local transform center in 3D.
        /// </summary>
        public Vector3 LocalTransformCenter { get; protected set; }

        /// <summary>
        /// Plane where transformation occured.
        /// </summary>
        public Plane WorldTransformPlane { get; private set; }

        #endregion

        #region Private variables

        [SerializeField]
        private ProjectionType projection = ProjectionType.Camera;

        [SerializeField]
        private Vector3 projectionNormal = Vector3.forward;

        /// <summary>
        /// Camera which is used to project touch points from screen space to a 3d plane.
        /// </summary>
        protected Camera projectionCamera;

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            updateProjectionCamera();
            updateProjectionPlane();
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            updateProjectionCamera();
            updateProjectionPlane();
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
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
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            base.touchesCancelled(touches);

            touchesEnded(touches);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            WorldTransformCenter = InvalidPosition;
            PreviousWorldTransformCenter = InvalidPosition;
            LocalTransformCenter = InvalidPosition;
            PreviousLocalTransformCenter = InvalidPosition;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Converts a vector from global space to object's local space.
        /// </summary>
        /// <param name="global">Global vector to convert.</param>
        /// <returns>Vector in local space.</returns>
        protected virtual Vector3 globalToLocalPosition(Vector3 global)
        {
            if (transform.parent != null)
            {
                return transform.parent.InverseTransformPoint(global);
            }
            return global;
        }

        /// <summary>
        /// Updates projection camera.
        /// </summary>
        protected void updateProjectionCamera()
        {
            if (activeTouches.Count == 0) projectionCamera = Camera.main;
            else projectionCamera = Cluster.GetClusterCamera(activeTouches);
        }

        /// <summary>
        /// Updates projection plane based on options set.
        /// </summary>
        protected void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;

            Vector3 center;
            if (collider != null) center = collider.bounds.center;
            else center = transform.position;

            switch (projection)
            {
                case ProjectionType.Camera:
                    WorldTransformPlane = new Plane(projectionCamera.transform.forward, center);
                    break;
                case ProjectionType.Local:
                    WorldTransformPlane = new Plane(transform.TransformDirection(projectionNormal).normalized, center);
                    break;
                case ProjectionType.Global:
                    WorldTransformPlane = new Plane(projectionNormal.normalized, center);
                    break;
            }
        }

        #endregion
    }
}