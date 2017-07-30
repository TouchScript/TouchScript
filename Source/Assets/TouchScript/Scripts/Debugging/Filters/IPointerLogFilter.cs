/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using TouchScript.Debugging.Loggers;

namespace TouchScript.Debugging.Filters
{
    /// <summary>
    /// A filter of event data for <see cref="IPointerLogger"/>.
    /// </summary>
    public interface IPointerLogFilter
    {
        /// <summary>
        /// Checks if an event should be filtered.
        /// </summary>
        /// <param name="log">The log eveny.</param>
        /// <returns><c>True</c> if the event should be filtered; <c>false</c> otherwise.</returns>
        bool Applies(ref PointerLog log);
    }
}

#endif