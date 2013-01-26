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
        protected bool Dirty { get; private set; }

        #endregion

        protected Cluster() {
            markDirty();
        }

        #region Public methods
		
		public static Vector2 GetCenterPosition(IList<TouchPoint> touches) {
            var length = touches.Count;
            if (length == 0) throw new InvalidOperationException("No points in cluster.");
            if (length == 1) return touches[0].Position;

            var x = 0f;
            var y = 0f;
            foreach (var point in touches) {
                x += point.Position.x;
                y += point.Position.y;
            }
            return new Vector2(x/(float) length, y/(float) length);
        }

        public static Vector2 GetPreviousCenterPosition(IList<TouchPoint> touches) {
            var length = touches.Count;
            if (length == 0) throw new InvalidOperationException("No points in cluster.");
            if (length == 1) return touches[0].PreviousPosition;

            var x = 0f;
            var y = 0f;
            foreach (var point in touches) {
                x += point.PreviousPosition.x;
                y += point.PreviousPosition.y;
            }
            return new Vector2(x/(float) length, y/(float) length);
        }


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
        /// Invalidates cluster state.
        /// Call this method to recalculate cluster properties.
        /// </summary>
        public void Invalidate() {
            markDirty();
        }

        /// <summary>
        /// Removes all points.
        /// </summary>
        public void RemoveAllPoints() {
            Points.Clear();
            markDirty();
        }

        #endregion

        #region Protected functions

        protected void markDirty() {
            Dirty = true;
        }

        protected void markClean() {
            Dirty = false;
        }

        protected String getHash(List<TouchPoint> touches) {
            var result = "";
            foreach (var touchPoint in touches) {
                result += "#" + touchPoint.Id;
            }
            return result;
        }

        #endregion
    }
}