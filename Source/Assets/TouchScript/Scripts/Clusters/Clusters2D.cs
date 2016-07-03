/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Clusters
{
    /// <summary>
    /// Represents a pool of points separated into two clusters.
    /// </summary>
    public sealed class Clusters2D
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

        private List<Pointer> points = new List<Pointer>();
        private bool dirty;
        private List<Pointer> cluster1 = new List<Pointer>();
        private List<Pointer> cluster2 = new List<Pointer>();
        private float minPointDistance, minPointDistanceSqr;
        private bool hasClusters = false;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Clusters2D"/> class.
        /// </summary>
        public Clusters2D()
        {
            MinPointsDistance = 0;
            markDirty();
        }

        #region Public methods

        /// <summary>
        /// Calculates the center position of one of the clusters.
        /// </summary>
        /// <param name="id"> Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>. </param>
        /// <returns> Cluster's centroid position or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no pointers. </returns>
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
        /// <returns> Cluster's centroid previous position or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no pointers. </returns>
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
        /// Adds a pointer to cluster. </summary>
        /// <param name="pointer"> A pointer. </param>
        public void AddPoint(Pointer pointer)
        {
            if (points.Contains(pointer)) return;

            points.Add(pointer);
            markDirty();
        }

        /// <summary>
        /// Adds a list of pointers to cluster.
        /// </summary>
        /// <param name="pointers"> List of pointers. </param>
        public void AddPoints(IList<Pointer> pointers)
        {
            var count = pointers.Count;
            for (var i = 0; i < count; i++) AddPoint(pointers[i]);
        }

        /// <summary>
        /// Removes a pointer from cluster.
        /// </summary>
        /// <param name="pointer"> A pointer. </param>
        public void RemovePoint(Pointer pointer)
        {
            if (!points.Contains(pointer)) return;

            points.Remove(pointer);
            markDirty();
        }

        /// <summary>
        /// Removes a list of pointers from cluster.
        /// </summary>
        /// <param name="points"> List of pointers. </param>
        public void RemovePoints(IList<Pointer> points)
        {
            var count = points.Count;
            for (var i = 0; i < count; i++) RemovePoint(points[i]);
        }

        /// <summary>
        /// Removes all pointers from cluster.
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
                Pointer obj1 = null;
                Pointer obj2 = null;

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