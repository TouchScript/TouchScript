/*
 * @author Valentin Simonov / http://va.lent.in/
 */


using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Projection utils.
    /// </summary>
    internal static class ProjectionUtils
    {
        internal static Vector3 NormalizeScreenPosition(Vector2 screenPosition)
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
        internal static Vector3 ProjectOnPlaneFromCamera(Vector2 position, Camera camera, Vector3 planeOrigin, Vector3 planeNormal)
        {
            var ray = camera.ScreenPointToRay(position);
            return ProjectOnPlane(ray.origin, ray.direction, planeOrigin, planeNormal);
        }

        internal static Vector3 ProjectOnPlane(Vector3 point, Vector3 direction, Vector3 planeOrigin, Vector3 planeNormal)
        {
            var VdotN = Vector3.Dot(direction, planeNormal);
            if (Mathf.Approximately(VdotN, 0)) return Vector3.zero;

            return point - (Vector3.Dot(point, planeNormal) - Vector3.Dot(planeOrigin, planeNormal)) / VdotN * direction;
        }
    }
}