/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Base class for transform gestures.
    /// </summary>
    public abstract class Transform2DGestureBase : Gesture
    {
        public enum ProjectionType
        {
            Camera,
            Local,
            Global
        }

        #region Unity fields

        #endregion

        #region Private variables

        [SerializeField]
        private ProjectionType projection = ProjectionType.Camera;

        [SerializeField]
        private Vector3 projectionNormal = Vector3.zero;

        protected Camera projectionCamera;

        #endregion

        #region Public properties

        public ProjectionType Projection
        {
            get { return projection; }
            set
            {
                if (projection == value) return;
                projection = value;
                updateProjectionPlane();
            }
        }

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
                updateProjectionPlane();
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

        #region Unity

        protected override void Awake()
        {
            base.Awake();
            updateProjectionCamera(Camera.mainCamera);
            updateProjectionPlane();
        }

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches)
        {}

        protected override void touchesMoved(IList<TouchPoint> touches)
        {}

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == 0)
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

        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            touchesEnded(touches);
        }

        protected override void reset()
        {
            WorldTransformCenter = Vector3.zero;
            LocalTransformCenter = Vector3.zero;
        }

        #endregion

        #region Private functions

        protected virtual Vector3 globalToLocalPosition(Vector3 global)
        {
            if (transform.parent != null)
            {
                return transform.parent.worldToLocalMatrix.MultiplyVector(global);
            }
            return global;
        }

        protected void updateProjectionCamera(Camera targetCamera)
        {
            var changed = targetCamera != projectionCamera;
            projectionCamera = targetCamera;
            if (changed) updateProjectionPlane();
        }

        protected void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;
            switch (projection)
            {
                case ProjectionType.Camera:
                    WorldTransformPlane = new Plane(projectionCamera.transform.forward, transform.position);
                    break;
                case ProjectionType.Local:
                    WorldTransformPlane = new Plane(transform.localToWorldMatrix.MultiplyVector(projectionNormal).normalized, transform.position);
                    break;
                case ProjectionType.Global:
                    WorldTransformPlane = new Plane(projectionNormal.normalized, transform.position);
                    break;
            }
        }

        #endregion
    }
}