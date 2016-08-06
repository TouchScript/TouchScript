/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.InputSources;
using UnityEngine;

namespace TouchScript.Pointers
{
    public interface IPointer
    {
        /// <summary>
        /// Internal unique pointer id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Pointer type. See <see cref="Pointer.PointerType"/>.
        /// </summary>
        Pointer.PointerType Type { get; }

        /// <summary>
        /// Original input source which created this pointer.
        /// <seealso cref="IInputSource"/>
        /// </summary>
        IInputSource InputSource { get; }

        /// <summary>
        /// <para>Current position in screen coordinates.</para>
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// <para>Gets or sets pointer flags: <see cref="FLAG_ARTIFICIAL"/>, <see cref="FLAG_FIRST_BUTTON"/>, <see cref="FLAG_SECOND_BUTTON"/>, <see cref="FLAG_THIRD_BUTTON"/>, <see cref="FLAG_INCONTACT"/>.</para>
        /// <para>Note: setting this property doesn't immediately change its value, the value actually changes during the next TouchManager update phase.</para>
        /// </summary>
        uint Flags { get; set; }

        /// <summary>
        /// Returns <see cref="HitData"/> for current pointer position, i.e. what is right beneath it. Caches the result for the entire frame.
        /// </summary>
        /// <param name="forceRecalculate">if set to <c>true</c> forces to recalculate the value.</param>
        HitData GetOverData(bool forceRecalculate = false);
    }
}