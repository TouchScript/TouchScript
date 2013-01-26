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
using Random = System.Random;

namespace TouchScript.Clusters {
    /// <summary>
    /// Represents a pool of points separated into two clusters.
    /// </summary>
    public class Cluster2 : Cluster {
        public const int CLUSTER1 = 0;
        public const int CLUSTER2 = 1;

        #region Public properties

        /// <summary>
        /// Minimum distance in cm between clusters to treat them as two separate clusters.
        /// Default: 1cm.
        /// </summary>
        public float MinPointsDistance { get; set; }

        /// <summary>
        /// Indicates that this cluster instance has two valid clusters.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has clusters; otherwise, <c>false</c>.
        /// </value>
        public bool HasClusters {
            get {
                if (Points.Count < 2) return false;
                foreach (var p1 in Points) {
                    foreach (var p2 in Points) {
                        if (p1 == p2) continue;
                        if (Vector2.Distance(p1.Position, p2.Position) > MinPointsDistance) return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Private variables

        private List<TouchPoint> cluster1 = new List<TouchPoint>();
        private List<TouchPoint> cluster2 = new List<TouchPoint>();
        private Random rnd = new Random();

        #endregion

        public Cluster2() : base() {
            MinPointsDistance = 1;
        }

        #region Public methods

        /// <summary>
        /// Gets the center position of one of the clusters.
        /// </summary>
        /// <param name="id">Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>.</param>
        /// <returns>Cluster's centroid position.</returns>
        /// <exception cref="System.InvalidOperationException">If used neither CLUSTER1 or CLUSTER2 as id.</exception>
        public Vector2 GetCenterPosition(int id) {
            if (!HasClusters) throw new InvalidOperationException("Cluster has less than 2 points.");
            if (Dirty) distributePoints();

            Vector2 result;
            switch (id) {
                case CLUSTER1:
                    result = Cluster.GetCenterPosition(cluster1);
                    break;
                case CLUSTER2:
                    result = Cluster.GetCenterPosition(cluster2);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("{0} is not a valid cluster index.", id));
            }
            return result;
        }

        /// <summary>
        /// Gets previous center position of one of the clusters.
        /// </summary>
        /// <param name="id">Cluster id. Either <see cref="CLUSTER1"/> or <see cref="CLUSTER2"/>.</param>
        /// <returns>Cluster's centroid previous position.</returns>
        /// <exception cref="System.InvalidOperationException">If used neither CLUSTER1 or CLUSTER2 as id.</exception>
        public Vector2 GetPreviousCenterPosition(int id) {
            if (!HasClusters) throw new InvalidOperationException("Cluster has less than 2 points.");
            if (Dirty) distributePoints();

            Vector2 result;
            switch (id) {
                case CLUSTER1:
                    result = Cluster.GetPreviousCenterPosition(cluster1);
                    break;
                case CLUSTER2:
                    result = Cluster.GetPreviousCenterPosition(cluster2);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("{0} is not a valid cluster index.", id));
            }
            return result;
        }

        #endregion

        #region Private functions

        private void distributePoints() {
            cluster1.Clear();
            cluster2.Clear();
            cluster1.Add(Points[0]);
            cluster2.Add(Points[1]);

            var total = Points.Count;
            if (total == 2) return;

            var oldHash1 = "";
            var oldHash2 = "";
            var hash1 = "#";
            var hash2 = "#";

            while (oldHash1 != hash1 || oldHash2 != hash2) {
                var center1 = Cluster.GetCenterPosition(cluster1);
                var center2 = Cluster.GetCenterPosition(cluster2);
                TouchPoint obj1 = null;
                TouchPoint obj2 = null;

                // Take most distant points from cluster1 and cluster2
                var maxDist1 = -float.MaxValue;
                var maxDist2 = -float.MaxValue;
                for (var i = 0; i < total; i++) {
                    var obj = Points[i];
                    var dist = (center1 - obj.Position).sqrMagnitude;
                    if (dist > maxDist2) {
                        maxDist2 = dist;
                        obj2 = obj;
                    }

                    dist = (center2 - obj.Position).sqrMagnitude;
                    if (dist > maxDist1) {
                        maxDist1 = dist;
                        obj1 = obj;
                    }
                }

                // If it is the same point it means that this point is too far away from both clusters and has to be in a separate cluster
                if (obj1 == obj2) {
                    center1 = (center1 + center2)*.5f;
                    center2 = obj2.Position;
                } else {
                    center1 = obj1.Position;
                    center2 = obj2.Position;
                }

                cluster1.Clear();
                cluster2.Clear();

                for (var i = 0; i < total; i++) {
                    var obj = Points[i];
                    if ((center1 - obj.Position).sqrMagnitude < (center2 - obj.Position).sqrMagnitude) {
                        cluster1.Add(obj);
                    } else {
                        cluster2.Add(obj);
                    }
                }

                oldHash1 = hash1;
                oldHash2 = hash2;
                hash1 = getHash(cluster1);
                hash2 = getHash(cluster2);
            }

            markClean();
        }

        #endregion
    }
}