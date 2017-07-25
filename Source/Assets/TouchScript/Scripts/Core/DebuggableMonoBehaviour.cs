/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG
using TouchScript.Utils.Attributes;
#endif
using UnityEngine;

namespace TouchScript.Core
{
    /// <summary>
    /// A debuggable component. When built with TOUCHSCRIPT_DEBUG define has a checkbox to turn debug information on and off.
    /// </summary>
    public class DebuggableMonoBehaviour : MonoBehaviour, IDebuggable
    {
        /// <inheritdoc />
        public virtual bool DebugMode
        {
            get
            {
#if TOUCHSCRIPT_DEBUG
                return debugMode;
#else
                return false;
#endif
            }
            set
            {
#if TOUCHSCRIPT_DEBUG
                debugMode = value;
#endif
            }
        }

#if TOUCHSCRIPT_DEBUG
        [SerializeField]
        [ToggleLeft]
        private bool debugMode = false;
#endif
    }
}