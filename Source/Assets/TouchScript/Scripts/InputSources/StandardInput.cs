/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.InputSources.InputHandlers;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes Windows input events: mouse, touch and pen. Works with both Windows 7 and Windows 8.
    /// Known issues:
    /// <list type="bullet">
    ///     <item>Touch and pen input doesn't work in the editor.</item>
    /// </list>
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Standalone Input")]
    public sealed class StandardInput : InputSource
    {
        #region Constants

        public enum Windows8TouchAPIType
        {
            Windows8,
            Windows7,
            Unity,
            None
        }

        public enum Windows7TouchAPIType
        {
            Windows7,
            Unity,
            None
        }

        private static readonly Version WIN7_VERSION = new Version(6, 1, 0, 0);
        private static readonly Version WIN8_VERSION = new Version(6, 2, 0, 0);

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

        public Windows8TouchAPIType Windows8Touch = Windows8TouchAPIType.Windows8;
        public Windows7TouchAPIType Windows7Touch = Windows7TouchAPIType.Windows7;
        public bool WebPlayerTouch = true;
        public bool WebGLTouch = true;
        public bool Windows8Mouse = true;
        public bool Windows7Mouse = true;
        public bool UniversalWindowsMouse = true;

        #endregion

        #region Private variables

        private MouseHandler mouseHandler;
        private TouchHandler touchHandler;
#if UNITY_STANDALONE_WIN
        private Windows8MouseHandler windows8MouseHandler;
        private Windows8TouchHandler windows8TouchHandler;
        private Windows7TouchHandler windows7TouchHandler;
#endif

        #endregion

        #region Public methods

        public override void UpdateInput()
        {
            base.UpdateInput();

            if (mouseHandler != null) mouseHandler.Update();
            if (touchHandler != null) touchHandler.Update();
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
#if UNITY_STANDALONE_WIN
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
            mouseHandler = new MouseHandler((p) => beginTouch(p, new Tags(MouseTags)), moveTouch, endTouch, cancelTouch);
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
            touchHandler = new TouchHandler((p) => beginTouch(p, new Tags(TouchTags)), moveTouch, endTouch, cancelTouch);
        }

        private void disableTouch()
        {
            if (touchHandler != null)
            {
                touchHandler.Dispose();
                touchHandler = null;
            }
        }

#if UNITY_STANDALONE_WIN
        private void enableWindows8Mouse()
        {
            windows8MouseHandler = new Windows8MouseHandler();
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
            windows7TouchHandler = new Windows7TouchHandler((p, s) => beginTouch(p, new Tags(TouchTags)), moveTouch,
                endTouch, cancelTouch);
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
            windows8TouchHandler = new Windows8TouchHandler((p, s) =>
            {
                switch (s)
                {
                    case WindowsTouchHandler.TouchSource.Touch:
                        return beginTouch(p, new Tags(TouchTags));
                    case WindowsTouchHandler.TouchSource.Pen:
                        return beginTouch(p, new Tags(PenTags));
                    default:
                        return beginTouch(p, new Tags(MouseTags));
                }
            }, moveTouch, endTouch, cancelTouch);
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