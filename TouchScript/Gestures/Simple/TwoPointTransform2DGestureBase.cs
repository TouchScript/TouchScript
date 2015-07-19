/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Base class for transform gestures involving two points/clusters.
    /// </summary>
    public abstract class TwoPointTransform2DGestureBase : MultiPointTransform2DGestureBase
    {
        #region Public properties
        
        /// <summary>
        /// Minimum points count for gesture to begin.
        /// </summary>
        public override int MinPointsCount
        {
            get { return minPointsCount; }
            set { minPointsCount = 2; }
        }
        
        #endregion
    }
}