/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using TouchScript.Debugging.Loggers;

namespace TouchScript.Debugging.Filters
{
    /// <summary>
    /// An event log filter which filters data by events.
    /// </summary>
    /// <seealso cref="TouchScript.Debugging.Filters.IPointerLogFilter" />
    public class PointerLogFilter : IPointerLogFilter
    {
        /// <summary>
        /// A binary mask of events based on <see cref="PointerEvent"/> values.
        /// </summary>
        public uint EventMask { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerLogFilter"/> class.
        /// </summary>
        public PointerLogFilter()
        {
            EventMask = uint.MaxValue;
        }

        /// <inheritdoc />
        public bool Applies(ref PointerLog log)
        {
            var evt = (int) log.Event;
            return (EventMask & (uint) (1 << evt)) != 0;
        }
    }
}

#endif