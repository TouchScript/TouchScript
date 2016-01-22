/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures.Clustered
{
    /// <summary>
    /// ScreenTransformGesture which works with centroid of all touches instead of with just the first touch.
    /// Should be used for large touch surfaces.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Clustered/Pinned Transform Gesture (Clustered)")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_Clustered_ClusteredPinnedTransformGesture.htm")]
    public class ClusteredPinnedTransformGesture : PinnedTransformGesture
    {
        #region Protected methods

        /// <inheritdoc />
        protected override bool relevantTouches(IList<TouchPoint> touches)
        {
            return true;
        }

        /// <inheritdoc />
        protected override Vector2 getPointScreenPosition()
        {
            return ClusterUtils.Get2DCenterPosition(activeTouches);
        }

        /// <inheritdoc />
        protected override Vector2 getPointPreviousScreenPosition()
        {
            return ClusterUtils.GetPrevious2DCenterPosition(activeTouches);
        }

        #endregion
    }
}