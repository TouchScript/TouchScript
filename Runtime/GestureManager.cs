/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Core;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Facade for current instance of <see cref="IGestureManager"/>.
    /// </summary>
    /// <remarks>
    /// <para>Why IList instead of Pointer in pointer events?</para>
    /// <para>Right now touchesBegan/touchesMoved/touchesEnded methods in Gesture class accept IList as their argument which seems to overcomplicate a lot of stuff and just calling touchBegan(TouchPoint) would be easier.</para>
    /// <para>The later approach was tried in 7.0 and reverted in 8.0 since it introduced a really hard to fix gesture priority issue. If with lists a gesture knows all touches changed during current frame, individual touchMoved calls have to be buffered till the end of frame. But there's no way to execute gesture recognition logic at the end of frame in the right hierarchical order. This concern resulted in the following issue: https://github.com/TouchScript/TouchScript/issues/203
    /// </para>
    /// </remarks>
    public sealed class GestureManager : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Gets the GestureManager instance.
        /// </summary>
        public static IGestureManager Instance
        {
            get { return GestureManagerInstance.Instance; }
        }

        #endregion
    }
}