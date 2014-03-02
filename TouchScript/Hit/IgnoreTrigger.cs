using UnityEngine;

namespace TouchScript.Hit
{
    [AddComponentMenu("TouchScript/Behaviors/Ignore Trigger")]
    public sealed class IgnoreTrigger : HitTest
    {
        public override ObjectHitResult IsHit(ITouchHit hit)
        {
            var hit3d = hit as ITouchHit3D;
            if (hit3d != null && hit3d.Collider != null)
            {
                if (hit3d.Collider.isTrigger) return ObjectHitResult.Miss;
                return ObjectHitResult.Hit;
            }

            var hit2d = hit as ITouchHit2D;
            if (hit2d.Collider2D != null)
            {
                if (hit2d.Collider2D.isTrigger) return ObjectHitResult.Miss;
                return ObjectHitResult.Hit;
            }

            return ObjectHitResult.Hit;
        }
    }
}