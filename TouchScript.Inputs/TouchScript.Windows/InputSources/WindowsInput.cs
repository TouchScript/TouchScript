/*
 * @author Valentin Simonov / http://va.lent.in/
 * @author Valentin Frolov
 * @author Andrew David Griffiths
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    [AddComponentMenu("TouchScript/Input Sources/Windows Input")]
    public sealed class WindowsInput : InputSource
    {

        #region Constants

        private static readonly Version WIN8_VERSION = new Version(6, 2, 9200, 0);
        private const string PRESS_AND_HOLD_ATOM = "MicrosoftTabletPenServiceProperty";

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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

        #endregion

        #region Private variables

        private IntPtr hMainWindow;
        private IntPtr oldWndProcPtr;
        private IntPtr newWndProcPtr;

        private WndProcDelegate newWndProc;
        private ushort pressAndHoldAtomID;
        private int touchInputSize;

        private MouseHandler mouseHandler;
        private Dictionary<int, int> winToInternalId = new Dictionary<int, int>();
        private bool isInitialized = false;

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            if (Application.isEditor)
            {
                mouseHandler = new MouseHandler((p) => beginTouch(p, new Tags(MouseTags)), moveTouch, endTouch, cancelTouch);
                return;
            }

            if (Application.platform != RuntimePlatform.WindowsPlayer)
            {
                enabled = false;
                return;
            }

            init();
        }

        protected override void Update()
        {
            base.Update();

            Debug.Log("Update started at " + Time.time);
            if (mouseHandler != null) mouseHandler.Update();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            if (isInitialized)
            {
                if (pressAndHoldAtomID != 0)
                {
                    RemoveProp(hMainWindow, PRESS_AND_HOLD_ATOM);
                    GlobalDeleteAtom(pressAndHoldAtomID);
                }

                SetWindowLongPtr(hMainWindow, -4, oldWndProcPtr);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= WIN8_VERSION)
                {
                    EnableMouseInPointer(false);
                }
                else
                {
                    UnregisterTouchWindow(hMainWindow);
                }

                hMainWindow = IntPtr.Zero;
                oldWndProcPtr = IntPtr.Zero;
                newWndProcPtr = IntPtr.Zero;

                newWndProc = null;
            }

            foreach (var i in winToInternalId)
            {
                cancelTouch(i.Value);
            }

            if (mouseHandler != null)
            {
                mouseHandler.Destroy();
                mouseHandler = null;
            }

            base.OnDisable();
        }

        #endregion

        #region Private functions

        private void init()
        {
            hMainWindow = GetForegroundWindow();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= WIN8_VERSION)
            {
                newWndProc = wndProcWin8;
                EnableMouseInPointer(true);
            }
            else
            {
                newWndProc = wndProcWin7;
                touchInputSize = Marshal.SizeOf(typeof(TOUCHINPUT));
                RegisterTouchWindow(hMainWindow, 0);

                Debug.Log("Initializing mouse handling code for Windows 7.");
                mouseHandler = new MouseHandler(
                    (p) =>
                    {
                        var id = beginTouch(p, new Tags(MouseTags));
                        Debug.Log(string.Format("Mouse input at {0} with id {1} detected.", p, id));
                        return id;
                    },
                    (i, p) =>
                    {
                        moveTouch(i, p);
                        Debug.Log(string.Format("Mouse cursor with id {1} moved to {0}.", p, i));
                    },
                    (i) =>
                    {
                        endTouch(i);
                        Debug.Log(string.Format("Mouse cursor with id {0} removed.", i));
                    }, cancelTouch);
            }

            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);

            pressAndHoldAtomID = GlobalAddAtom(PRESS_AND_HOLD_ATOM);
            SetProp(hMainWindow, PRESS_AND_HOLD_ATOM, 1);

            isInitialized = true;
        }

        private IntPtr wndProcWin7(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_TOUCH:
                    decodeWin7Touches(wParam, lParam);
                    break;
                case WM_CLOSE:
                    UnregisterTouchWindow(hWnd);
                    SetWindowLongPtr(hWnd, -4, oldWndProcPtr);
                    Application.Quit();
                    return IntPtr.Zero;
            }
            return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
        }

        private IntPtr wndProcWin8(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_POINTERDOWN:
                case WM_POINTERUP:
                case WM_POINTERUPDATE:
                    decodeWin8Touches(msg, wParam, lParam);
                    break;
                case WM_CLOSE:
                    SetWindowLongPtr(hWnd, -4, oldWndProcPtr);
                    Application.Quit();
                    return IntPtr.Zero;
            }
            return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
        }

        private void decodeWin7Touches(IntPtr wParam, IntPtr lParam)
        {
            int inputCount = LOWORD(wParam.ToInt32());
            TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];

            if (!GetTouchInputInfo(lParam, inputCount, inputs, touchInputSize))
            {
                return;
            }

            for (int i = 0; i < inputCount; i++)
            {
                TOUCHINPUT touch = inputs[i];

                if ((touch.dwFlags & (int)TouchEvent.TOUCHEVENTF_DOWN) != 0)
                {
                    POINT p = new POINT();
                    p.X = touch.x / 100;
                    p.Y = touch.y / 100;
                    ScreenToClient(hMainWindow, ref p);

                    var pos = new Vector2(p.X, Screen.height - p.Y);
                    var id = beginTouch(pos, new Tags(TouchTags));
                    winToInternalId.Add(touch.dwID, id);
                    Debug.Log(string.Format("Touch input at {0} with id {1} detected.", pos, id));
                }
                else if ((touch.dwFlags & (int)TouchEvent.TOUCHEVENTF_UP) != 0)
                {
                    int existingId;
                    if (winToInternalId.TryGetValue(touch.dwID, out existingId))
                    {
                        winToInternalId.Remove(touch.dwID);
                        endTouch(existingId);
                        Debug.Log(string.Format("Touch cursor with id {0} removed.", existingId));
                    }
                }
                else if ((touch.dwFlags & (int)TouchEvent.TOUCHEVENTF_MOVE) != 0)
                {
                    int existingId;
                    if (winToInternalId.TryGetValue(touch.dwID, out existingId))
                    {
                        POINT p = new POINT();
                        p.X = touch.x / 100;
                        p.Y = touch.y / 100;
                        ScreenToClient(hMainWindow, ref p);

                        var pos = new Vector2(p.X, Screen.height - p.Y);
                        moveTouch(existingId, pos);
                        Debug.Log(string.Format("Touch cursor with id {1} moved to {0}.", pos, existingId));
                    }
                }
            }

            CloseTouchInputHandle(lParam);
        }

        private void decodeWin8Touches(uint msg, IntPtr wParam, IntPtr lParam)
        {
            int pointerId = LOWORD(wParam.ToInt32());

            POINTER_INFO pointerInfo = new POINTER_INFO();
            if (!GetPointerInfo(pointerId, ref pointerInfo))
            {
                return;
            }

            POINT p = new POINT();
            p.X = pointerInfo.ptPixelLocation.X;
            p.Y = pointerInfo.ptPixelLocation.Y;
            ScreenToClient(hMainWindow, ref p);

            int existingId;

            switch (msg)
            {
                case WM_POINTERDOWN:
                    Tags tags = null;
                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                            tags = new Tags(TouchTags);
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                            tags = new Tags(PenTags);
                            break;
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                            tags = new Tags(MouseTags);
                            break;
                    }
                    winToInternalId.Add(pointerId, beginTouch(new Vector2(p.X, Screen.height - p.Y), tags));
                    break;
                case WM_POINTERUP:
                    if (winToInternalId.TryGetValue(pointerId, out existingId))
                    {
                        winToInternalId.Remove(pointerId);
                        endTouch(existingId);
                    }
                    break;
                case WM_POINTERUPDATE:
                    if (winToInternalId.TryGetValue(pointerId, out existingId))
                    {
                        moveTouch(existingId, new Vector2(p.X, Screen.height - p.Y));
                    }
                    break;
            }
        }

        private int LOWORD(int value)
        {
            return value & 0xffff;
        }

        #endregion

        #region p/invoke

        private const int WM_CLOSE = 0x0010;
        private const int WM_TOUCH = 0x0240;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERUPDATE = 0x0245;

        private enum TouchEvent : int
        {
            TOUCHEVENTF_MOVE = 0x0001,
            TOUCHEVENTF_DOWN = 0x0002,
            TOUCHEVENTF_UP = 0x0004,
            TOUCHEVENTF_INRANGE = 0x0008,
            TOUCHEVENTF_PRIMARY = 0x0010,
            TOUCHEVENTF_NOCOALESCE = 0x0020,
            TOUCHEVENTF_PEN = 0x0040
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        private enum POINTER_INPUT_TYPE
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
        }

        private enum POINTER_BUTTON_CHANGE_TYPE
        {
            POINTER_CHANGE_NONE,
            POINTER_CHANGE_FIRSTBUTTON_DOWN,
            POINTER_CHANGE_FIRSTBUTTON_UP,
            POINTER_CHANGE_SECONDBUTTON_DOWN,
            POINTER_CHANGE_SECONDBUTTON_UP,
            POINTER_CHANGE_THIRDBUTTON_DOWN,
            POINTER_CHANGE_THIRDBUTTON_UP,
            POINTER_CHANGE_FOURTHBUTTON_DOWN,
            POINTER_CHANGE_FOURTHBUTTON_UP,
            POINTER_CHANGE_FIFTHBUTTON_DOWN,
            POINTER_CHANGE_FIFTHBUTTON_UP,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct POINTER_INFO
        {
            public POINTER_INPUT_TYPE pointerType;
            public UInt32 pointerId;
            public UInt32 frameId;
            public UInt32 pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public POINT ptPixelLocation;
            public POINT ptHimetricLocation;
            public POINT ptPixelLocationRaw;
            public POINT ptHimetricLocationRaw;
            public UInt32 dwTime;
            public UInt32 historyCount;
            public Int32 inputData;
            public UInt32 dwKeyStates;
            public UInt64 PerformanceCount;
            public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterTouchWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern void CloseTouchInputHandle(IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPointerInfo(int pointerID, ref POINTER_INFO pPointerInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr EnableMouseInPointer(bool value);

        [DllImport("Kernel32.dll")]
        static extern ushort GlobalAddAtom(string lpString);

        [DllImport("Kernel32.dll")]
        static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("user32.dll")]
        static extern int SetProp(IntPtr hWnd, string lpString, int hData);

        [DllImport("user32.dll")]
        static extern int RemoveProp(IntPtr hWnd, string lpString);

        #endregion

    }
}
