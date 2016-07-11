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
    /// Pointer layer which represents a 3d camera looking into the world. Determines which objects may be hit in the view of a camera attached to parent GameObject.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Camera Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_CameraLayer.htm")]
    public class CameraLayer : CameraLayerBase
    {
        #region Private variables
#if UNITY_5_3_OR_NEWER
        private RaycastHit[] raycastHits = new RaycastHit[20];
#endif
        private List<RaycastHit> sortedHits = new List<RaycastHit>(20);
        private Transform cachedTransform;

        private RaycastHitComparer comparer;

#endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying) return;

            cachedTransform = GetComponent<Transform>();
            comparer = new RaycastHitComparer(cachedTransform);
        }

#endregion

#region Protected functions

        /// <inheritdoc />
        protected override LayerHitResult castRay(Ray ray, out HitData hit)
        {
            hit = default(HitData);
#if UNITY_5_3_OR_NEWER
            var count = Physics.RaycastNonAlloc(ray, raycastHits, float.PositiveInfinity, LayerMask);
#else
            var raycastHits = Physics.RaycastAll(ray, float.PositiveInfinity, LayerMask);
            var count = raycastHits.Length;
#endif

            if (count == 0) return LayerHitResult.Miss;
            if (count > 1)
            {
                sortHits(raycastHits, count);

                RaycastHit raycastHit = default(RaycastHit);
                for (var i = 0; i < count; i++)
                {
                    raycastHit = sortedHits[i];
                    switch (doHit(raycastHit, out hit))
                    {
                        case HitTest.ObjectHitResult.Hit:
                            return LayerHitResult.Hit;
                        case HitTest.ObjectHitResult.Discard:
                            return LayerHitResult.Miss;
                    }
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

        private HitTest.ObjectHitResult doHit(RaycastHit raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this);
            return checkHitFilters(hit);
        }

        private void sortHits(RaycastHit[] hits, int count)
        {
            sortedHits.Clear();
            for (var i = 0; i < count; i++) sortedHits.Add(hits[i]);
            sortedHits.Sort(comparer);
        }

        #endregion

        private class RaycastHitComparer : IComparer<RaycastHit>
        {
            private Transform transform;

            public RaycastHitComparer(Transform transform)
            {
                this.transform = transform;
            }

            public int Compare(RaycastHit a, RaycastHit b)
            {
                var cameraPos = transform.position;
                if (a.collider.transform == b.collider.transform) return 0;
                var distA = (a.point - cameraPos).sqrMagnitude;
                var distB = (b.point - cameraPos).sqrMagnitude;
                return distA < distB ? -1 : 1;
            }
        }

    }
}