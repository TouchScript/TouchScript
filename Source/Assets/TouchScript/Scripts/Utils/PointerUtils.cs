/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.Layers;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Pointer utils.
    /// </summary>
    public static class PointerUtils
    {
        /// <summary>
        /// Determines whether the pointer is over a specific GameObject.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <param name="target"> The target. </param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsPointerOnTarget(Pointer pointer, Transform target)
        {
            if (pointer == null || pointer.Layer == null || target == null) return false;
            TouchHit hit;
            if ((pointer.Layer.Hit(pointer.Position, out hit) == TouchLayer.LayerHitResult.Hit) &&
                (target == hit.Transform || hit.Transform.IsChildOf(target)))
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the pointer is over its target GameObject.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsPointerOnTarget(Pointer pointer)
        {
            if (pointer == null) return false;
            return IsPointerOnTarget(pointer, pointer.Target);
        }
    }
}