/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Projection utils.
    /// </summary>
    public static class ProjectionUtils
    {
        /// <summary>
        /// Projects a screen point to a plane.
        /// </summary>
        /// <param name="position">Screen point.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="projectionPlane">The projection plane.</param>
        /// <returns>Projected point on the plane in World coordinates.</returns>
        public static Vector3 CameraToPlaneProjection(Vector2 position, Camera camera, Plane projectionPlane)
        {
            var distance = 0f;
            var ray = camera.ScreenPointToRay(position);
            var result = projectionPlane.Raycast(ray, out distance);
            if (!result && distance == 0f) return -projectionPlane.normal * projectionPlane.GetDistanceToPoint(Vector3.zero); // perpendicular to the screen

            return ray.origin + ray.direction * distance;
        }

        public static Vector3 ScreenToPlaneProjection(Vector2 position, Plane projectionPlane)
        {
            var distance = 0f;
            var ray = new Ray(position, Vector3.forward);
            var result = projectionPlane.Raycast(ray, out distance);
            if (!result && distance == 0f) return -projectionPlane.normal * projectionPlane.GetDistanceToPoint(Vector3.zero); // perpendicular to the screen

            return ray.origin + new Vector3(0, 0, distance);
        }
    }
}