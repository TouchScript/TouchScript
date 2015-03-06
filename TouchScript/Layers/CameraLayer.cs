/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Touch layer which represents a 3d camera looking into the world. Determines which objects may be hit in the view of a camera attached to parent GameObject.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Camera Layer")]
    public sealed class CameraLayer : CameraLayerBase
    {
        #region Private variables

        private List<RaycastHit> sortedHits;

        private Transform cachedTransform;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            cachedTransform = GetComponent<Transform>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            sortedHits = new List<RaycastHit>(20);
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override LayerHitResult castRay(Ray ray, out ITouchHit hit)
        {
            hit = null;
            var raycastHits = Physics.RaycastAll(ray, float.PositiveInfinity, LayerMask);

            if (raycastHits.Length == 0) return LayerHitResult.Miss;
            if (raycastHits.Length > 1)
            {
                sortHits(raycastHits);

                RaycastHit raycastHit = default(RaycastHit);
                var i = 0;
                while (i < sortedHits.Count)
                {
                    raycastHit = sortedHits[i];
                    switch (doHit(raycastHit, out hit))
                    {
                        case HitTest.ObjectHitResult.Hit:
                            return LayerHitResult.Hit;
                        case HitTest.ObjectHitResult.Discard:
                            return LayerHitResult.Miss;
                    }
                    i++;
                }
            }
            else
            {
                switch (doHit(raycastHits[0], out hit))
                {
                    case HitTest.ObjectHitResult.Hit:
                        return LayerHitResult.Hit;
                    case HitTest.ObjectHitResult.Error:
                        return LayerHitResult.Error;
                    default:
                        return LayerHitResult.Miss;
                }
            }

            return LayerHitResult.Miss;
        }

        private HitTest.ObjectHitResult doHit(RaycastHit raycastHit, out ITouchHit hit)
        {
            hit = TouchHitFactory.Instance.GetTouchHit(raycastHit);
            var hitTests = raycastHit.transform.GetComponents<HitTest>();
            if (hitTests.Length == 0) return HitTest.ObjectHitResult.Hit;

            var hitResult = HitTest.ObjectHitResult.Hit;
            foreach (var test in hitTests)
            {
                if (!test.enabled) continue;
                hitResult = test.IsHit(hit);
                if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) break;
            }

            return hitResult;
        }

        private void sortHits(RaycastHit[] hits)
        {
            var cameraPos = cachedTransform.position;
            sortedHits.Clear();
            sortedHits.AddRange(hits);
            sortedHits.Sort((a, b) =>
            {
                if (a.collider.transform == b.collider.transform) return 0;
                var distA = (a.point - cameraPos).sqrMagnitude;
                var distB = (b.point - cameraPos).sqrMagnitude;
                return distA < distB ? -1 : 1;
            });
        }

        #endregion
    }
}
