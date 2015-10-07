/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if DEBUG
using TouchScript.Utils.Attributes;
#endif
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// A debuggable component. When built in Debug mode has a checkbox to turn debug information on and off.
    /// </summary>
    public class DebuggableMonoBehaviour : MonoBehaviour, IDebuggable
    {
        /// <inheritdoc />
        public bool DebugMode
        {
            get
            {
#if DEBUG
                return debugMode;
#else
                return false;
#endif
            }
            set
            {
#if DEBUG
                debugMode = value;
#endif
            }
        }

#if DEBUG
        [SerializeField]
        [ToggleLeft]
        private bool debugMode = false;
#endif
    }
}