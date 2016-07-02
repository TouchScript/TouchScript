/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using TouchScript.Pointers;
using TouchScript.InputSources.InputHandlers;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Input source which transforms mouse to pointer.
    /// </summary>
    [System.Obsolete("MouseInput is deprecated! Please use StandardInput instead.")]
    public sealed class MouseInput : InputSource
    {
        #region Public properties

        /// <summary>
        /// Indicates if this input source should be disabled on mobile platforms.
        /// </summary>
        /// <remarks>
        /// Operation Systems which support touch input send first touches as mouse clicks which may result in duplicated pointer points in exactly the same coordinates. This affects clusters and multitouch gestures.
        /// </remarks>
        [ToggleLeft]
        public bool DisableOnMobilePlatforms = true;

        /// <summary>
        /// Tags added to pointers coming from this input.
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

            mouseHandler.UpdateInput();
        }

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool @return)
        {
            if (mouseHandler != null) return mouseHandler.CancelPointer(pointer, @return);
            return base.CancelPointer(pointer, @return);
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

            mouseHandler = new MouseHandler(Tags, beginPointer, movePointer, endPointer, cancelPointer);
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