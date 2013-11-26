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
    public class CameraLayer : CameraLayerBase
    {

        private List<RaycastHit> sortedHits;

        protected override LayerHitResult castRay(Ray ray, out TouchHit hit)
        {
            hit = new TouchHit();
            var hits = Physics.RaycastAll(ray, float.PositiveInfinity, LayerMask);

            if (hits.Length == 0) return LayerHitResult.Miss;
            if (hits.Length > 1) hits = sortHits(hits);

            var success = false;
            foreach (var raycastHit in hits)
            {
                hit = TouchHit.FromRaycastHit(raycastHit);
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
            sortedHits = new List<RaycastHit>();
        }

        private RaycastHit[] sortHits(RaycastHit[] hits)
        {
            var cameraPos = camera.transform.position;
            sortedHits.Clear();
            sortedHits.AddRange(hits);
            sortedHits.Sort((a, b) =>
            {
                if (a.transform == b.transform) return 0;
                var distA = (a.point - cameraPos).sqrMagnitude;
                var distB = (b.point - cameraPos).sqrMagnitude;
                return distA < distB ? -1 : 1;
            });
            return sortedHits.ToArray();
        }

    }
}