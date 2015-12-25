/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using UnityEngine;
using TouchScript.InputSources.InputHandlers;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Input source which transfers mouse clicks to touches.
    /// </summary>
    [System.Obsolete("MouseInput is deprecated! Please use StandardInput instead.")]
    public sealed class MouseInput : InputSource
    {
        #region Public properties

        /// <summary>
        /// Indicates if this input source should be disabled on mobile platforms.
        /// </summary>
        /// <remarks>
        /// Operation Systems which support touch input send first touches as mouse clicks which may result in duplicated touch points in exactly the same coordinates. This affects clusters and multitouch gestures.
        /// </remarks>
        [ToggleLeft]
        public bool DisableOnMobilePlatforms = true;

        /// <summary>
        /// Tags added to touches coming from this input.
        /// </summary>
        public Tags Tags = new Tags(Tags.INPUT_MOUSE);

        #endregion

        #region Private variables

        private MouseHandler mouseHandler;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void UpdateInput()
        {
            base.UpdateInput();

            mouseHandler.Update();
        }

        /// <inheritdoc />
        public override void CancelTouch(TouchPoint touch, bool @return)
        {
            base.CancelTouch(touch, @return);

            if (mouseHandler != null) mouseHandler.CancelTouch(touch, @return);
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            Debug.LogWarning("[TouchScript] MouseInput is deprecated! Please use StandardInput instead.");

            if (DisableOnMobilePlatforms)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.WP8Player:
                    case RuntimePlatform.MetroPlayerARM:
                    case RuntimePlatform.MetroPlayerX64:
                    case RuntimePlatform.MetroPlayerX86:
                    case RuntimePlatform.TizenPlayer:
                    case RuntimePlatform.BlackBerryPlayer:
                        // don't need mouse here
                        enabled = false;
                        return;
                }
            }

            mouseHandler = new MouseHandler(Tags, beginTouch, moveTouch, endTouch, cancelTouch);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            mouseHandler.Dispose();
            mouseHandler = null;

            base.OnDisable();
        }

        #endregion
    }
}