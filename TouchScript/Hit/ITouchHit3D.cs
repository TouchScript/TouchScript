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
        /// Gets barycentric coordinate of the touch point.
        /// </summary>
        /// <value>
        /// Barycentric coordinate of touch point.
        /// </value>
        Vector3 BarycentricCoordinate { get; }

        /// <summary>
        /// Gets the collider which was hit by the touch.
        /// </summary>
        /// <value>
        /// The collider hit by the touch.
        /// </value>
        Collider Collider { get; }

        /// <summary>
        /// Gets the distance from the screen point to the world point where the object was hit.
        /// </summary>
        /// <value>
        /// The distance from the screen point to the world point where the object was hit.
        /// </value>
        float Distance { get; }

        /// <summary>
        /// Gets the lightmap coordinate of the world point where the object was hit.
        /// </summary>
        /// <value>
        /// The lightmap coordinate of the world point where the object was hit.
        /// </value>
        Vector2 LightmapCoord { get; }

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

        /// <summary>
        /// Gets the texture coordinate at the point where the object was hit.
        /// </summary>
        /// <value>
        /// The texture coordinate at the point where the object was hit.
        /// </value>
        Vector2 TextureCoord { get; }

        /// <summary>
        /// Gets the second texture coordinate at the point where the object was hit.
        /// </summary>
        /// <value>
        /// The second texture coordinate at the point where the object was hit.
        /// </value>
        Vector2 TextureCoord2 { get; }

        /// <summary>
        /// Gets the index of the hit triangle.
        /// </summary>
        /// <value>
        /// The index of the hit triangle.
        /// </value>
        int TriangleIndex { get; }
    }
}