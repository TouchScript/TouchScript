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
    /// Pointer layer which works with Unity 4.3+ 2d physics. Can pick 2d objects hit by pointers in right order.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Camera Layer 2D")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_CameraLayer2D.htm")]
    public class CameraLayer2D : CameraLayerBase
    {
        #region Private variables

        [SerializeField]
        [HideInInspector]
        private int[] layerIds = new int[0];

        private RaycastHit2D[] raycastHits = new RaycastHit2D[20];

        private Dictionary<int, int> layerById = new Dictionary<int, int>();
        private List<RaycastHit2D> sortedHits = new List<RaycastHit2D>(20);

        private RaycastHit2DComparer comparer;

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!Application.isPlaying) return;

            layerById.Clear();
            for (var i = 0; i < layerIds.Length; i++)
            {
                var value = layerIds[i];
                if (layerById.ContainsKey(value)) continue;
                layerById.Add(value, i);
            }

            comparer = new RaycastHit2DComparer(layerById);
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override HitResult castRay(IPointer pointer, Ray ray, out HitData hit)
        {
            hit = default(HitData);
            var count = Physics2D.GetRayIntersectionNonAlloc(ray, raycastHits, float.PositiveInfinity, LayerMask);

            if (count == 0) return HitResult.Miss;
            if (count > 1)
            {
                sortHits(raycastHits, count);

                RaycastHit2D raycastHit = default(RaycastHit2D);
                for (var i = 0; i < count; i++)
                {
                    raycastHit = sortedHits[i];
                    var result = doHit(pointer, raycastHit, out hit);
                    if (result != HitResult.Miss) return result;
                }
                return HitResult.Miss;
            }
            return doHit(pointer, raycastHits[0], out hit);
        }

        private HitResult doHit(IPointer pointer, RaycastHit2D raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this);
            return checkHitFilters(pointer, hit);
        }

        private void sortHits(RaycastHit2D[] hits, int count)
        {
            sortedHits.Clear();
            for (var i = 0; i < count; i++) sortedHits.Add(hits[i]);
            sortedHits.Sort(comparer);
        }

        #endregion

        private class RaycastHit2DComparer : IComparer<RaycastHit2D>
        {
            private Dictionary<int, int> layerById;

            public RaycastHit2DComparer(Dictionary<int, int> layerById)
            {
                this.layerById = layerById;
            }

            public int Compare(RaycastHit2D a, RaycastHit2D b)
            {
                if (a.collider.transform == b.collider.transform) return 0;

                var sprite1 = a.transform.GetComponent<SpriteRenderer>();
                var sprite2 = b.transform.GetComponent<SpriteRenderer>();
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

                return a.distance < b.distance ? -1 : 1;
            }
        }

    }
}
