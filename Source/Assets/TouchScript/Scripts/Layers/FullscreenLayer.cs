/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Layer which gets all input from a camera. Should be used instead of a background object getting all the touches which come through.
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
            /// Get touches from main camera.
            /// </summary>
            MainCamera,

            /// <summary>
            /// Get touches from specific camera.
            /// </summary>
            Camera,

            /// <summary>
            /// Get all touches on Z=0 plane without a camera.
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
                setName();
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
        private List<HitTest> tmpHitTestList = new List<HitTest>(10);

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            if (_camera != null)
            {
                if (!_camera.pixelRect.Contains(position)) return LayerHitResult.Miss;
            }

            hit = new TouchHit(transform);
            transform.GetComponents(tmpHitTestList);
            var count = tmpHitTestList.Count;
            if (count == 0) return LayerHitResult.Hit;

            for (var i = 0; i < count; i++)
            {
                var test = tmpHitTestList[i];
                if (!test.enabled) continue;
                var hitResult = test.IsHit(hit);
                if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard)
                    return LayerHitResult.Miss;
            }

            return LayerHitResult.Hit;
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
        protected override void setName()
        {
            if (_camera == null) Name = "Global Fullscreen";
            else Name = "Fullscreen @ " + _camera.name;
        }

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
            setName();
        }

        private void cacheCameraTransform()
        {
            if (_camera == null) cameraTransform = null;
            else cameraTransform = _camera.transform;
        }

        #endregion
    }
}