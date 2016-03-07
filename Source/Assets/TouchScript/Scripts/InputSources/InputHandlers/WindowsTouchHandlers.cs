/*
 * @author Valentin Simonov / http://va.lent.in/
 * @author Valentin Frolov
 * @author Andrew David Griffiths
 */

#if UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Windows 8 touch handling implementation which can be embedded to other (input) classes.
    /// </summary>
    public class Windows8TouchHandler : WindowsTouchHandler
    {

        private Tags mouseTags, touchTags, penTags;

        /// <inheritdoc />
        public Windows8TouchHandler(Tags touchTags, Tags mouseTags, Tags penTags,
            Func<Vector2, Tags, bool, TouchPoint> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch,
            Action<int> cancelTouch) : base(touchTags, beginTouch, moveTouch, endTouch, cancelTouch)
        {
            this.mouseTags = mouseTags;
            this.touchTags = touchTags;
            this.penTags = penTags;

            Init(TOUCH_API.WIN8, pointerDownDelegate, pointerUpdateDelegate, pointerUpDelegate);
        }

        protected override Tags getTagsForType(POINTER_INPUT_TYPE type)
        {
            switch (type)
            {
                case POINTER_INPUT_TYPE.PT_MOUSE:
                    return mouseTags;
                case POINTER_INPUT_TYPE.PT_TOUCH:
                    return touchTags;
                case POINTER_INPUT_TYPE.PT_PEN:
                    return penTags;
                default:
                    return Tags.EMPTY;
            }
        }

    }

    public class Windows7TouchHandler : WindowsTouchHandler
    {
        private int touchInputSize;

        /// <inheritdoc />
        public Windows7TouchHandler(Tags tags, Func<Vector2, Tags, bool, TouchPoint> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch, Action<int> cancelTouch) : base(tags, beginTouch, moveTouch, endTouch, cancelTouch)
        {
            touchInputSize = Marshal.SizeOf(typeof (TOUCHINPUT));
            RegisterTouchWindow(hMainWindow, 0);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            UnregisterTouchWindow(hMainWindow);

            base.Dispose();
        }

        private IntPtr wndProcWin7(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_TOUCH:
                    decodeWin7Touches(wParam, lParam);
                    return IntPtr.Zero;
                case WM_CLOSE:
                    Dispose();
                    SendMessage(hWnd, WM_CLOSE, wParam, lParam);
                    return IntPtr.Zero;
                default:
                    return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
            }
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

                if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_DOWN) != 0)
                {
                    POINT p = new POINT();
                    p.X = touch.x/100;
                    p.Y = touch.y/100;
                    ScreenToClient(hMainWindow, ref p);

                    winToInternalId.Add(touch.dwID, beginTouch(new Vector2((p.X - offsetX)*scaleX, Screen.height - (p.Y - offsetY)*scaleY), tags, true).Id);
                }
                else if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_UP) != 0)
                {
                    int existingId;
                    if (winToInternalId.TryGetValue(touch.dwID, out existingId))
                    {
                        winToInternalId.Remove(touch.dwID);
                        endTouch(existingId);
                    }
                }
                else if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_MOVE) != 0)
                {
                    int existingId;
                    if (winToInternalId.TryGetValue(touch.dwID, out existingId))
                    {
                        POINT p = new POINT();
                        p.X = touch.x/100;
                        p.Y = touch.y/100;
                        ScreenToClient(hMainWindow, ref p);

                        moveTouch(existingId,
                            new Vector2((p.X - offsetX)*scaleX, Screen.height - (p.Y - offsetY)*scaleY));
                    }
                }
            }

            CloseTouchInputHandle(lParam);
        }
    }

    public abstract class WindowsTouchHandler : IDisposable
    {
        /// <summary>
        /// Source of touch input.
        /// </summary>
        public enum TouchSource
        {
            Touch,
            Pen,
            Mouse
        }

        /// <summary>
        /// Windows constant to turn off press and hold visual effect.
        /// </summary>
        public const string PRESS_AND_HOLD_ATOM = "MicrosoftTabletPenServiceProperty";

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        public delegate void PointerDown(int id, POINTER_INPUT_TYPE type, Vector2 position, uint flags);
        public delegate void PointerUpdate(int id, Vector2 position, uint flags);
        public delegate void PointerUp(int id, uint flags);
        public delegate void Ping(int id);

        protected PointerDown pointerDownDelegate;
        protected PointerUpdate pointerUpdateDelegate;
        protected PointerUp pointerUpDelegate;

        protected Func<Vector2, Tags, bool, TouchPoint> beginTouch;
        protected Action<int, Vector2> moveTouch;
        protected Action<int> endTouch;
        protected Action<int> cancelTouch;

        protected Tags tags; 
        protected IntPtr hMainWindow;
        protected IntPtr oldWndProcPtr;
        protected IntPtr newWndProcPtr;
        protected WndProcDelegate newWndProc;
        protected ushort pressAndHoldAtomID;
        protected Dictionary<int, int> winToInternalId = new Dictionary<int, int>();

        protected float offsetX, offsetY, scaleX, scaleY;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsTouchHandler"/> class.
        /// </summary>
        /// <param name="beginTouch"> A function called when a new touch is detected. As <see cref="InputSource.beginTouch(Vector2)"/> this function must accept a Vector2 position of the new touch and return an instance of <see cref="TouchPoint"/>. </param>
        /// <param name="moveTouch"> A function called when a touch is moved. As <see cref="InputSource.moveTouch"/> this function must accept an int id and a Vector2 position. </param>
        /// <param name="endTouch"> A function called when a touch is lifted off. As <see cref="InputSource.endTouch"/> this function must accept an int id. </param>
        /// <param name="cancelTouch"> A function called when a touch is cancelled. As <see cref="InputSource.cancelTouch"/> this function must accept an int id. </param>
        public WindowsTouchHandler(Tags tags, Func<Vector2, Tags, bool, TouchPoint> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch, Action<int> cancelTouch)
        {
            this.tags = tags;
            this.beginTouch = beginTouch;
            this.moveTouch = moveTouch;
            this.endTouch = endTouch;
            this.cancelTouch = cancelTouch;

            pointerDownDelegate = pointerDown;
            pointerUpdateDelegate = pointerUpdate;
            pointerUpDelegate = pointerUp;

            hMainWindow = GetActiveWindow();
            disablePressAndHold();
            initScaling();
        }

        /// <inheritdoc />
        public bool CancelTouch(TouchPoint touch, bool @return)
        {
            int internalId = -1;
            foreach (var t in winToInternalId)
            {
                if (t.Value == touch.Id)
                {
                    internalId = t.Key;
                    break;
                }
            }
            if (internalId > -1)
            {
                cancelTouch(touch.Id);
                if (@return) winToInternalId[internalId] = beginTouch(touch.Position, touch.Tags, false).Id;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            foreach (var i in winToInternalId) cancelTouch(i.Value);

            enablePressAndHold();
            DisposePlugin();
        }

        protected void disablePressAndHold()
        {
            // https://msdn.microsoft.com/en-us/library/bb969148(v=vs.85).aspx
            pressAndHoldAtomID = GlobalAddAtom(PRESS_AND_HOLD_ATOM);
            SetProp(hMainWindow, PRESS_AND_HOLD_ATOM,
                TABLET_DISABLE_PRESSANDHOLD | // disables press and hold (right-click) gesture
                TABLET_DISABLE_PENTAPFEEDBACK | // disables UI feedback on pen up (waves)
                TABLET_DISABLE_PENBARRELFEEDBACK | // disables UI feedback on pen button down (circle)
                TABLET_DISABLE_FLICKS // disables pen flicks (back, forward, drag down, drag up);
                );
        }

        protected void enablePressAndHold()
        {
            if (pressAndHoldAtomID != 0)
            {
                RemoveProp(hMainWindow, PRESS_AND_HOLD_ATOM);
                GlobalDeleteAtom(pressAndHoldAtomID);
            }
        }

        protected void initScaling()
        {
            if (!Screen.fullScreen)
            {
                offsetX = offsetY = 0;
                scaleX = scaleY = 1;
            }
            else
            {
                int width, height;
                getNativeMonitorResolution(out width, out height);
                float scale = Mathf.Max(Screen.width/((float) width), Screen.height/((float) height));
                offsetX = (width - Screen.width/scale)*.5f;
                offsetY = (height - Screen.height/scale)*.5f;
                scaleX = scale;
                scaleY = scale;
            }

            SetScreenParams(Screen.width, Screen.height, offsetX, offsetY, scaleX, scaleY);
        }

        private void getNativeMonitorResolution(out int width, out int height)
        {
            var monitor = MonitorFromWindow(GetActiveWindow(), MONITOR_DEFAULTTONEAREST);
            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            if (!GetMonitorInfo(monitor, ref monitorInfo))
            {
                width = Screen.width;
                height = Screen.height;
            }
            else
            {
                width = monitorInfo.rcMonitor.Width;
                height = monitorInfo.rcMonitor.Height;
            }
        }

        protected virtual Tags getTagsForType(POINTER_INPUT_TYPE type)
        {
            return Tags.EMPTY;
        }

        private void pointerDown(int id, POINTER_INPUT_TYPE type, Vector2 position, uint flags)
        {
            if ((flags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED) return;
            winToInternalId.Add(id, beginTouch(position, getTagsForType(type), true).Id);

        }

        private void pointerUpdate(int id, Vector2 position, uint flags)
        {
            int existingId;
            if (winToInternalId.TryGetValue(id, out existingId))
            {
                if ((flags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                {
                    winToInternalId.Remove(id);
                    cancelTouch(existingId);
                }
                else
                {
                    moveTouch(existingId, position);
                }
            }
        }

        private void pointerUp(int id, uint flags)
        {
            int existingId;
            if (winToInternalId.TryGetValue(id, out existingId))
            {
                winToInternalId.Remove(id);
                if ((flags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                    cancelTouch(existingId);
                else endTouch(existingId);
            }
        }

        #region p/invoke

        public enum TOUCH_API
        {
            WIN7,
            WIN8
        }

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        public static extern void Init(TOUCH_API api, PointerDown pointerDown, PointerUpdate pointerUpdate, PointerUp pointerUp);

        [DllImport("WindowsTouch", EntryPoint = "Dispose", CallingConvention = CallingConvention.StdCall)]
        public static extern void DisposePlugin();

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY);

        public const int GWL_WNDPROC = -4;

        public const int WM_CLOSE = 0x0010;
        public const int WM_TOUCH = 0x0240;
        public const int WM_POINTERDOWN = 0x0246;
        public const int WM_POINTERUP = 0x0247;
        public const int WM_POINTERUPDATE = 0x0245;

        public const int POINTER_FLAG_CANCELLED = 0x00008000;

        public const int TABLET_DISABLE_PRESSANDHOLD = 0x00000001;
        public const int TABLET_DISABLE_PENTAPFEEDBACK = 0x00000008;
        public const int TABLET_DISABLE_PENBARRELFEEDBACK = 0x00000010;
        public const int TABLET_DISABLE_FLICKS = 0x00010000;

        public const int MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int X
            {
                get { return Left; }
                set
                {
                    Right -= (Left - value);
                    Left = value;
                }
            }

            public int Y
            {
                get { return Top; }
                set
                {
                    Bottom -= (Top - value);
                    Top = value;
                }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }
        }

        public enum TOUCH_EVENT : int
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
        public struct TOUCHINPUT
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

        public enum POINTER_INPUT_TYPE
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
        }

        public enum POINTER_BUTTON_CHANGE_TYPE
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
        public struct POINTER_INFO
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
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public static int LOWORD(int value)
        {
            return value & 0xffff;
        }

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        public static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterTouchWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [Out] TOUCHINPUT[] pInputs,
            int cbSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern void CloseTouchInputHandle(IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPointerInfo(int pointerID, ref POINTER_INFO pPointerInfo);

        [DllImport("Kernel32.dll")]
        public static extern ushort GlobalAddAtom(string lpString);

        [DllImport("Kernel32.dll")]
        public static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("user32.dll")]
        public static extern int SetProp(IntPtr hWnd, string lpString, int hData);

        [DllImport("user32.dll")]
        public static extern int RemoveProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        #endregion
    }

    /// <summary>
    /// A class which turns on mouse to WM_POINTER events redirection on Windows 8.
    /// </summary>
    public class Windows8MouseHandler : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Windows8MouseHandler"/> class.
        /// </summary>
        public Windows8MouseHandler()
        {
            EnableMouseInPointer(true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            EnableMouseInPointer(false);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr EnableMouseInPointer(bool value);
    }
}

#endif