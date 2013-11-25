using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/Camera Layer 2D")]
    public class CameraLayer2D : CameraLayerBase
    {

        protected override LayerHitResult castRay(Ray ray, out TouchHit hit)
        {
            hit = new TouchHit();
            var hits = Physics2D.GetRayIntersectionAll(ray, float.PositiveInfinity, LayerMask);

            if (hits.Length == 0) return LayerHitResult.Miss;

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

    }
}
