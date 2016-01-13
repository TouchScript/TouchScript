/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
#endif
using TouchScript.InputSources.InputHandlers;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes standard input events (mouse, touch, pen) on all platforms.
    /// Initializes proper inputs automatically. Replaces old Mobile and Mouse inputs.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Standard Input")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_InputSources_StandardInput.htm")]
    public sealed class StandardInput : InputSource
    {
        #region Constants

        /// <summary>
        /// Touch API to use on Windows 8 and later OS versions.
        /// </summary>
        public enum Windows8TouchAPIType
        {
            /// <summary>
            /// Windows 8 WM_POINTER API.
            /// </summary>
            Windows8,

            /// <summary>
            /// Windows 7 WM_TOUCH API.
            /// </summary>
            Windows7,

            /// <summary>
            /// Built-in Unity 5 WM_TOUCH implementation.
            /// </summary>
            Unity,

            /// <summary>
            /// Don't initialize touch input at all.
            /// </summary>
            None
        }

        /// <summary>
        /// Touch API to use on Windows 7.
        /// </summary>
        public enum Windows7TouchAPIType
        {
            /// <summary>
            /// Windows 7 WM_TOUCH API.
            /// </summary>
            Windows7,

            /// <summary>
            /// Built-in Unity 5 WM_TOUCH implementation.
            /// </summary>
            Unity,

            /// <summary>
            /// Don't initialize touch input at all.
            /// </summary>
            None
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private static readonly Version WIN7_VERSION = new Version(6, 1, 0, 0);
        private static readonly Version WIN8_VERSION = new Version(6, 2, 0, 0);
#endif

        #endregion

        #region Public properties

        /// <summary>
        /// Tags added to touches coming from this input.
        /// </summary>
        public Tags TouchTags = new Tags(Tags.INPUT_TOUCH);

        /// <summary>
        /// Tags added to mouse touches coming from this input.
        /// </summary>
        public Tags MouseTags = new Tags(Tags.INPUT_MOUSE);

        /// <summary>
        /// Tags added to pen touches coming from this input.
        /// </summary>
        public Tags PenTags = new Tags(Tags.INPUT_PEN);

        /// <summary>
        /// Touch API to use on Windows 8.
        /// </summary>
        public Windows8TouchAPIType Windows8Touch = Windows8TouchAPIType.Windows8;

        /// <summary>
        /// Touch API to use on Windows 7.
        /// </summary>
        public Windows7TouchAPIType Windows7Touch = Windows7TouchAPIType.Windows7;

        /// <summary>
        /// Initialize touch input in WebPlayer or not.
        /// </summary>
        public bool WebPlayerTouch = true;

        /// <summary>
        /// Initialize touch input in WebGL or not.
        /// </summary>
        public bool WebGLTouch = true;

        /// <summary>
        /// Initialize mouse input on Windows 8+ or not.
        /// </summary>
        public bool Windows8Mouse = true;

        /// <summary>
        /// Initialize mouse input on Windows 7 or not.
        /// </summary>
        public bool Windows7Mouse = true;

        /// <summary>
        /// Initialize mouse input on UWP or not.
        /// </summary>
        public bool UniversalWindowsMouse = true;

        #endregion

        #region Private variables

        private MouseHandler mouseHandler;
        private TouchHandler touchHandler;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private Windows8MouseHandler windows8MouseHandler;
        private Windows8TouchHandler windows8TouchHandler;
        private Windows7TouchHandler windows7TouchHandler;
#endif

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void UpdateInput()
        {
            base.UpdateInput();

            if (touchHandler != null)
            {
                touchHandler.Update();
                // Unity adds mouse events from touches resulting in duplicated pointers.
                // Don't update mouse if touch input is present.
                if (mouseHandler != null)
                {
                    if (touchHandler.HasTouches) mouseHandler.EndTouches();
                    else mouseHandler.Update();
                }
            }
            else if (mouseHandler != null) mouseHandler.Update();
        }

        /// <inheritdoc />
        public override void CancelTouch(TouchPoint touch, bool @return)
        {
            base.CancelTouch(touch, @return);

            var handled = false;
            if (touchHandler != null) handled = touchHandler.CancelTouch(touch, @return);
            if (mouseHandler != null && !handled) handled = mouseHandler.CancelTouch(touch, @return);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windows7TouchHandler != null && !handled) handled = windows7TouchHandler.CancelTouch(touch, @return);
            if (windows8TouchHandler != null && !handled) windows8TouchHandler.CancelTouch(touch, @return);
#endif
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

#if UNITY_EDITOR
            enableTouch();
            enableMouse();
#else
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            enableMouse();
#elif UNITY_STANDALONE_WIN
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (Environment.OSVersion.Version >= WIN8_VERSION)
                {
                    // Windows 8+
                    switch (Windows8Touch)
                    {
                        case Windows8TouchAPIType.Windows8:
                            enableWindows8Touch();
                            if (Windows8Mouse) enableWindows8Mouse();
                            break;
                        case Windows8TouchAPIType.Windows7:
                            enableWindows7Touch();
                            if (Windows8Mouse) enableMouse();
                            break;
                        case Windows8TouchAPIType.Unity:
                            enableTouch();
                            if (Windows8Mouse) enableMouse();
                            break;
                        case Windows8TouchAPIType.None:
                            enableMouse();
                            break;
                    }
                }
                else if (Environment.OSVersion.Version >= WIN7_VERSION)
                {
                    // Windows 7
                    switch (Windows7Touch)
                    {
                        case Windows7TouchAPIType.Windows7:
                            enableWindows7Touch();
                            if (Windows7Mouse) enableMouse();
                            break;
                        case Windows7TouchAPIType.Unity:
                            enableTouch();
                            if (Windows7Mouse) enableMouse();
                            break;
                        case Windows7TouchAPIType.None:
                            enableMouse();
                            break;
                    }
                }
                else
                {
                    // Some other earlier Windows
                    enableMouse();
                }
            }
            else
            {
                // Some other earlier Windows
                enableMouse();
            }
#elif UNITY_WEBPLAYER
            if (WebPlayerTouch) enableTouch();
            enableMouse();
#elif UNITY_WEBGL
            if (WebGLTouch) enableTouch();
            enableMouse();
#elif UNITY_WSA || UNITY_WSA_8_0 || UNITY_WSA_8_1 || UNITY_WSA_10_0
            enableTouch();
            if (UniversalWindowsMouse) enableMouse();
#elif UNITY_PS3 || UNITY_PS4 || UNITY_XBOX360 || UNITY_XBOXONE
            enableMouse();
#else // UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_BLACKBERRY || UNITY_TIZEN || UNITY_WP8 || UNITY_WP8_1
            enableTouch();
#endif
#endif
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            disableMouse();
            disableTouch();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            disableWindows8Mouse();
            disableWindows7Touch();
            disableWindows8Touch();
#endif

            base.OnDisable();
        }

        #endregion

        #region Private functions

        private void enableMouse()
        {
            mouseHandler = new MouseHandler(MouseTags, beginTouch, moveTouch, endTouch, cancelTouch);
            Debug.Log("[TouchScript] Initialized Unity mouse input.");
        }

        private void disableMouse()
        {
            if (mouseHandler != null)
            {
                mouseHandler.Dispose();
                mouseHandler = null;
            }
        }

        private void enableTouch()
        {
            touchHandler = new TouchHandler(TouchTags, beginTouch, moveTouch, endTouch, cancelTouch);
            Debug.Log("[TouchScript] Initialized Unity touch input.");
        }

        private void disableTouch()
        {
            if (touchHandler != null)
            {
                touchHandler.Dispose();
                touchHandler = null;
            }
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private void enableWindows8Mouse()
        {
            windows8MouseHandler = new Windows8MouseHandler();
            Debug.Log("[TouchScript] Initialized Windows 8 mouse input.");
        }

        private void disableWindows8Mouse()
        {
            if (windows8MouseHandler != null)
            {
                windows8MouseHandler.Dispose();
                windows8MouseHandler = null;
            }
        }

        private void enableWindows7Touch()
        {
            windows7TouchHandler = new Windows7TouchHandler(TouchTags, beginTouch, moveTouch, endTouch, cancelTouch);
            Debug.Log("[TouchScript] Initialized Windows 7 touch input.");
        }

        private void disableWindows7Touch()
        {
            if (windows7TouchHandler != null)
            {
                windows7TouchHandler.Dispose();
                windows7TouchHandler = null;
            }
        }

        private void enableWindows8Touch()
        {
            windows8TouchHandler = new Windows8TouchHandler(TouchTags, MouseTags, PenTags, beginTouch, moveTouch, endTouch, cancelTouch);
            Debug.Log("[TouchScript] Initialized Windows 8 touch input.");
        }

        private void disableWindows8Touch()
        {
            if (windows8TouchHandler != null)
            {
                windows8TouchHandler.Dispose();
                windows8TouchHandler = null;
            }
        }
#endif

        #endregion
    }
}