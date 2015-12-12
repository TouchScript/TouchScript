/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Touch utils.
    /// </summary>
    public static class TouchUtils
    {
        /// <summary>
        /// Determines whether the touch is over a specific GameObject.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        /// <param name="target"> The target. </param>
        /// <returns> <c>true</c> if the touch is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsTouchOnTarget(TouchPoint touch, Transform target)
        {
            if (touch == null || touch.Layer == null || target == null) return false;
            TouchHit hit;
            if ((touch.Layer.Hit(touch.Position, out hit) == TouchLayer.LayerHitResult.Hit) &&
                (target == hit.Transform || hit.Transform.IsChildOf(target)))
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the touch is over its target GameObject.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        /// <returns> <c>true</c> if the touch is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsTouchOnTarget(TouchPoint touch)
        {
            if (touch == null) return false;
            return IsTouchOnTarget(touch, touch.Target);
        }
    }
}