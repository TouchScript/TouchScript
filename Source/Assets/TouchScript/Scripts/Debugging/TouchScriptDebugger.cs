/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using UnityEngine;
using TouchScript.Debugging.Loggers;

namespace TouchScript.Debugging
{
    /// <summary>
    /// A set of debugging tools for TouchScript.
    /// </summary>
    public class TouchScriptDebugger
    {
        /// <summary>
        /// The singleton instance of the debugger.
        /// </summary>
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

        /// <summary>
        /// Current logger to record pointer events.
        /// </summary>
        public IPointerLogger PointerLogger
        {
            get { return pointerLogger; }
        }

        private static TouchScriptDebugger instance;
        private IPointerLogger pointerLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchScriptDebugger"/> class.
        /// </summary>
        public TouchScriptDebugger()
        {
            pointerLogger = new PointerLogger();
        }
    }
}

#endif