using System;
using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    public abstract class CameraLayerBase : TouchLayer
    {

        #region Private fields

        [SerializeField]
        private LayerMask layerMask = -1;

        #endregion

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

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            hit = new TouchHit();

            if (camera == null) return LayerHitResult.Error;
            if (camera.enabled == false || camera.gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            if (!camera.pixelRect.Contains(position)) return LayerHitResult.Miss;

            var ray = camera.ScreenPointToRay(new Vector3(position.x, position.y, camera.nearClipPlane));
            return castRay(ray, out hit);
        }

        /// <inheritdoc />
        protected override LayerHitResult beginTouch(TouchPoint touch)
        {
            TouchHit hit;
            var result = Hit(touch.Position, out hit);
            if (result == LayerHitResult.Hit)
            {
                touch.Hit = hit;
                touch.Target = hit.Transform;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void setName()
        {
            if (String.IsNullOrEmpty(Name) && Camera != null) Name = Camera.name;
        }

        protected abstract LayerHitResult castRay(Ray ray, out TouchHit hit);

    }
}
