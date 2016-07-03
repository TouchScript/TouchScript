/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
#endif
using TouchScript.InputSources.InputHandlers;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes standard input events (mouse, pointer, pen) on all platforms.
    /// Initializes proper inputs automatically. Replaces old Mobile and Mouse inputs.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Standard Input")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_InputSources_StandardInput.htm")]
    public sealed class StandardInput : InputSource
    {
        #region Constants

        /// <summary>
        /// Pointer API to use on Windows 8 and later OS versions.
        /// </summary>
        public enum Windows8APIType
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
            /// Don't initialize pointer input at all.
            /// </summary>
            None
        }

        /// <summary>
        /// Pointer API to use on Windows 7.
        /// </summary>
        public enum Windows7APIType
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
            /// Don't initialize pointer input at all.
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
        /// Pointer API to use on Windows 8.
        /// </summary>
        public Windows8APIType Windows8API = Windows8APIType.Windows8;

        /// <summary>
        /// Pointer API to use on Windows 7.
        /// </summary>
        public Windows7APIType Windows7API = Windows7APIType.Windows7;

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
        private Windows8PointerHandler windows8PointerHandler;
        private Windows7PointerHandler windows7PointerHandler;
#endif

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void UpdateInput()
        {
            base.UpdateInput();

            if (touchHandler != null)
            {
                touchHandler.UpdateInput();
                // Unity adds mouse events from touches resulting in duplicated pointers.
                // Don't update mouse if pointer input is present.
                if (mouseHandler != null)
                {
                    if (touchHandler.HasPointers) mouseHandler.EndPointers();
                    else mouseHandler.UpdateInput();
                }
            }
            else if (mouseHandler != null) mouseHandler.UpdateInput();
        }

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool @return)
        {
            base.CancelPointer(pointer, @return);

            var handled = false;
            if (touchHandler != null) handled = touchHandler.CancelPointer(pointer, @return);
            if (mouseHandler != null && !handled) handled = mouseHandler.CancelPointer(pointer, @return);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windows7PointerHandler != null && !handled) handled = windows7PointerHandler.CancelPointer(pointer, @return);
            if (windows8PointerHandler != null && !handled) handled = windows8PointerHandler.CancelPointer(pointer, @return);
#endif

            return handled;
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
                    switch (Windows8API)
                    {
                        case Windows8APIType.Windows8:
                            enableWindows8Touch();
                            if (Windows8Mouse) enableWindows8Mouse();
                            break;
                        case Windows8APIType.Windows7:
                            enableWindows7Touch();
                            if (Windows8Mouse) enableMouse();
                            break;
                        case Windows8APIType.Unity:
                            enableTouch();
                            if (Windows8Mouse) enableMouse();
                            break;
                        case Windows8APIType.None:
                            enableMouse();
                            break;
                    }
                }
                else if (Environment.OSVersion.Version >= WIN7_VERSION)
                {
                    // Windows 7
                    switch (Windows7API)
                    {
                        case Windows7APIType.Windows7:
                            enableWindows7Touch();
                            if (Windows7Mouse) enableMouse();
                            break;
                        case Windows7APIType.Unity:
                            enableTouch();
                            if (Windows7Mouse) enableMouse();
                            break;
                        case Windows7APIType.None:
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
            mouseHandler = new MouseHandler(beginPointer, movePointer, endPointer, cancelPointer);
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
            touchHandler = new TouchHandler(beginPointer, movePointer, endPointer, cancelPointer);
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
            windows7PointerHandler = new Windows7PointerHandler(beginPointer, movePointer, endPointer, cancelPointer);
            Debug.Log("[TouchScript] Initialized Windows 7 pointer input.");
        }

        private void disableWindows7Touch()
        {
            if (windows7PointerHandler != null)
            {
                windows7PointerHandler.Dispose();
                windows7PointerHandler = null;
            }
        }

        private void enableWindows8Touch()
        {
            windows8PointerHandler = new Windows8PointerHandler(beginPointer, movePointer, endPointer, cancelPointer);
            Debug.Log("[TouchScript] Initialized Windows 8 pointer input.");
        }

        private void disableWindows8Touch()
        {
            if (windows8PointerHandler != null)
            {
                windows8PointerHandler.Dispose();
                windows8PointerHandler = null;
            }
        }
#endif

#endregion
    }
}