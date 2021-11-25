/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Layer which gets all input from a camera. Should be used instead of a background object getting all the pointers which come through.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Fullscreen Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_FullscreenLayer.htm")]
    public class FullscreenLayer : TouchLayer
    {
        #region Constants

        /// <summary>
        /// The type of FullscreenLayer.
        /// </summary>
        public enum LayerType
        {
            /// <summary>
            /// Get pointers from main camera.
            /// </summary>
            MainCamera,

            /// <summary>
            /// Get pointers from specific camera.
            /// </summary>
            Camera,

            /// <summary>
            /// Get all pointers on Z=0 plane without a camera.
            /// </summary>
            Global
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Layer type.
        /// </summary>
        public LayerType Type
        {
            get { return type; }
            set
            {
                if (value == type) return;
                type = value;
                updateCamera();
                cacheCameraTransform();
            }
        }

        /// <summary>
        /// Target camera if <see cref="LayerType.Camera"/> or <see cref="LayerType.MainCamera"/> is used.
        /// </summary>
        public Camera Camera
        {
            get { return _camera; }
            set
            {
                if (value == _camera) return;
                _camera = value;
                if (_camera == null) Type = LayerType.Global;
                else Type = LayerType.Camera;
            }
        }

        /// <inheritdoc />
        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(layerName))
                {
                    if (_camera == null) return "Global Fullscreen";
                    return "Fullscreen @ " + _camera.name;
                }
                return layerName;
            }
        }

        /// <inheritdoc />
        public override Vector3 WorldProjectionNormal
        {
            get
            {
                if (cameraTransform == null) return transform.forward;
                return cameraTransform.forward;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private LayerType type = LayerType.MainCamera;

        [SerializeField]
        private Camera _camera;

        private Transform cameraTransform;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override HitResult Hit(IPointer pointer, out HitData hit)
        {
            if (base.Hit(pointer, out hit) != HitResult.Hit) return HitResult.Miss;

            if (_camera != null)
            {
                if (!_camera.pixelRect.Contains(pointer.Position)) return HitResult.Miss;
            }

            hit = new HitData(transform, this);
            var result = checkHitFilters(pointer, hit);
            if (result != HitResult.Hit)
            {
                hit = default(HitData);
                return result;
            }
            return HitResult.Hit;
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            updateCamera();
            base.Awake();
            if (!Application.isPlaying) return;

            cacheCameraTransform();
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override ProjectionParams createProjectionParams()
        {
            if (_camera) return new CameraProjectionParams(_camera);
            return base.createProjectionParams();
        }

        #endregion

        #region Private functions

        private void updateCamera()
        {
            switch (type)
            {
                case LayerType.Global:
                    _camera = null;
                    break;
                case LayerType.MainCamera:
                    _camera = Camera.main;
                    if (_camera == null) Debug.LogError("No Main camera found!");
                    break;
            }
        }

        private void cacheCameraTransform()
        {
            if (_camera == null) cameraTransform = null;
            else cameraTransform = _camera.transform;
        }

        #endregion
    }
}