/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Clusters
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Cluster
    {
        #region Public properties

        public int PointsCount
        {
            get { return Points.Count; }
        }

        #endregion

        #region Private variables

        protected List<TouchPoint> Points = new List<TouchPoint>();
        protected bool dirty { get; private set; }

        #endregion

        protected Cluster()
        {
            markDirty();
        }

        #region Static methods

        public static Camera GetClusterCamera(IList<TouchPoint> touches)
        {
            if (touches.Count == 0) return Camera.mainCamera;
            var cam = touches[0].Layer.Camera;
            if (cam == null) return Camera.mainCamera;
            return cam;
        }

        public static Vector2 Get2DCenterPosition(IList<TouchPoint> touches)
        {
            var length = touches.Count;
            if (length == 0) throw new InvalidOperationException("No points in cluster.");
            if (length == 1) return touches[0].Position;

            var position = new Vector2();
            foreach (var point in touches)
            {
                position += point.Position;
            }
            return position/(float)length;
        }

        public static Vector2 GetPrevious2DCenterPosition(IList<TouchPoint> touches)
        {
            var length = touches.Count;
            if (length == 0) throw new InvalidOperationException("No points in cluster.");
            if (length == 1) return touches[0].PreviousPosition;

            var position = new Vector2();
            foreach (var point in touches)
            {
                position += point.PreviousPosition;
            }
            return position/(float)length;
        }

        public static String getHash(List<TouchPoint> touches)
        {
            var result = "";
            foreach (var touchPoint in touches)
            {
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
        public void AddPoint(TouchPoint point)
        {
            if (Points.Contains(point)) return;

            Points.Add(point);
            markDirty();
        }

        public void AddPoints(IList<TouchPoint> points)
        {
            foreach (var point in points)
            {
                AddPoint(point);
            }
        }

        /// <summary>
        /// Removes the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public void RemovePoint(TouchPoint point)
        {
            if (!Points.Contains(point)) return;

            Points.Remove(point);
            markDirty();
        }

        public void RemovePoints(IList<TouchPoint> points)
        {
            foreach (var point in points)
            {
                RemovePoint(point);
            }
        }

        /// <summary>
        /// Removes all points.
        /// </summary>
        public void RemoveAllPoints()
        {
            Points.Clear();
            markDirty();
        }

        /// <summary>
        /// Invalidates cluster state.
        /// Call this method to recalculate cluster properties.
        /// </summary>
        public void Invalidate()
        {
            markDirty();
        }

        #endregion

        #region Protected functions

        protected void markDirty()
        {
            dirty = true;
        }

        protected void markClean()
        {
            dirty = false;
        }

        #endregion
    }
}