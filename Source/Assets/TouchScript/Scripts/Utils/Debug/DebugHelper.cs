/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

#if TOUCHSCRIPT_DEBUG

namespace TouchScript.Utils.Debug
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