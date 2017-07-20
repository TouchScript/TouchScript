/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using TouchScript.Debugging.Loggers;

namespace TouchScript.Debugging.Filters
{
    public class PointerLogFilter : IPointerLogFilter
    {

        public uint EventMask { get; set; }

        public PointerLogFilter()
        {
            EventMask = uint.MaxValue;
        }

        public bool Applies(ref PointerLog log)
        {
            var evt = (int)log.Event;
            if ((EventMask & (uint)(1 << evt)) == 0) return false;

            return true;
        }

    }
}

#endif