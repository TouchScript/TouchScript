/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System.Collections.Generic;
using TouchScript.Debugging.Filters;
using UnityEngine;
using TouchScript.Debugging.Loggers;
using TouchScript.Pointers;

namespace TouchScript.Debugging
{
    /// <summary>
    /// A set of debugging tools for TouchScript.
    /// </summary>
    public class TouchScriptDebugger : ScriptableObject
    {
        /// <summary>
        /// The singleton instance of the debugger.
        /// </summary>
        public static TouchScriptDebugger Instance
        {
            get
            {
                if (instance == null)
                {
                    var objs = Resources.FindObjectsOfTypeAll<TouchScriptDebugger>();
                    if (objs.Length > 0) instance = objs[0];
                    else
                    {
                        instance = CreateInstance<TouchScriptDebugger>();
                        instance.hideFlags = HideFlags.HideAndDontSave;
                    }
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
            set
            {
                if (value == null) return;
                if (pointerLogger == value) return;
                pointerLogger.Dispose();
                pointerLogger = value;
            }
        }

        private static TouchScriptDebugger instance;
        private IPointerLogger pointerLogger;

        public void ClearPointerLogger()
        {
            if (Application.isEditor)
                pointerLogger = new DummyLogger();
            else
                pointerLogger = new FileWriterLogger();
        }

        private void OnEnable()
        {
            if (pointerLogger == null) ClearPointerLogger();
        }

        private void OnDisable()
        {
            if (pointerLogger != null) pointerLogger.Dispose();
        }

        private class DummyLogger : IPointerLogger
        {
            public int PointerCount
            {
                get { return 0; }
            }

            public void Log(Pointer pointer, PointerEvent evt) {}

            public List<PointerData> GetFilteredPointerData(IPointerDataFilter filter = null)
            {
                return new List<PointerData>();
            }

            public List<PointerLog> GetFilteredLogsForPointer(int id, IPointerLogFilter filter = null)
            {
                return new List<PointerLog>();
            }

            public void Dispose() {}
        }
    }
}

#endif