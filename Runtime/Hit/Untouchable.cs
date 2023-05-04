/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// Makes an object it is attached to untouchable, i.e. it completely ignores all pointers landing on it.
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/Untouchable")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Hit_Untouchable.htm")]
    public class Untouchable : HitTest
    {
        #region Public properties

        /// <summary>
        /// Indicates if instead of not reacting to pointers the object should completely discard them making it impossible for other gestures to receive them.
        /// </summary>
        /// <value> If <c>true</c> pointers are not only prevented but discarded making it impossible for other gestures to receive them. </value>
        public bool DiscardPointer = false;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override HitResult IsHit(IPointer pointer, HitData hit)
        {
            return DiscardPointer ? HitResult.Discard : HitResult.Miss;
        }

        #endregion
    }
}