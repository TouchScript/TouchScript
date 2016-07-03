/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using TouchScript.Pointers;
using TouchScript.InputSources.InputHandlers;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Mobile Input Source. Gathers touch input from built-in Unity's Input.Touches API. Though, should be used on mobile devices.
    /// </summary>
    [System.Obsolete("MobileInput is deprecated! Please use StandardInput instead.")]
    public sealed class MobileInput : InputSource
    {
        #region Public properties

        /// <summary>
        /// Indicates if this input source should be disabled on platforms which don't support pointer input with Input.Pointers.
        /// </summary>
        [ToggleLeft]
        public bool DisableOnNonTouchPlatforms = true;

        #endregion

        #region Private variables

        private TouchHandler touchHandler;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void UpdateInput()
        {
            base.UpdateInput();

            if (touchHandler != null) touchHandler.UpdateInput();
        }

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool @return)
        {
            if (touchHandler != null) return touchHandler.CancelPointer(pointer, @return);
            return base.CancelPointer(pointer, @return);
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            Debug.LogWarning("[TouchScript] MobileInput is deprecated! Please use StandardInput instead.");

            if (DisableOnNonTouchPlatforms)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.MetroPlayerARM:
                    case RuntimePlatform.MetroPlayerX64:
                    case RuntimePlatform.MetroPlayerX86:
                    case RuntimePlatform.WP8Player:
                    case RuntimePlatform.BlackBerryPlayer:
                        break;
                    default:
                        // don't need mobile pointer here
                        enabled = false;
                        return;
                }
            }

            touchHandler = new TouchHandler(beginPointer, movePointer, endPointer, cancelPointer);

            base.OnEnable();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            if (touchHandler != null)
            {
                touchHandler.Dispose();
                touchHandler = null;
            }

            base.OnDisable();
        }

        #endregion
    }
}