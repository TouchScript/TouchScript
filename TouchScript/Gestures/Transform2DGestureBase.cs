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
        #region Unity fields

        /// <summary>
        /// Normal vector of a plane in which transformation occures.
        /// Setting it to <c>Vector3.zero</c> means that gesture works in camera plane.
        /// Other value defines a plane with normal in local or global coordinates.
        /// </summary>
        public Vector3 NormalVector = Vector3.zero;

        /// <summary>
        /// If true, normal vector is specified in local coordinates; otherwise, it is in global coordinates.
        /// </summary>
        public bool IsLocal = true;

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        /// Transformation center in screen coordinates.
        /// </summary>
        public Vector2 ScreenTransformCenter { get; protected set; }

        /// <summary>
        /// Previous center in screen coordinates.
        /// </summary>
        public Vector2 PreviousScreenTransformCenter { get; protected set; }

        /// <summary>
        /// Transformation center in normalized screen coordinates.
        /// </summary>
        public Vector2 NormalizedScreenTransformCenter {
            get { return new Vector2(ScreenTransformCenter.x/Screen.width, ScreenTransformCenter.y/Screen.height); }
        }

        /// <summary>
        /// Previous center in screen coordinates.
        /// </summary>
        public Vector2 PreviousNormalizedScreenTransformCenter {
            get { return new Vector2(PreviousScreenTransformCenter.x/Screen.width, PreviousScreenTransformCenter.y/Screen.height); }
        }

        /// <summary>
        /// Previous global transform center in 3D.
        /// </summary>
        public Vector3 PreviousGlobalTransformCenter { get; protected set; }

        /// <summary>
        /// Global transform center in 3D.
        /// </summary>
        public Vector3 GlobalTransformCenter { get; protected set; }

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
        public Plane GlobalTransformPlane { get; protected set; }

        #endregion

        #region Unity

        protected override void Awake() {
            base.Awake();
            resetGestureProperties();
        }

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches) {
        }

        protected override void touchesMoved(IList<TouchPoint> touches) {
        }

        protected override void touchesEnded(IList<TouchPoint> touches) {
            foreach (var touch in touches) {
                if (ActiveTouches.Count == 0) {
                    switch (State) {
                        case GestureState.Began:
                        case GestureState.Changed:
                            setState(GestureState.Ended);
                            break;
                    }
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches) {
            touchesEnded(touches);
        }

        protected override void reset() {
            resetGestureProperties();
        }

        #endregion

        #region Private functions

        protected virtual Vector3 globalToLocalPosition(Vector3 global) {
            if (transform.parent != null) {
                return transform.parent.worldToLocalMatrix.MultiplyVector(global);
            }
            return global;
        }

        protected virtual Vector3 get3DPosition(Plane plane, Camera camera, Vector2 pos2D) {
            var ray = camera.ScreenPointToRay(pos2D);
            var relativeIntersection = 0f;
            plane.Raycast(ray, out relativeIntersection);
            return ray.origin + ray.direction*relativeIntersection;
        }

        protected virtual void resetGestureProperties() {
            GlobalTransformCenter = Vector3.zero;
            LocalTransformCenter = Vector3.zero;
            GlobalTransformPlane = new Plane();
        }

        #endregion
    }
}