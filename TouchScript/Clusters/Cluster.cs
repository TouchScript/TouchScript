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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Clusters {
    /// <summary>
    /// 
    /// </summary>
    public abstract class Cluster {
        #region Public properties

        public int PointsCount {
            get { return Points.Count; }
        }

        #endregion

        #region Private variables

        protected List<TouchPoint> Points = new List<TouchPoint>();
        protected bool dirty { get; private set; }

        #endregion

        protected Cluster() {
            markDirty();
        }

        #region Static methods

        public static Camera GetClusterCamera(IList<TouchPoint> touches) {
            if (touches.Count == 0) return Camera.mainCamera;
            return touches[0].HitCamera;
        }

        public static Vector2 Get2DCenterPosition(IList<TouchPoint> touches) {
            var length = touches.Count;
            if (length == 0) throw new InvalidOperationException("No points in cluster.");
            if (length == 1) return touches[0].Position;

            var position = new Vector2();
            foreach (var point in touches) {
                position += point.Position;
            }
            return position/(float) length;
        }

        public static Vector2 GetPrevious2DCenterPosition(IList<TouchPoint> touches) {
            var length = touches.Count;
            if (length == 0) throw new InvalidOperationException("No points in cluster.");
            if (length == 1) return touches[0].PreviousPosition;

            var position = new Vector2();
            foreach (var point in touches) {
                position += point.PreviousPosition;
            }
            return position/(float) length;
        }

        public static String getHash(List<TouchPoint> touches) {
            var result = "";
            foreach (var touchPoint in touches) {
                result += "#" + touchPoint.Id;
            }
            return result;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public void AddPoint(TouchPoint point) {
            if (Points.Contains(point)) return;

            Points.Add(point);
            markDirty();
        }

        public void AddPoints(IList<TouchPoint> points) {
            foreach (var point in points) {
                AddPoint(point);
            }
        }

        /// <summary>
        /// Removes the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public void RemovePoint(TouchPoint point) {
            if (!Points.Contains(point)) return;

            Points.Remove(point);
            markDirty();
        }

        /// <summary>
        /// Removes all points.
        /// </summary>
        public void RemoveAllPoints() {
            Points.Clear();
            markDirty();
        }

        /// <summary>
        /// Invalidates cluster state.
        /// Call this method to recalculate cluster properties.
        /// </summary>
        public void Invalidate() {
            markDirty();
        }

        #endregion

        #region Protected functions

        protected void markDirty() {
            dirty = true;
        }

        protected void markClean() {
            dirty = false;
        }

        #endregion

    }
}