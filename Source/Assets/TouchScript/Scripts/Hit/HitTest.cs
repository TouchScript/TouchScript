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
        /// Result of a check to find if a hit object should recieve this touch or not.
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

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether a touch point hit the object.
        /// </summary>
        /// <param name="hit"> Data from a raycast. </param>
        /// <returns> <see cref="ObjectHitResult.Hit"/> if touch point hits the object, <see cref="ObjectHitResult.Miss"/> if it doesn't, <see cref="ObjectHitResult.Discard"/> if it doesn't and this touch must be ignored, Error otherwise. </returns>
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