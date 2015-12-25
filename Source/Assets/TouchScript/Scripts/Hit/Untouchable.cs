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
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Hit_Untouchable.htm")]
    public class Untouchable : HitTest
    {
        #region Public properties

        /// <summary>
        /// Indicates if instead of not reacting to touches the object should completely discard them making it impossible for other gestures to receive them.
        /// </summary>
        /// <value> If <c>true</c> touch points are not only prevented but discarded making it impossible for other gestures to receive them. </value>
        public bool DiscardTouch = false;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override ObjectHitResult IsHit(TouchHit hit)
        {
            return DiscardTouch ? ObjectHitResult.Discard : ObjectHitResult.Miss;
        }

        #endregion
    }
}