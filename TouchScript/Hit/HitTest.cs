/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// Parent class for all hit test handlers.
    /// </summary>
    public abstract class HitTest : MonoBehaviour
    {

        /// <summary>
        /// Result of a check to see if a hit object should recieve this touch or not.
        /// </summary>
        public enum ObjectHitResult
        {
            /// <summary>
            /// Something happened.
            /// </summary>
            Error = 0,
            /// <summary>
            /// This is a hit, object should recieve touch.
            /// </summary>
            Hit = 1,
            /// <summary>
            /// Object should not recieve touch.
            /// </summary>
            Miss = 2,
            /// <summary>
            /// Object should not recieve touch and this touch should be discarded and not tested with any other object.
            /// </summary>
            Discard = 3
        }

        /// <summary>
        /// Determines whether a touch point hit the object.
        /// </summary>
        /// <param name="hit">Data from a raycast.</param>
        /// <returns>Hit if touch point hits the object, Miss if it doesn't, Discard if it doesn't and this touch must be ignored, Error otherwise.</returns>
        public virtual ObjectHitResult IsHit(RaycastHit hit)
        {
            return ObjectHitResult.Hit;
        }
    }
}