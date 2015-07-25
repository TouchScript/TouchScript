/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript
{
    public class DebuggableMonoBehaviour : MonoBehaviour, IDebuggable
    {

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
        private bool debugMode = true;
#endif

    }
}
