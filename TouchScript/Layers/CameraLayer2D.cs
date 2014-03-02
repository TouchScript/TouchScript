using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/Camera Layer 2D")]
    public sealed class CameraLayer2D : CameraLayerBase
    {
        #region Private variables

        [SerializeField]
        [HideInInspector]
        private int[] sortedLayerIds = new int[0];

        private List<RaycastHit2D> sortedHits;

        #endregion

        #region Unity methods

        private void OnEnable()
        {
            sortedHits = new List<RaycastHit2D>();
        }

        #endregion

        #region Protected functions

        protected override LayerHitResult castRay(Ray ray, out ITouchHit hit)
        {
            hit = null;
            var hits = Physics2D.GetRayIntersectionAll(ray, float.PositiveInfinity, LayerMask);

            if (hits.Length == 0) return LayerHitResult.Miss;
            if (hits.Length > 1) hits = sortHits(hits);

            var success = false;
            foreach (var raycastHit in hits)
            {
                hit = TouchHitFactory.Instance.GetTouchHit(raycastHit);
                var hitTests = raycastHit.transform.GetComponents<HitTest>();
                if (hitTests.Length == 0)
                {
                    success = true;
                    break;
                }

                var hitResult = HitTest.ObjectHitResult.Hit;
                foreach (var test in hitTests)
                {
                    if (!test.enabled) continue;
                    hitResult = test.IsHit(hit);
                    if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) break;
                }

                if (hitResult == HitTest.ObjectHitResult.Hit)
                {
                    success = true;
                    break;
                }
                if (hitResult == HitTest.ObjectHitResult.Discard) break;
            }

            if (success) return LayerHitResult.Hit;

            return LayerHitResult.Miss;
        }

        private RaycastHit2D[] sortHits(RaycastHit2D[] hits)
        {
            sortedHits.Clear();
            sortedHits.AddRange(hits);
            sortedHits.Sort((a, b) =>
            {
                if (a.collider.transform == b.collider.transform) return 0;

                var sprite1 = a.transform.GetComponent<SpriteRenderer>();
                var sprite2 = b.transform.GetComponent<SpriteRenderer>();
                if (sprite1 == null || sprite2 == null) return 0;

                var s1Id = sprite1.sortingLayerID < sortedLayerIds.Length ? sortedLayerIds[sprite1.sortingLayerID] : 0;
                var s2Id = sprite2.sortingLayerID < sortedLayerIds.Length ? sortedLayerIds[sprite2.sortingLayerID] : 0;
                if (s1Id < s2Id) return 1;
                if (s1Id > s2Id) return -1;
                if (sprite1.sortingOrder < sprite2.sortingOrder) return 1;
                if (sprite1.sortingOrder > sprite2.sortingOrder) return -1;
                return 0;
            });
            return sortedHits.ToArray();
        }

        #endregion
    }
}