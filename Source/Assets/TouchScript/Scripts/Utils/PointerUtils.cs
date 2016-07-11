/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
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
        /// Determines whether the pointer is over its target GameObject.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsPointerOnTarget(Pointer pointer)
        {
            if (pointer == null) return false;
            return IsPointerOnTarget(pointer, pointer.GetPressData().Target);
        }

        /// <summary>
        /// Determines whether the pointer is over a specific GameObject.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <param name="target"> The target. </param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsPointerOnTarget(Pointer pointer, Transform target)
        {
            HitData hit;
            return IsPointerOnTarget(pointer, target, out hit);
        }

        /// <summary>
        /// Determines whether the pointer is over a specific GameObject.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="target">The target.</param>
        /// <param name="hit">The hit.</param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise. </returns>
        public static bool IsPointerOnTarget(Pointer pointer, Transform target, out HitData hit)
        {
            hit = default(HitData);
            if (pointer == null || target == null) return false;
            hit = pointer.GetOverData();
            if (hit.Target == null) return false;
            return hit.Target.IsChildOf(target);
        }
    }
}