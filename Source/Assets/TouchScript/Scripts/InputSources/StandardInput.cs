/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
#endif
using TouchScript.InputSources.InputHandlers;
using TouchScript.Pointers;
using TouchScript.Utils.Attributes;
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
        public Windows8APIType Windows8API
        {
            get { return windows8API; }
        }

        /// <summary>
        /// Pointer API to use on Windows 7.
        /// </summary>
        public Windows7APIType Windows7API
        {
            get { return windows7API; }
        }

        /// <summary>
        /// Initialize touch input in WebGL or not.
        /// </summary>
        public bool WebGLTouch
        {
            get { return webGLTouch; }
        }

        /// <summary>
        /// Initialize mouse input on Windows 8+ or not.
        /// </summary>
        public bool Windows8Mouse
        {
            get { return windows8Mouse; }
        }

        /// <summary>
        /// Initialize mouse input on Windows 7 or not.
        /// </summary>
        public bool Windows7Mouse
        {
            get { return windows7Mouse; }
        }

        /// <summary>
        /// Initialize mouse input on UWP or not.
        /// </summary>
        public bool UniversalWindowsMouse
        {
            get { return universalWindowsMouse; }
        }

        /// <summary>
        /// Use emulated second mouse pointer with ALT or not.
        /// </summary>
        public bool EmulateSecondMousePointer
        {
            get { return emulateSecondMousePointer; }
            set
            {
                emulateSecondMousePointer = value;
                if (mouseHandler != null) mouseHandler.EmulateSecondMousePointer = value;
            }
        }

        #endregion

        #region Private variables

        private static StandardInput instance;

#pragma warning disable CS0414

		[SerializeField]
        [HideInInspector]
        private bool generalProps; // Used in the custom inspector

        [SerializeField]
        [HideInInspector]
        private bool windowsProps; // Used in the custom inspector

		[SerializeField]
		[HideInInspector]
		private bool webglProps; // Used in the custom inspector

#pragma warning restore CS0414

		[SerializeField]
        private Windows8APIType windows8API = Windows8APIType.Windows8;

        [SerializeField]
        private Windows7APIType windows7API = Windows7APIType.Windows7;

        [ToggleLeft]
        [SerializeField]
        private bool webGLTouch = true;

        [ToggleLeft]
        [SerializeField]
        private bool windows8Mouse = true;

        [ToggleLeft]
        [SerializeField]
        private bool windows7Mouse = true;

        [ToggleLeft]
        [SerializeField]
        private bool universalWindowsMouse = true;

        [ToggleLeft]
        [SerializeField]
        private bool emulateSecondMousePointer = true;

        private MouseHandler mouseHandler;
        private TouchHandler touchHandler;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private Windows8PointerHandler windows8PointerHandler;
        private Windows7PointerHandler windows7PointerHandler;
#endif

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool UpdateInput()
        {
            if (base.UpdateInput()) return true;

            var handled = false;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windows8PointerHandler != null) 
            {
                handled = windows8PointerHandler.UpdateInput();
            } 
            else
            {
                if (windows7PointerHandler != null) 
                {
                    handled = windows7PointerHandler.UpdateInput();
                }
                else 
#endif
            if (touchHandler != null)
            {
                handled = touchHandler.UpdateInput();
            }
            if (mouseHandler != null)
            {
                if (handled) mouseHandler.CancelMousePointer();
                else handled = mouseHandler.UpdateInput();
            }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            }
#endif
            return handled;
        }

        /// <inheritdoc />
        public override void UpdateResolution()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windows8PointerHandler != null) windows8PointerHandler.UpdateResolution();
            else if (windows7PointerHandler != null) windows7PointerHandler.UpdateResolution();
#endif
            if (touchHandler != null) touchHandler.UpdateResolution();
            if (mouseHandler != null) mouseHandler.UpdateResolution();
        }

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            base.CancelPointer(pointer, shouldReturn);

            var handled = false;
            if (touchHandler != null) handled = touchHandler.CancelPointer(pointer, shouldReturn);
            if (mouseHandler != null && !handled) handled = mouseHandler.CancelPointer(pointer, shouldReturn);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windows7PointerHandler != null && !handled) handled = windows7PointerHandler.CancelPointer(pointer, shouldReturn);
            if (windows8PointerHandler != null && !handled) handled = windows8PointerHandler.CancelPointer(pointer, shouldReturn);
#endif

            return handled;
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void OnEnable()
        {
            if (instance != null) Destroy(instance);
            instance = this;

            base.OnEnable();

            Input.simulateMouseWithTouches = false;

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
            if (CoordinatesRemapper != null) updateCoordinatesRemapper(CoordinatesRemapper);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            disableMouse();
            disableTouch();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            disableWindows7Touch();
            disableWindows8Touch();
#endif

            base.OnDisable();
        }

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
			basicEditor = true;
		}

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void updateCoordinatesRemapper(ICoordinatesRemapper remapper)
        {
            base.updateCoordinatesRemapper(remapper);
            if (mouseHandler != null) mouseHandler.CoordinatesRemapper = remapper;
            if (touchHandler != null) touchHandler.CoordinatesRemapper = remapper;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windows7PointerHandler != null) windows7PointerHandler.CoordinatesRemapper = remapper;
            if (windows8PointerHandler != null) windows8PointerHandler.CoordinatesRemapper = remapper;
#endif
        }

        #endregion

        #region Private functions

        private void enableMouse()
        {
            mouseHandler = new MouseHandler(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer);
            mouseHandler.EmulateSecondMousePointer = emulateSecondMousePointer;
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
            touchHandler = new TouchHandler(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer);
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
        private void enableWindows7Touch()
        {
            windows7PointerHandler = new Windows7PointerHandler(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer);
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
            windows8PointerHandler = new Windows8PointerHandler(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer);
            windows8PointerHandler.MouseInPointer = windows8Mouse;
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