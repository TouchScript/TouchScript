/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// An object representing metadata for touches hitting 2d objects with 2d colliders.
    /// </summary>
    public interface ITouchHit2D : ITouchHit
    {
        /// <summary>
        /// Gets touched 2d collider.
        /// </summary>
        /// <value>
        /// Collider2D which was hit.
        /// </value>
        Collider2D Collider2D { get; }

        /// <summary>
        /// Gets the coordinates where the touch hits the object.
        /// </summary>
        /// <value>
        /// World coordinates of the point where the object was touched.
        /// </value>
        Vector3 Point { get; }

        /// <summary>
        /// Gets the Rigidbidy2D which was touched.
        /// </summary>
        /// <value>
        /// Touched Rigidbody2D.
        /// </value>
        Rigidbody2D Rigidbody2D { get; }
    }
}
