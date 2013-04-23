/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    [AddComponentMenu("TouchScript/Behaviors/Untouchable")]
    class Untouchable : HitTest
    {

        public bool DiscardTouch = false;

        public override ObjectHitResult IsHit(RaycastHit hit)
        {
            return DiscardTouch ? ObjectHitResult.Discard : ObjectHitResult.Miss;
        }

    }
}
