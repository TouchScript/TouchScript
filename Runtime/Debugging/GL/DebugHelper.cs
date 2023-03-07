/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using UnityEngine;

namespace TouchScript.Debugging.GL
{
    public static class DebugHelper
    {
        public static int GetDebugId(Object obj)
        {
            return int.MinValue + (obj.GetInstanceID() << 10);
        }
    }
}

#endif