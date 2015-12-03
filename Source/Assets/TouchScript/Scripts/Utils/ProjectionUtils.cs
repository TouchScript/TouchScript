/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Projection utils.
    /// </summary>
    public static class ProjectionUtils
    {
        /// <summary>
        /// Projects a screen point to a plane from a camera's point of view.
        /// </summary>
        /// <param name="position">Screen point.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="projectionPlane">Projection plane.</param>
        /// <returns>Projected point on the plane in World coordinates.</returns>
        public static Vector3 CameraToPlaneProjection(Vector2 position, Camera camera, Plane projectionPlane)
        {
            var distance = 0f;
            var ray = camera.ScreenPointToRay(position);
            var result = projectionPlane.Raycast(ray, out distance);
            if (!result && Mathf.Approximately(distance, 0f)) return -projectionPlane.normal * projectionPlane.GetDistanceToPoint(Vector3.zero); // perpendicular to the screen

            return ray.origin + ray.direction * distance;
        }

        /// <summary>
        /// Projects a screen point to a plane using parallel projection.
        /// </summary>
        /// <param name="position">Screen point.</param>
        /// <param name="projectionPlane">Projection plane.</param>
        /// <returns>Projected point on the plane in World coordinates.</returns>
        public static Vector3 ScreenToPlaneProjection(Vector2 position, Plane projectionPlane)
        {
            var distance = 0f;
            var ray = new Ray(position, Vector3.forward);
            var result = projectionPlane.Raycast(ray, out distance);
            if (!result && Mathf.Approximately(distance, 0f)) return -projectionPlane.normal * projectionPlane.GetDistanceToPoint(Vector3.zero); // perpendicular to the screen

            return ray.origin + new Vector3(0, 0, distance);
        }
    }
}