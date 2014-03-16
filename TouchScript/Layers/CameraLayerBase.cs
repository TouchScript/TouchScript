/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Abstract base class for camera-based layers.
    /// </summary>
    public abstract class CameraLayerBase : TouchLayer
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the layer mask which is used to select layers which should be touchable from this layer.
        /// </summary>
        /// <value>A mask to exclude objects from possibly touchable list.</value>
        public LayerMask LayerMask
        {
            get { return layerMask; }
            set { layerMask = value; }
        }

        /// <inheritdoc />
        public override Camera Camera
        {
            get { return camera; }
        }

        #endregion

        #region Private fields

        [SerializeField]
        private LayerMask layerMask = -1;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            if (camera == null) return LayerHitResult.Error;
            if (camera.enabled == false || camera.gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            if (!camera.pixelRect.Contains(position)) return LayerHitResult.Miss;

            var ray = camera.ScreenPointToRay(new Vector3(position.x, position.y, camera.nearClipPlane));
            return castRay(ray, out hit);
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            if (String.IsNullOrEmpty(Name) && Camera != null) Name = Camera.name;
        }

        /// <summary>
        /// Casts a ray from camera position.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="hit">Hit information if the ray has hit something.</param>
        /// <returns>Hit result.</returns>
        protected abstract LayerHitResult castRay(Ray ray, out ITouchHit hit);

        #endregion
    }
}