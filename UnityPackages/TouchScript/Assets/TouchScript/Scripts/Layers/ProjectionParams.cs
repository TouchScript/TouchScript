/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// </summary>
    public class ProjectionParams
    {

        /// <summary>
        /// </summary>
        public virtual Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            return ProjectionUtils.ScreenToPlaneProjection(screenPosition, projectionPlane);
        }

        /// <summary>
        /// </summary>
        public virtual Vector2 ProjectFrom(Vector3 worldPosition)
        {
            return new Vector2(worldPosition.x, worldPosition.y);
        }

    }

    /// <summary>
    /// </summary>
    public class CameraProjectionParams : ProjectionParams
    {

        /// <summary>
        /// </summary>
        protected Camera camera;

        /// <summary>
        /// </summary>
        public CameraProjectionParams(Camera camera)
        {
            this.camera = camera;
        }

        /// <inheritdoc />
        public override Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            return ProjectionUtils.CameraToPlaneProjection(screenPosition, camera, projectionPlane);
        }

        /// <inheritdoc />
        public override Vector2 ProjectFrom(Vector3 worldPosition)
        {
            return camera.WorldToScreenPoint(worldPosition);
        }
    }

    public class CanvasProjectionParams : ProjectionParams
    {

        protected Canvas canvas;
        protected RectTransform rect;
        protected RenderMode mode;
        protected Camera camera;

        public CanvasProjectionParams(Canvas canvas)
        {
            this.canvas = canvas;
            mode = canvas.renderMode;

            if (mode == RenderMode.ScreenSpaceOverlay)
            {
                rect = canvas.GetComponent<RectTransform>();
            }
            else
            {
                camera = canvas.worldCamera ?? Camera.main;
            }
        }

        public override Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            if (mode == RenderMode.ScreenSpaceOverlay) return base.ProjectTo(screenPosition, projectionPlane);
            return ProjectionUtils.CameraToPlaneProjection(screenPosition, camera, projectionPlane);
        }

        public override Vector2 ProjectFrom(Vector3 worldPosition)
        {
            if (mode == RenderMode.ScreenSpaceOverlay) return base.ProjectFrom(worldPosition);
            return camera.WorldToScreenPoint(worldPosition);
        }
    }

}
