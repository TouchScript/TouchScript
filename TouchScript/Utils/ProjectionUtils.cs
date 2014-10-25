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
        public static Vector3 NormalizeScreenPosition(Vector2 screenPosition)
        {
            return new Vector3(screenPosition.x/Screen.width - .5f, (screenPosition.y - Screen.height*.5f)/Screen.width, 0);
        }

        /// <summary>
        /// Projects a screen point to a plane from a camera's point of view.
        /// </summary>
        /// <param name="position">Screen point.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="planeOrigin">Projection plane origin.</param>
        /// <param name="planeNormal">Projection plane normal.</param>
        /// <returns>Projected point on the plane in World coordinates. If planeNormal is perpendicular to Camera.forward, returns Vector3.zero.</returns>
        public static Vector3 CameraToPlaneProjection(Vector2 position, Camera camera, Vector3 planeOrigin, Vector3 planeNormal)
        {
            var ray = camera.ScreenPointToRay(position);
            var VdotN = Vector3.Dot(ray.direction, planeNormal);
            if (Mathf.Approximately(VdotN, 0)) return Vector3.zero;

            return ray.origin - (Vector3.Dot(ray.origin, planeNormal) - Vector3.Dot(planeOrigin, planeNormal)) / VdotN*ray.direction;
        }

        /// <summary>
        /// Projects a screen point from the z=0 plane to another plane using parallel projection.
        /// </summary>
        /// <param name="position">Screen point.</param>
        /// <param name="planeOrigin">Projection plane origin.</param>
        /// <param name="planeNormal">Projection plane normal.</param>
        /// <returns>Projected point on the plane in World coordinates. If planeNormal is perpendicular to Camera.forward, returns Vector3.zero.</returns>
        public static Vector3 ScreenToPlaneProjection(Vector2 position, Vector3 planeOrigin, Vector3 planeNormal)
        {
            var ray = new Ray(position, Vector3.forward);
            var VdotN = Vector3.Dot(ray.direction, planeNormal);
            if (Mathf.Approximately(VdotN, 0)) return Vector3.zero;

            return ray.origin - (Vector3.Dot(ray.origin, planeNormal) - Vector3.Dot(planeOrigin, planeNormal)) / VdotN * ray.direction;
        }
    }
}