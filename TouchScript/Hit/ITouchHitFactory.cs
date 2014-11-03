/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// A factory which is used to create instances of ITouchHit.
    /// </summary>
    public interface ITouchHitFactory
    {
        /// <summary>
        /// Creates an instance of ITouchHit from value.
        /// </summary>
        /// <param name="value">Result of Physics.Raycast.</param>
        /// <returns>An instance of ITouchHit.</returns>
        ITouchHit GetTouchHit(RaycastHit value);

        /// <summary>
        /// Creates an instance of ITouchHit from value.
        /// </summary>
        /// <param name="value">Result of Physics2D.Raycast.</param>
        /// <returns>An instance of ITouchHit.</returns>
        ITouchHit GetTouchHit(RaycastHit2D value);

        /// <summary>
        /// Creates an instance of ITouchHit from value.
        /// </summary>
        /// <param name="value">Transform which has been hit.</param>
        /// <returns>An instance of ITouchHit.</returns>
        ITouchHit GetTouchHit(Transform value);
    }
}
