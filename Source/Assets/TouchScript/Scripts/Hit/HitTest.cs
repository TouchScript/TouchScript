/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// Base class for all hit test handlers.
    /// </summary>
    public abstract class HitTest : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Result of a check to find if a hit object should recieve this pointer or not.
        /// </summary>
        public enum ObjectHitResult
        {
            /// <summary>
            /// Something happened.
            /// </summary>
            Error = 0,

            /// <summary>
            /// This is a hit, object should recieve pointer.
            /// </summary>
            Hit = 1,

            /// <summary>
            /// Object should not recieve pointer.
            /// </summary>
            Miss = 2,

            /// <summary>
            /// Object should not recieve pointer and this pointer should be discarded and not tested with any other object.
            /// </summary>
            Discard = 3
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether a pointer hit the object.
        /// </summary>
        /// <param name="hit"> Data from a raycast. </param>
        /// <returns> <see cref="ObjectHitResult.Hit"/> if pointer hits the object, <see cref="ObjectHitResult.Miss"/> if it doesn't, <see cref="ObjectHitResult.Discard"/> if it doesn't and this pointer must be ignored, Error otherwise. </returns>
        public virtual ObjectHitResult IsHit(TouchHit hit)
        {
            return ObjectHitResult.Hit;
        }

        #endregion

        #region Unity methods

        private void OnEnable() {}

        #endregion
    }
}