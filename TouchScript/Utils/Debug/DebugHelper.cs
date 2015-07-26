/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

#if DEBUG

namespace TouchScript.Utils.Debug
{
    public static class DebugHelper
    {
        public static int GetDebugId(Object obj)
        {
            return (obj.GetInstanceID() >> 10) << 10;
        }
    }
}

#endif