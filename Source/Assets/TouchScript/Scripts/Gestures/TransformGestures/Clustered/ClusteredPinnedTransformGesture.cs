/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Gestures.TransformGestures.Clustered
{
    /// <summary>
    /// PinnedTransformGesture which works with centroid of all pointers instead of with just the first one.
    /// Should be used for large touch surfaces.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Clustered/Pinned Transform Gesture (Clustered)")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_TransformGestures_Clustered_ClusteredPinnedTransformGesture.htm")]
    public class ClusteredPinnedTransformGesture : PinnedTransformGesture
    {
        #region Protected methods

        /// <inheritdoc />
        protected override bool relevantPointers(IList<Pointer> pointers)
        {
            return true;
        }

        /// <inheritdoc />
        protected override Vector2 getPointScreenPosition()
        {
            return ClusterUtils.Get2DCenterPosition(activePointers);
        }

        /// <inheritdoc />
        protected override Vector2 getPointPreviousScreenPosition()
        {
            return ClusterUtils.GetPrevious2DCenterPosition(activePointers);
        }

        #endregion
    }
}