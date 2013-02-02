/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/Camera Layer")]
    public class CameraLayer : LayerBase
    {
        public override HitResult Hit(Vector2 position, out RaycastHit hit, out Camera hitCamera)
        {
            hit = new RaycastHit();
            hitCamera = null;

            if (camera == null) return HitResult.Error;

            var ray = camera.ScreenPointToRay(new Vector3(position.x, position.y, camera.nearClipPlane));
            var hits = Physics.RaycastAll(ray);

            if (hits.Length == 0) return HitResult.Miss;
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

                var allowed = true;
                foreach (var test in hitTests)
                {
                    allowed = test.IsHit(raycastHit);
                    if (!allowed) break;
                }
                if (allowed)
                {
                    hit = raycastHit;
                    success = true;
                    break;
                }
            }

            if (success)
            {
                hitCamera = camera;
                return HitResult.Hit;
            }

            return HitResult.Miss;
        }
    }
}