/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;

namespace TouchScript.Clusters {
    /// <summary>
    /// Cluster of all points.
    /// Basically a centroid of points.
    /// </summary>
    public class Cluster1 : Cluster {
        #region Public methods

        /// <summary>
        /// Indicates that this cluster instance represents a valid cluster.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance represents a valid cluster; otherwise, <c>false</c>.
        /// </value>
        public bool HasCluster {
            get { return Points.Count > 0; }
        }

        /// <summary>
        /// Gets the center position.
        /// </summary>
        /// <returns>Centroid of all points.</returns>
        public Vector2 GetCenterPosition() {
            return getCenterPosition(Points);
        }

        /// <summary>
        /// Gets the previous center position.
        /// </summary>
        /// <returns>Previous position of points centroid.</returns>
        public Vector2 GetPreviousCenterPosition() {
            return getPreviousCenterPosition(Points);
        }

        #endregion
    }
}