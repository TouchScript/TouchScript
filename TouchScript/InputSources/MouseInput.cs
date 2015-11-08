/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Input source which transfers mouse clicks to touches.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Mouse Input")]
    public sealed class MouseInput : InputSource
    {
        
        #region Public properties

        /// <summary>
        /// Indicates if this input source should be disabled on mobile platforms.
        /// </summary>
        /// <remarks>
        /// Operation Systems which support touch input send first touches as mouse clicks which may result in duplicated touch points in exactly the same coordinates. This affects clusters and multitouch gestures.
        /// </remarks>
        [ToggleLeft] public bool DisableOnMobilePlatforms = true;

        /// <summary>
        /// Tags added to touches coming from this input.
        /// </summary>
        public Tags Tags = new Tags(Tags.INPUT_MOUSE);

        #endregion

        #region Private variables

        private MouseHandler mouseHandler;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            Debug.LogWarning("MouseInput is deprecated. Please use StandaloneInput.");

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

            mouseHandler = new MouseHandler((p) => beginTouch(p, new Tags(Tags)), moveTouch, endTouch, cancelTouch);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            mouseHandler.Destroy();
            mouseHandler = null;

            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();

            mouseHandler.Update();
        }

        #endregion

    }
}
