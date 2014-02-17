using UnityEngine;

namespace TouchScript.Hit
{
    [AddComponentMenu("TouchScript/Behaviors/Ignore Trigger")]
    public class IgnoreTrigger : HitTest
    {
        public override ObjectHitResult IsHit(TouchHit hit)
        {
            if (hit.Collider != null)
            {
                if (hit.Collider.isTrigger) return ObjectHitResult.Miss;
                return ObjectHitResult.Hit;
            }
            if (hit.Collider2d != null)
            {
                if (hit.Collider2d.isTrigger) return ObjectHitResult.Miss;
                return ObjectHitResult.Hit;
            }
            return ObjectHitResult.Hit;
        }
    }
}