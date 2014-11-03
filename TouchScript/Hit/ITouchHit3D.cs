/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// An object representing metadata for touches hitting 3d objects with 3d colliders.
    /// </summary>
    public interface ITouchHit3D : ITouchHit
    {
        /// <summary>
        /// Gets the collider which was hit by the touch.
        /// </summary>
        /// <value>
        /// The collider hit by the touch.
        /// </value>
        Collider Collider { get; }

        /// <summary>
        /// Gets the normal to the surface at the point where the object was hit.
        /// </summary>
        /// <value>
        /// The normal to the surface at the point where the object was hit.
        /// </value>
        Vector3 Normal { get; }

        /// <summary>
        /// Gets the world coordinate of the point where the object was hit.
        /// </summary>
        /// <value>
        /// World coordinate of the point where the object was hit.
        /// </value>
        Vector3 Point { get; }

        /// <summary>
        /// Gets the rigidbody of the hit object.
        /// </summary>
        /// <value>
        /// The rigidbody on the object which was hit.
        /// </value>
        Rigidbody Rigidbody { get; }
    }
}
