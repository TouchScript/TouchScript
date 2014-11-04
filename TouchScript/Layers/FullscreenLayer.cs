/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Layer which gets all input from a camera. Should be used instead of a background object getting all the touches which come through.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Fullscreen Layer")]
    public sealed class FullscreenLayer : TouchLayer
    {
        #region Constants

        /// <summary>
        /// The type of FullscreenLayer.
        /// </summary>
        public enum LayerType
        {
            /// <summary>
            /// Get touches from main camera.
            /// </summary>
            MainCamera,

            /// <summary>
            /// Get touches from specific camera.
            /// </summary>
            Camera
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
                if (_camera == null) return;
                if (_camera == Camera.main)
                {
                    Type = LayerType.MainCamera;
                    return;
                }

                _camera = value;
                Type = LayerType.Camera;
            }
        }

        /// <inheritdoc />
        public override Vector3 WorldProjectionNormal
        {
            get
            {
                if (_camera == null) return Vector3.forward;
                return cachedTransform.forward;
            }
        }

        /// <inheritdoc />
        public override Vector3 LayerOrigin
        {
            get
            {
                if (_camera == null) return Vector3.zero;
                return cachedTransform.position;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private LayerType type = LayerType.MainCamera;

        [SerializeField]
        private Camera _camera;

        private Transform cachedTransform;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            if (_camera == null) return LayerHitResult.Error;
            if (_camera.enabled == false || _camera.gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            if (!_camera.pixelRect.Contains(position)) return LayerHitResult.Miss;

            hit = TouchHitFactory.Instance.GetTouchHit(transform, ProjectTo(position, LayerOrigin + WorldProjectionNormal * _camera.farClipPlane, WorldProjectionNormal));
            var hitTests = transform.GetComponents<HitTest>();
            if (hitTests.Length == 0) return LayerHitResult.Hit;

            foreach (var test in hitTests)
            {
                if (!test.enabled) continue;
                var hitResult = test.IsHit(hit);
                if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) return LayerHitResult.Miss;
            }

            return LayerHitResult.Hit;
        }

        /// <inheritdoc />
        public override Vector3 ProjectTo(Vector2 screenPosition, Vector3 origin, Vector3 normal)
        {
            if (_camera == null) return TouchManager.INVALID_3D_POSITION;

            return ProjectionUtils.ProjectOnPlaneFromCamera(screenPosition, _camera, origin, normal);
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            updateCamera();

            base.Awake();
        }

        // To be able to turn it off
        private void OnEnable() {}

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            if (_camera == null) return;

            Name = "Fullscreen @ " + _camera.name;
        }

        #endregion

        #region Private functions

        private void updateCamera()
        {
            if (type == LayerType.MainCamera) 
            {
                _camera = Camera.main;
            }
            if (_camera == null)
            {
                Debug.LogError("Fullscreen layer couldn't find camera!");
                enabled = false;
                return;
            }

            cachedTransform = _camera.transform;
            setName();
        }

        #endregion
    }
}
