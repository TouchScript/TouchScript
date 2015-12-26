/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// <see cref="TouchLayer"/> specific projection parameters. Used by layers to project touches in the world and world coordinates onto layers.
    /// </summary>
    public class ProjectionParams
    {
        /// <summary>
        /// Projects a screen point on a 3D plane.
        /// </summary>
        /// <param name="screenPosition"> Screen point. </param>
        /// <param name="projectionPlane"> Projection plane. </param>
        /// <returns> Projected point in 3D. </returns>
        public virtual Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            return ProjectionUtils.ScreenToPlaneProjection(screenPosition, projectionPlane);
        }

        /// <summary>
        /// Projects a world point onto layer.
        /// </summary>
        /// <param name="worldPosition"> World position. </param>
        /// <returns> 2D point on layer. </returns>
        public virtual Vector2 ProjectFrom(Vector3 worldPosition)
        {
            return new Vector2(worldPosition.x, worldPosition.y);
        }
    }

    /// <summary>
    /// Projection parameters for a camera based <see cref="TouchLayer"/>.
    /// </summary>
    public class CameraProjectionParams : ProjectionParams
    {
        /// <summary>
        /// Camera used for projection.
        /// </summary>
        protected Camera camera;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProjectionParams"/> class.
        /// </summary>
        /// <param name="camera"> The camera. </param>
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

    /// <summary>
    /// Projection parameters for a UI based <see cref="TouchLayer"/>.
    /// </summary>
    public class CanvasProjectionParams : ProjectionParams
    {
        /// <summary>
        /// The canvas.
        /// </summary>
        protected Canvas canvas;

        /// <summary>
        /// Canvas RectTransform.
        /// </summary>
        protected RectTransform rect;

        /// <summary>
        /// Canvas mode.
        /// </summary>
        protected RenderMode mode;

        /// <summary>
        /// Canvas camera.
        /// </summary>
        protected Camera camera;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasProjectionParams"/> class.
        /// </summary>
        /// <param name="canvas"> The canvas. </param>
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

        /// <inheritdoc />
        public override Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            if (mode == RenderMode.ScreenSpaceOverlay) return base.ProjectTo(screenPosition, projectionPlane);
            return ProjectionUtils.CameraToPlaneProjection(screenPosition, camera, projectionPlane);
        }

        /// <inheritdoc />
        public override Vector2 ProjectFrom(Vector3 worldPosition)
        {
            if (mode == RenderMode.ScreenSpaceOverlay) return base.ProjectFrom(worldPosition);
            return camera.WorldToScreenPoint(worldPosition);
        }
    }
}