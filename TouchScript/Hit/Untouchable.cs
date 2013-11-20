/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// Makes an object it is attached to untouchable, i.e. it completely ignores all touch points landing on it.
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/Untouchable")]
    public class Untouchable : HitTest
    {
        /// <summary>
        /// If <c>true</c> touch point not only prevented but discarded making it impossible for other gestures to get it.
        /// </summary>
        public bool DiscardTouch = false;

        /// <inheritdoc />
        public override ObjectHitResult IsHit(TouchHit hit)
        {
            return DiscardTouch ? ObjectHitResult.Discard : ObjectHitResult.Miss;
        }
    }
}