/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using TouchScript.Debugging.Loggers;
using UnityEngine;

namespace TouchScript.Debugging.Filters
{
    public interface IPointerLogFilter
    {
        bool Applies(ref PointerLog log);
    }
}

#endif