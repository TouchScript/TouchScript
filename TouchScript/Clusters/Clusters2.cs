/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Clusters
{
    /// <summary>
    /// Represents a pool of points separated into two clusters.
    /// </summary>
    public class Clusters2 : Cluster
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
        /// Minimum distance in pixels between clusters to treat them as two separate clusters.
        /// Default: 0.
        /// </summary>
        public float MinPointsDistance
        {
            get { return minPointDistance; }
            set
            {
                minPointDistance = value;
                minPointDistanceSqr = value*value;
            }
        }

        /// <summary>
        /// Indicates that this cluster instance has two valid clusters.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has clusters; otherwise, <c>false</c>.
        /// </value>
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

        private List<TouchPoint> cluster1 = new List<TouchPoint>();
        private List<TouchPoint> cluster2 = new List<TouchPoint>();
        private float minPointDistance, minPointDistanceSqr;
        private bool hasClusters = false;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Clusters2"/> class.
        /// </summary>
        public Clusters2() : base()
        {
            MinPointsDistance = 0;
        }

        #region Public methods

        /// <summary>
        /// Calculates the center position of one of the clusters.
        /// </summary>
        /// <param name="id">Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>.</param>
        /// <returns>Cluster's centroid position or <see cref="TouchPoint.InvalidPosition"/> if cluster contains no points.</returns>
        public Vector2 GetCenterPosition(int id)
        {
            if (!HasClusters) return TouchPoint.InvalidPosition;

            Vector2 result;
            switch (id)
            {
                case CLUSTER1:
                    result = Get2DCenterPosition(cluster1);
                    break;
                case CLUSTER2:
                    result = Get2DCenterPosition(cluster2);
                    break;
                default:
                    return TouchPoint.InvalidPosition;
            }
            return result;
        }

        /// <summary>
        /// Calculates previous center position of one of the clusters.
        /// </summary>
        /// <param name="id">Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>.</param>
        /// <returns>Cluster's centroid previous position or <see cref="TouchPoint.InvalidPosition"/> if cluster contains no points.</returns>
        public Vector2 GetPreviousCenterPosition(int id)
        {
            if (!HasClusters) return TouchPoint.InvalidPosition;

            Vector2 result;
            switch (id)
            {
                case CLUSTER1:
                    result = GetPrevious2DCenterPosition(cluster1);
                    break;
                case CLUSTER2:
                    result = GetPrevious2DCenterPosition(cluster2);
                    break;
                default:
                    return TouchPoint.InvalidPosition;
            }
            return result;
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
                var center1 = Get2DCenterPosition(cluster1);
                var center2 = Get2DCenterPosition(cluster2);
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
                    center1 = (center1 + center2)*.5f;
                    center2 = obj2.Position;
                } else
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
                    } else
                    {
                        cluster2.Add(obj);
                    }
                }

                oldHash1 = hash1;
                oldHash2 = hash2;
                hash1 = GetPointsHash(cluster1);
                hash2 = GetPointsHash(cluster2);
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

        #endregion
    }
}