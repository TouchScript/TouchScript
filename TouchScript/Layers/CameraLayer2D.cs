/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace TouchScript.Layers
{
    /// <summary>
    /// Touch layer which works with Unity 4.3+ 2d physics. Can pick 2d objects hit by touches in right order.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Camera Layer 2D")]
    public sealed class CameraLayer2D : CameraLayerBase
    {
        #region Private variables

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("sortedLayerIds")]
        private int[] layerIds = new int[0];

        private Dictionary<int, int> layerById = new Dictionary<int, int>();

        private List<RaycastHit2D> sortedHits;

        #endregion

        #region Unity methods

        private void OnEnable()
        {
            layerIds = LayerUtils.GetSortingLayerUniqueIDs ();
            sortedHits = new List<RaycastHit2D>();
            layerById.Clear();
            for (var i = 0; i < layerIds.Length; i++)
            {
                var value = layerIds[i];
                if (layerById.ContainsKey(value)) continue;
                layerById.Add(value, i);
            }
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override LayerHitResult castRay(Ray ray, out ITouchHit hit)
        {
            hit = null;
            var raycastHits = Physics2D.GetRayIntersectionAll(ray, float.PositiveInfinity, LayerMask);

            if (raycastHits.Length == 0) return LayerHitResult.Miss;
            if (raycastHits.Length > 1)
            {
                sortHits(raycastHits);

                RaycastHit2D raycastHit = default(RaycastHit2D);
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

        private HitTest.ObjectHitResult doHit(RaycastHit2D raycastHit, out ITouchHit hit)
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

        private void sortHits(RaycastHit2D[] hits)
        {
            sortedHits.Clear();
            sortedHits.AddRange(hits);
            sortedHits.Sort((a, b) =>
            {
                if (a.collider.transform == b.collider.transform) return 0;

                var sprite1 = a.collider.transform.GetComponent<SpriteRenderer>();
                var sprite2 = b.collider.transform.GetComponent<SpriteRenderer>();
                if (sprite1 != null && sprite2 != null)
                {
                    int s1Id, s2Id;
                    if (!layerById.TryGetValue(sprite1.sortingLayerID, out s1Id)) s1Id = 0;
                    if (!layerById.TryGetValue(sprite2.sortingLayerID, out s2Id)) s2Id = 0;
                    if (s1Id < s2Id) return 1;
                    if (s1Id > s2Id) return -1;
                    if (sprite1.sortingOrder < sprite2.sortingOrder) return 1;
                    if (sprite1.sortingOrder > sprite2.sortingOrder) return -1;
                }

                var cameraPos = GetComponent<Camera>().transform.position;
                var distA = (a.transform.position - cameraPos).sqrMagnitude;
                var distB = (b.transform.position - cameraPos).sqrMagnitude;
                return distA < distB ? -1 : 1;
            });
        }

        #endregion
    }
}
