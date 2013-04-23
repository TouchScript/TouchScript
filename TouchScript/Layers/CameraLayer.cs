/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Touch layer for a camera. Used to test if specific camera sees an object which should be hit by a touch point.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Camera Layer")]
    public class CameraLayer : TouchLayer
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
        public override LayerHitResult Hit(Vector2 position, out RaycastHit hit)
        {
            hit = new RaycastHit();

            if (camera == null) return LayerHitResult.Error;
            if (camera.enabled == false || camera.gameObject.active == false) return LayerHitResult.Miss;

            var ray = camera.ScreenPointToRay(new Vector3(position.x, position.y, camera.nearClipPlane));
            var hits = Physics.RaycastAll(ray, float.PositiveInfinity, layerMask);

            if (hits.Length == 0) return LayerHitResult.Miss;
            if (hits.Length > 1)
            {
                var cameraPos = camera.transform.position;
                var sortedHits = new List<RaycastHit>(hits);
                sortedHits.Sort((a, b) =>
                                {
                                    var distA = (a.point - cameraPos).sqrMagnitude;
                                    var distB = (b.point - cameraPos).sqrMagnitude;
                                    return distA < distB ? -1 : 1;
                                });
                hits = sortedHits.ToArray();
            }

            var success = false;
            foreach (var raycastHit in hits)
            {
                var hitTests = raycastHit.transform.GetComponents<HitTest>();
                if (hitTests.Length == 0)
                {
                    hit = raycastHit;
                    success = true;
                    break;
                }

                HitTest.ObjectHitResult hitResult = HitTest.ObjectHitResult.Error;
                foreach (var test in hitTests)
                {
                    hitResult = test.IsHit(raycastHit);
                    if (hitResult == HitTest.ObjectHitResult.Hit || hitResult == HitTest.ObjectHitResult.Discard)
                    {
                        break;
                    }
                }

                if (hitResult == HitTest.ObjectHitResult.Hit)
                {
                    hit = raycastHit;
                    success = true;
                    break;
                }
                if (hitResult == HitTest.ObjectHitResult.Discard)
                {
                    break;
                }
            }

            if (success)
            {
                return LayerHitResult.Hit;
            }

            return LayerHitResult.Miss;
        }

        /// <inheritdoc />
        protected override LayerHitResult beginTouch(TouchPoint touch)
        {
            RaycastHit hit;
            var result = Hit(touch.Position, out hit);
            if (result == LayerHitResult.Hit)
            {
                touch.Hit = hit;
                touch.Target = hit.transform;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void setName()
        {
            if (String.IsNullOrEmpty(Name) && Camera != null) Name = Camera.name;
        }
    }
}