/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using UnityEngine;
using UnityEditor;
using TouchScript.Debugging.Loggers;

namespace TouchScript.Debugging
{
    public class TouchScriptDebugger
    {

        public static TouchScriptDebugger Instance
        {
            get
            {
                if (!Application.isPlaying) return null;
                if (instance == null)
                {
                    instance = new TouchScriptDebugger();
                }
                return instance;
            }
        }

        private static TouchScriptDebugger instance;

        public IPointerLogger PointerLogger
        {
            get { return pointerLogger; }
        }

        private IPointerLogger pointerLogger;

        public TouchScriptDebugger()
        {
            pointerLogger = new PointerLogger();
        }

    }
}

#endif