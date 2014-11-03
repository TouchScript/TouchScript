/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Hit;
using TouchScript.Utils;
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
        public override Vector3 WorldProjectionNormal
        {
            get
            {
                if (_camera == null) return Vector3.forward;
                return _camera.transform.forward;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private LayerMask layerMask = -1;

        /// <summary>
        /// Camera.
        /// </summary>
        protected Camera _camera;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            if (_camera == null) return LayerHitResult.Error;
            if (_camera.enabled == false || _camera.gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            if (!_camera.pixelRect.Contains(position)) return LayerHitResult.Miss;

            var ray = _camera.ScreenPointToRay(position);
            return castRay(ray, out hit);
        }

        /// <inheritdoc />
        public override Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            return ProjectionUtils.CameraToPlaneProjection(screenPosition, _camera, projectionPlane);
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            updateCamera();

            base.Awake();
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            if (String.IsNullOrEmpty(Name) && _camera != null) Name = _camera.name;
        }

        /// <summary>
        /// Finds a camera.
        /// </summary>
        protected virtual void updateCamera()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null) _camera = Camera.main;
            if (_camera != null) return;
            Debug.LogError("No Camera found for CameraLayer '" + name + "'.");
            enabled = false;
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
