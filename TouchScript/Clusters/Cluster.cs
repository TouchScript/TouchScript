/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Clusters {
    /// <summary>
    /// 
    /// </summary>
    public abstract class Cluster {
        /// <summary>
        /// What happens when you add or remove a point from cluster:
        /// <list type="bullet">
        ///     <item>Nothing — Nothing happened. Point hasn't been added.</item>
        ///     <item>PointAdded — Point successfully added.</item>
        ///     <item>FirstPointAdded — That was the first point added to cluster.</item>
        ///     <item>PointRemoved — Point successfully removed.</item>
        ///     <item>LastPointRemoved — That was the last point removed from cluster.</item>
        /// </list>
        /// </summary>
        public enum OperationResult {
            Nothing,
            WrongCamera,
            PointAdded,
            FirstPointAdded,
            PointRemoved,
            LastPointRemoved
        }

        #region Public properties

        public Camera Camera { get; private set; }

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

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public OperationResult AddPoint(TouchPoint point) {
            if (Points.Contains(point)) return OperationResult.Nothing;

            Points.Add(point);
            markDirty();

            if (PointsCount == 1) {
                Camera = point.HitCamera;
                return OperationResult.FirstPointAdded;
            } else if (point.HitCamera != Camera) {
                return OperationResult.WrongCamera;
            }
            return OperationResult.PointAdded;
        }

        /// <summary>
        /// Removes the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public OperationResult RemovePoint(TouchPoint point) {
            if (!Points.Contains(point)) return OperationResult.Nothing;

            Points.Remove(point);
            markDirty();

            if (PointsCount == 0) return OperationResult.LastPointRemoved;
            return OperationResult.PointRemoved;
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

        protected Vector2 getCenterPosition(List<TouchPoint> touches) {
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

        protected Vector2 getPreviousCenterPosition(List<TouchPoint> touches) {
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