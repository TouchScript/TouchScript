/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using UnityEngine;
using TouchScript.InputSources.InputHandlers;

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
        /// Indicates if this input source should be disabled on platforms which don't support touch input with Input.Touches.
        /// </summary>
        [ToggleLeft]
        public bool DisableOnNonTouchPlatforms = true;

        /// <summary>
        /// Tags added to touches coming from this input.
        /// </summary>
        public Tags Tags = new Tags(Tags.INPUT_TOUCH);

        #endregion

        #region Private variables

        private TouchHandler touchHandler;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void UpdateInput()
        {
            base.UpdateInput();

            if (touchHandler != null) touchHandler.Update();
        }

        /// <inheritdoc />
        public override void CancelTouch(TouchPoint touch, bool @return)
        {
            base.CancelTouch(touch, @return);

            if (touchHandler != null) touchHandler.CancelTouch(touch, @return);
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
                        // don't need mobile touch here
                        enabled = false;
                        return;
                }
            }

            touchHandler = new TouchHandler(Tags, beginTouch, moveTouch, endTouch, cancelTouch);

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