/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Clusters
{
    /// <summary>
    /// Abstract base for points separated into clusters
    /// </summary>
    public abstract class Cluster
    {
        #region Public properties

        /// <summary>
        /// Number of total points in clusters represented by this object.
        /// </summary>
        /// <value>
        /// Number of points.
        /// </value>
        public int PointsCount
        {
            get { return points.Count; }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// List of points in clusters.
        /// </summary>
        protected List<TouchPoint> points = new List<TouchPoint>();
        /// <summary>
        /// Indicates if clusters must be rebuilt.
        /// </summary>
        protected bool dirty { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class.
        /// </summary>
        protected Cluster()
        {
            markDirty();
        }

        #region Static methods

        /// <summary>
        /// Returns camera instance touch points belong to.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Camera instance.</returns>
        public static Camera GetClusterCamera(IList<TouchPoint> touches)
        {
            if (touches.Count == 0) return Camera.mainCamera;
            var cam = touches[0].Layer.Camera;
            if (cam == null) return Camera.mainCamera;
            return cam;
        }

        /// <summary>
        /// Returns centroid of current positions for given touch points.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Vector2</returns>
        /// <exception cref="System.InvalidOperationException">No points in cluster.</exception>
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

        /// <summary>
        /// Returns centroid of previous positions for given touch points.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Vector2</returns>
        /// <exception cref="System.InvalidOperationException">No points in cluster.</exception>
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

        /// <summary>
        /// Returns unique hash for a list of points
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>String</returns>
        public static String GetPointsHash(List<TouchPoint> touches)
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
        /// Adds a point.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <returns></returns>
        public void AddPoint(TouchPoint point)
        {
            if (points.Contains(point)) return;

            points.Add(point);
            markDirty();
        }

        /// <summary>
        /// Adds a list of points.
        /// </summary>
        /// <param name="points">List of points.</param>
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
            if (!points.Contains(point)) return;

            points.Remove(point);
            markDirty();
        }

        /// <summary>
        /// Removes a list of points.
        /// </summary>
        /// <param name="points">List of points.</param>
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
            points.Clear();
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

        /// <summary>
        /// Signals that clusters changed and must be rebuilt.
        /// </summary>
        protected void markDirty()
        {
            dirty = true;
        }

        /// <summary>
        /// Signals that clusters have just been rebuilt.
        /// </summary>
        protected void markClean()
        {
            dirty = false;
        }

        #endregion
    }
}