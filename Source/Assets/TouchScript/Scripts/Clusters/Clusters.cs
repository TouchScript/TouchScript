/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Clusters
{
    /// <summary>
    /// Represents a pool of points separated into two clusters.
    /// </summary>
    public sealed class Clusters
    {
        #region Constants

        /// <summary>
        /// The first cluster.
        /// </summary>
        public const int CLUSTER1 = 0;

        /// <summary>
        /// The second cluster.
        /// </summary>
        public const int CLUSTER2 = 1;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the total number of points in clusters represented by this object.
        /// </summary>
        public int PointsCount
        {
            get { return points.Count; }
        }

        /// <summary>
        /// Gets or sets minimum distance in pixels between clusters to treat them as two separate clusters.
        /// </summary>
        /// <value> Minimum distance in pixels which must be between cluster centers to consider them as separate clusters. </value>
        /// <remarks> This value is used to set the limit of how close cluster can be. Sometimes very close points shouldn't be treated as being in separate clusters. </remarks>
        public float MinPointsDistance
        {
            get { return minPointDistance; }
            set
            {
                minPointDistance = value;
                minPointDistanceSqr = value * value;
            }
        }

        /// <summary>
        /// Indicates that this cluster instance has two valid clusters.
        /// </summary>
        /// <value> <c>true</c> if this instance has clusters; otherwise, <c>false</c>. </value>
        public bool HasClusters
        {
            get
            {
                if (dirty) distributePoints();
                return hasClusters;
            }
        }

        #endregion

        #region Private variables

        private List<TouchPoint> points = new List<TouchPoint>();
        private bool dirty;
        private List<TouchPoint> cluster1 = new List<TouchPoint>();
        private List<TouchPoint> cluster2 = new List<TouchPoint>();
        private float minPointDistance, minPointDistanceSqr;
        private bool hasClusters = false;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Clusters"/> class.
        /// </summary>
        public Clusters()
        {
            MinPointsDistance = 0;
            markDirty();
        }

        #region Public methods

        /// <summary>
        /// Calculates the center position of one of the clusters.
        /// </summary>
        /// <param name="id"> Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>. </param>
        /// <returns> Cluster's centroid position or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points. </returns>
        public Vector2 GetCenterPosition(int id)
        {
            if (!HasClusters) return TouchManager.INVALID_POSITION;

            Vector2 result;
            switch (id)
            {
                case CLUSTER1:
                    result = ClusterUtils.Get2DCenterPosition(cluster1);
                    break;
                case CLUSTER2:
                    result = ClusterUtils.Get2DCenterPosition(cluster2);
                    break;
                default:
                    return TouchManager.INVALID_POSITION;
            }
            return result;
        }

        /// <summary>
        /// Calculates previous center position of one of the clusters.
        /// </summary>
        /// <param name="id"> Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>. </param>
        /// <returns> Cluster's centroid previous position or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points. </returns>
        public Vector2 GetPreviousCenterPosition(int id)
        {
            if (!HasClusters) return TouchManager.INVALID_POSITION;

            Vector2 result;
            switch (id)
            {
                case CLUSTER1:
                    result = ClusterUtils.GetPrevious2DCenterPosition(cluster1);
                    break;
                case CLUSTER2:
                    result = ClusterUtils.GetPrevious2DCenterPosition(cluster2);
                    break;
                default:
                    return TouchManager.INVALID_POSITION;
            }
            return result;
        }

        /// <summary>
        /// Adds a point to cluster. </summary>
        /// <param name="point"> A point. </param>
        public void AddPoint(TouchPoint point)
        {
            if (points.Contains(point)) return;

            points.Add(point);
            markDirty();
        }

        /// <summary>
        /// Adds a list of points to cluster.
        /// </summary>
        /// <param name="points"> List of points. </param>
        public void AddPoints(IList<TouchPoint> points)
        {
            var count = points.Count;
            for (var i = 0; i < count; i++) AddPoint(points[i]);
        }

        /// <summary>
        /// Removes a point from cluster.
        /// </summary>
        /// <param name="point"> A point. </param>
        public void RemovePoint(TouchPoint point)
        {
            if (!points.Contains(point)) return;

            points.Remove(point);
            markDirty();
        }

        /// <summary>
        /// Removes a list of points from cluster.
        /// </summary>
        /// <param name="points"> List of points. </param>
        public void RemovePoints(IList<TouchPoint> points)
        {
            var count = points.Count;
            for (var i = 0; i < count; i++) RemovePoint(points[i]);
        }

        /// <summary>
        /// Removes all points from cluster.
        /// </summary>
        public void RemoveAllPoints()
        {
            points.Clear();
            markDirty();
        }

        /// <summary>
        /// Invalidates cluster state. Call this method to recalculate cluster properties.
        /// </summary>
        public void Invalidate()
        {
            markDirty();
        }

        #endregion

        #region Private functions

        private void distributePoints()
        {
            cluster1.Clear();
            cluster2.Clear();

            hasClusters = checkClusters();
            if (!hasClusters) return;

            cluster1.Add(points[0]);
            cluster2.Add(points[1]);

            var total = points.Count;
            if (total == 2) return;

            var oldHash1 = "";
            var oldHash2 = "";
            var hash1 = "#";
            var hash2 = "#";

            while (oldHash1 != hash1 || oldHash2 != hash2)
            {
                var center1 = ClusterUtils.Get2DCenterPosition(cluster1);
                var center2 = ClusterUtils.Get2DCenterPosition(cluster2);
                TouchPoint obj1 = null;
                TouchPoint obj2 = null;

                // Take most distant points from cluster1 and cluster2
                var maxDist1 = -float.MaxValue;
                var maxDist2 = -float.MaxValue;
                for (var i = 0; i < total; i++)
                {
                    var obj = points[i];
                    var dist = (center1 - obj.Position).sqrMagnitude;
                    if (dist > maxDist2)
                    {
                        maxDist2 = dist;
                        obj2 = obj;
                    }

                    dist = (center2 - obj.Position).sqrMagnitude;
                    if (dist > maxDist1)
                    {
                        maxDist1 = dist;
                        obj1 = obj;
                    }
                }

                // If it is the same point it means that this point is too far away from both clusters and has to be in a separate cluster
                if (obj1 == obj2)
                {
                    center1 = (center1 + center2) * .5f;
                    center2 = obj2.Position;
                }
                else
                {
                    center1 = obj1.Position;
                    center2 = obj2.Position;
                }

                cluster1.Clear();
                cluster2.Clear();

                for (var i = 0; i < total; i++)
                {
                    var obj = points[i];
                    if ((center1 - obj.Position).sqrMagnitude < (center2 - obj.Position).sqrMagnitude)
                    {
                        cluster1.Add(obj);
                    }
                    else
                    {
                        cluster2.Add(obj);
                    }
                }

                oldHash1 = hash1;
                oldHash2 = hash2;
                hash1 = ClusterUtils.GetPointsHash(cluster1);
                hash2 = ClusterUtils.GetPointsHash(cluster2);
            }

            markClean();
        }

        private bool checkClusters()
        {
            var length = points.Count - 1;
            if (length < 1) return false;
            if (length == 1)
            {
                var p1 = points[0].Position;
                var p2 = points[1].Position;
                var dx = p1.x - p2.x;
                var dy = p1.y - p2.y;
                if (dx * dx + dy * dy >= minPointDistanceSqr) return true;
                return false;
            }

            for (var i = 0; i < length; i++)
            {
                for (var j = i + 1; j <= length; j++)
                {
                    var p1 = points[i].Position;
                    var p2 = points[j].Position;
                    var dx = p1.x - p2.x;
                    var dy = p1.y - p2.y;
                    if (dx * dx + dy * dy >= minPointDistanceSqr) return true;
                }
            }
            return false;
        }

        private void markDirty()
        {
            dirty = true;
        }

        private void markClean()
        {
            dirty = false;
        }

        #endregion
    }
}