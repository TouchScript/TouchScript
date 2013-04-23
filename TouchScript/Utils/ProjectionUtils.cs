/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Projection utils.
    /// </summary>
    public class ProjectionUtils
    {
        /// <summary>
        /// Projects a screen point to a plane.
        /// </summary>
        /// <param name="position">Screen point.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="projectionPlane">The projection plane.</param>
        /// <returns></returns>
        public static Vector3 CameraToPlaneProjection(Vector2 position, Camera camera, Plane projectionPlane)
        {
            var ray = camera.ScreenPointToRay(position);
            var relativeIntersection = 0f;
            projectionPlane.Raycast(ray, out relativeIntersection);
            return ray.origin + ray.direction*relativeIntersection;
        }
    }
}