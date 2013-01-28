/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures {
    /// <summary>
    /// Base class for transform gestures.
    /// </summary>
    public abstract class Transform2DGestureBase : Gesture {
        public enum ProjectionType {
            Camera,
            Local,
            Global
        }

        #region Unity fields

        #endregion

        #region Private variables
        [SerializeField] private ProjectionType projection = ProjectionType.Camera;
        [SerializeField] private Vector3 projectionNormal = Vector3.zero;
        protected Camera projectionCamera;

        #endregion

        #region Public properties

        public ProjectionType Projection {
            get { return projection; }
            set {
                projection = value;
                updateProjectionPlane();
            }
        }

        public Vector3 ProjectionNormal {
            get {
                if (projection == ProjectionType.Camera) {
                    return projectionCamera.transform.forward;
                } else {
                    return projectionNormal;
                }
            }
            set {
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

        protected override void Awake() {
            base.Awake();
            updateProjectionPlane();
        }

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches) {}

        protected override void touchesMoved(IList<TouchPoint> touches) {}

        protected override void touchesEnded(IList<TouchPoint> touches) {
            if (ActiveTouches.Count == 0) {
                switch (State) {
                    case GestureState.Began:
                    case GestureState.Changed:
                        setState(GestureState.Ended);
                        break;
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches) {
            touchesEnded(touches);
        }

        protected override void reset() {
            WorldTransformCenter = Vector3.zero;
            LocalTransformCenter = Vector3.zero;
        }

        #endregion

        #region Private functions

        protected virtual Vector3 globalToLocalPosition(Vector3 global) {
            if (transform.parent != null) {
                return transform.parent.worldToLocalMatrix.MultiplyVector(global);
            }
            return global;
        }

        protected void updateProjectionPlane(Camera targetCamera = null) {
            switch (projection) {
                case ProjectionType.Camera:
                    if (targetCamera == null) targetCamera = Camera.mainCamera;
                    if (projectionCamera != targetCamera) {
                        projectionCamera = targetCamera;
                        WorldTransformPlane = new Plane(targetCamera.transform.forward, transform.position);
                    }
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