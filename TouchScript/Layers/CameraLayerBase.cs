using System;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    public abstract class CameraLayerBase : TouchLayer
    {
        #region Public properties

        /// <summary>
        /// Layer mask to select layers which should be touchable from this CameraLayer.
        /// </summary>
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
            hit = null;

            if (camera == null) return LayerHitResult.Error;
            if (camera.enabled == false || camera.gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            if (!camera.pixelRect.Contains(position)) return LayerHitResult.Miss;

            var ray = camera.ScreenPointToRay(new Vector3(position.x, position.y, camera.nearClipPlane));
            return castRay(ray, out hit);
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override LayerHitResult beginTouch(ITouchPoint touch, out ITouchHit hit)
        {
            var result = Hit(touch.Position, out hit);
            return result;
        }

        /// <inheritdoc />
        protected override void setName()
        {
            if (String.IsNullOrEmpty(Name) && Camera != null) Name = Camera.name;
        }

        protected abstract LayerHitResult castRay(Ray ray, out ITouchHit hit);

        #endregion
    }
}