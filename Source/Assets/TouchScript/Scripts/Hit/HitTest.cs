/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Hit
{

    /// <summary>
    /// Result of a check to find if a hit object should recieve this pointer or not.
    /// </summary>
    public enum HitResult
    {
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

    /// <summary>
    /// Base class for all hit test handlers.
    /// </summary>
    public abstract class HitTest : MonoBehaviour
    {

        #region Public methods

        /// <summary>
        /// Determines whether a pointer hit the object.
        /// </summary>
        /// <param name="pointer"> Pointer to raycast. </param>
        /// <param name="hit"> Data from a raycast. </param>
        /// <returns> <see cref="HitResult.Hit"/> if pointer hits the object, <see cref="HitResult.Miss"/> if it doesn't, <see cref="HitResult.Discard"/> if it doesn't and this pointer must be ignored, Error otherwise. </returns>
        public virtual HitResult IsHit(IPointer pointer, HitData hit)
        {
            return HitResult.Hit;
        }

        #endregion

        #region Unity methods

        private void OnEnable() {}

        #endregion
    }
}