using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/Camera Layer 2D")]
    public class CameraLayer2D : CameraLayerBase
    {

        private List<RaycastHit2D> sortedHits;

        protected override LayerHitResult castRay(Ray ray, out TouchHit hit)
        {
            hit = new TouchHit();
            var hits = Physics2D.GetRayIntersectionAll(ray, float.PositiveInfinity, LayerMask);

            if (hits.Length == 0) return LayerHitResult.Miss;
            if (hits.Length > 1) hits = sortHits(hits);

            var success = false;
            foreach (var raycastHit in hits)
            {
                hit = TouchHit.FromRaycastHit2D(raycastHit);
                var hitTests = raycastHit.transform.GetComponents<HitTest>();
                if (hitTests.Length == 0)
                {
                    success = true;
                    break;
                }

                var hitResult = HitTest.ObjectHitResult.Error;
                foreach (var test in hitTests)
                {
                    hitResult = test.IsHit(hit);
                    if (hitResult == HitTest.ObjectHitResult.Hit || hitResult == HitTest.ObjectHitResult.Discard)
                    {
                        break;
                    }
                }

                if (hitResult == HitTest.ObjectHitResult.Hit)
                {
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

        private void OnEnable()
        {
            sortedHits = new List<RaycastHit2D>();
        }

        private RaycastHit2D[] sortHits(RaycastHit2D[] hits)
        {
            sortedHits.Clear();
            sortedHits.AddRange(hits);
            sortedHits.Sort((a, b) =>
            {
                if (a.transform == b.transform) return 0;

                var sprite1 = a.transform.GetComponent<SpriteRenderer>();
                var sprite2 = b.transform.GetComponent<SpriteRenderer>();
                if (sprite1 == null || sprite2 == null) return 0;

                if (sprite1.sortingLayerID < sprite2.sortingLayerID) return 1;
                if (sprite1.sortingLayerID > sprite2.sortingLayerID) return -1;
                if (sprite1.sortingOrder < sprite2.sortingOrder) return 1;
                if (sprite1.sortingOrder > sprite2.sortingOrder) return -1;
                return 0;
            });
            return sortedHits.ToArray();
        }

    }
}
