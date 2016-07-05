/*
 * @author Valentin Simonov / http://va.lent.in/
 * @author Valentin Frolov
 * @author Andrew David Griffiths
 */

#if UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Windows 8 pointer handling implementation which can be embedded to other (input) classes.
    /// </summary>
    public class Windows8PointerHandler : WindowsPointerHandler
    {
        #region Public properties

        public bool MouseInPointer
        {
            get { return mouseInPointer; }
            set
            {
                EnableMouseInPointer(value);
                mouseInPointer = value;
                if (mouseInPointer)
                {
                    if (mousePointer == null) mousePointer = internalAddMousePointer(Vector3.zero);
                }
                else
                {
                    if (mousePointer != null)
                    {
                        if ((mousePointer.Flags & Pointer.FLAG_INCONTACT) != 0) releasePointer(mousePointer.Id);
                        removePointer(mousePointer.Id);
                    }
                }
            }
        }

        #endregion

        #region Private variables

        private bool mouseInPointer = true;
        private ObjectPool<MousePointer> mousePool;
        private ObjectPool<PenPointer> penPool;
        private MousePointer mousePointer;
        private PenPointer penPointer;

        #endregion

        #region Constructor

        /// <inheritdoc />
        public Windows8PointerHandler(AddPointerDelegate addPointer, MovePointerDelegate movePointer, PressPointerDelegate pressPointer, ReleasePointerDelegate releasePointer, RemovePointerDelegate removePointer, CancelPointerDelegate cancelPointer) : base(addPointer, movePointer, pressPointer, releasePointer, removePointer, cancelPointer)
        {
            mousePool = new ObjectPool<MousePointer>(4, () => new MousePointer(this), null, (t) => t.INTERNAL_Reset());
            penPool = new ObjectPool<PenPointer>(2, () => new PenPointer(this), null, (t) => t.INTERNAL_Reset());

            mousePointer = internalAddMousePointer(Vector3.zero);

            registerWindowProc(wndProcWin8);
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            if (pointer.Equals(mousePointer))
            {
                cancelPointer(mousePointer.Id);
                if (shouldReturn) mousePointer = internalReturnMousePointer(mousePointer, pointer.Position);
                else mousePointer = internalAddMousePointer(pointer.Position); // can't totally cancell mouse pointer
                return true;
            }
            if (pointer.Equals(penPointer))
            {
                cancelPointer(penPointer.Id);
                if (shouldReturn) penPointer = internalReturnPenPointer(penPointer, pointer.Position);
                else penPointer = internalAddPenPointer(pointer.Position); // can't totally cancell mouse pointer
                return true;
            }
            return base.CancelPointer(pointer, shouldReturn);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (mousePointer != null)
            {
                cancelPointer(mousePointer.Id);
                mousePointer = null;
            }
            if (penPointer != null)
            {
                cancelPointer(penPointer.Id);
                penPointer = null;
            }

            EnableMouseInPointer(false);

            base.Dispose();
        }

        #endregion

        #region Internal methods

        public override void INTERNAL_DiscardPointer(Pointer pointer)
        {
            if (pointer is MousePointer) mousePool.Release(pointer as MousePointer);
            else if (pointer is PenPointer) penPool.Release(pointer as PenPointer);
            else base.INTERNAL_DiscardPointer(pointer);
        }

        #endregion

        private IntPtr wndProcWin8(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_TOUCH:
                    CloseTouchInputHandle(lParam); // don't let Unity handle this
                    return IntPtr.Zero;
                case WM_POINTERDOWN:
                case WM_POINTERUP:
                case WM_POINTERUPDATE:
                    decodeWin8Touches(msg, wParam, lParam);
                    return IntPtr.Zero;
                case WM_CLOSE:
                    // Not having this crashes app on quit
                    SetWindowLongPtr(hWnd, -4, oldWndProcPtr);
                    SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    return IntPtr.Zero;
                default:
                    return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
            }
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
                {
                    if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED) break;
                    var position = new Vector2((p.X - offsetX) * scaleX, Screen.height - (p.Y - offsetY) * scaleY);
                    var buttonFlags = (pointerInfo.pointerFlags << 3) & Pointer.FLAG_INCONTACT;
                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                            pressPointer(mousePointer.Id);
                            mousePointer.Flags = (mousePointer.Flags & ~Pointer.FLAG_INCONTACT) | buttonFlags;
                            break;
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                            winTouchToInternalId.Add(pointerId, internalAddTouchPointer(position, buttonFlags).Id);
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                            pressPointer(penPointer.Id);
                            penPointer.Flags = (penPointer.Flags & ~Pointer.FLAG_INCONTACT) | buttonFlags;
                            break;
                    }
                }
                    break;

                case WM_POINTERUP:
                {
                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                            var id = mousePointer.Id;
                            if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                            {
                                cancelPointer(id);
                                mousePointer = null;
                            }
                            else releasePointer(id);
                            break;
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                            if (winTouchToInternalId.TryGetValue(pointerId, out existingId))
                            {
                                winTouchToInternalId.Remove(pointerId);
                                if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED) cancelPointer(existingId);
                                else
                                {
                                    releasePointer(existingId);
                                    removePointer(existingId);
                                }
                            }
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                            id = penPointer.Id;
                            if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                            {
                                cancelPointer(id);
                                penPointer = null;
                            }
                            else releasePointer(id);
                            break;
                    }
                }
                    break;
                case WM_POINTERUPDATE:
                {
                    var id = Pointer.INVALID_POINTER;
                    var position = new Vector2((p.X - offsetX) * scaleX, Screen.height - (p.Y - offsetY) * scaleY);
                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                            id = mousePointer.Id;
                            break;
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                            if (winTouchToInternalId.TryGetValue(pointerId, out existingId)) id = existingId;
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                            if (penPointer == null) internalAddPenPointer(position);
                            id = penPointer.Id;
                            break;
                    }
                    if (id != Pointer.INVALID_POINTER)
                    {
                        if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                        {
                            if (pointerInfo.pointerType == POINTER_INPUT_TYPE.PT_TOUCH) winTouchToInternalId.Remove(pointerId);
                            cancelPointer(id);
                        }
                        else
                        {
                            movePointer(id, position);
                        }
                    }
                }
                    break;
            }
        }

        private MousePointer internalAddMousePointer(Vector2 position, uint flags = 0)
        {
            var pointer = mousePool.Get();
            addPointer(pointer, position);
            pointer.Flags |= flags;
            return pointer;
        }

        private MousePointer internalReturnMousePointer(MousePointer pointer, Vector2 position)
        {
            var newPointer = mousePool.Get();
            newPointer.CopyFrom(pointer);
            addPointer(newPointer, position, false);
            if ((newPointer.Flags & Pointer.FLAG_INCONTACT) != 0) pressPointer(newPointer.Id);
            return newPointer;
        }

        private PenPointer internalAddPenPointer(Vector2 position, uint flags = 0)
        {
            var pointer = penPool.Get();
            addPointer(pointer, position);
            pointer.Flags |= flags;
            return pointer;
        }

        private PenPointer internalReturnPenPointer(PenPointer pointer, Vector2 position)
        {
            var newPointer = penPool.Get();
            newPointer.CopyFrom(pointer);
            addPointer(newPointer, position, false);
            if ((newPointer.Flags & Pointer.FLAG_INCONTACT) != 0) pressPointer(newPointer.Id);
            return newPointer;
        }
    }

    public class Windows7PointerHandler : WindowsPointerHandler
    {
        private int touchInputSize;

        /// <inheritdoc />
        public Windows7PointerHandler(AddPointerDelegate addPointer, MovePointerDelegate movePointer, PressPointerDelegate pressPointer, ReleasePointerDelegate releasePointer, RemovePointerDelegate removePointer, CancelPointerDelegate cancelPointer) : base(addPointer, movePointer, pressPointer, releasePointer, removePointer, cancelPointer)
        {
            touchInputSize = Marshal.SizeOf(typeof (TOUCHINPUT));
            RegisterTouchWindow(hMainWindow, 0);
            registerWindowProc(wndProcWin7);
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
                    // Not having this crashes app on quit
                    UnregisterTouchWindow(hWnd);
                    SetWindowLongPtr(hWnd, -4, oldWndProcPtr);
                    SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
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
                    p.X = touch.x / 100;
                    p.Y = touch.y / 100;
                    ScreenToClient(hMainWindow, ref p);

                    winTouchToInternalId.Add(touch.dwID, internalAddTouchPointer(new Vector2((p.X - offsetX) * scaleX, Screen.height - (p.Y - offsetY) * scaleY), Pointer.FLAG_FIRST_BUTTON).Id);
                }
                else if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_UP) != 0)
                {
                    int existingId;
                    if (winTouchToInternalId.TryGetValue(touch.dwID, out existingId))
                    {
                        winTouchToInternalId.Remove(touch.dwID);
                        releasePointer(existingId);
                        removePointer(existingId);
                    }
                }
                else if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_MOVE) != 0)
                {
                    int existingId;
                    if (winTouchToInternalId.TryGetValue(touch.dwID, out existingId))
                    {
                        POINT p = new POINT();
                        p.X = touch.x / 100;
                        p.Y = touch.y / 100;
                        ScreenToClient(hMainWindow, ref p);

                        movePointer(existingId,
                            new Vector2((p.X - offsetX) * scaleX, Screen.height - (p.Y - offsetY) * scaleY));
                    }
                }
            }

            CloseTouchInputHandle(lParam);
        }
    }

    public abstract class WindowsPointerHandler : IInputSource, IDisposable
    {
        #region Consts

        /// <summary>
        /// Source of pointer input.
        /// </summary>
        public enum PointerSource
        {
            Pointer,
            Pen,
            Mouse
        }

        /// <summary>
        /// Windows constant to turn off press and hold visual effect.
        /// </summary>
        public const string PRESS_AND_HOLD_ATOM = "MicrosoftTabletPenServiceProperty";

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Protected variables

        protected AddPointerDelegate addPointer;
        protected MovePointerDelegate movePointer;
        protected PressPointerDelegate pressPointer;
        protected ReleasePointerDelegate releasePointer;
        protected RemovePointerDelegate removePointer;
        protected CancelPointerDelegate cancelPointer;

        protected IntPtr hMainWindow;
        protected IntPtr oldWndProcPtr;
        protected IntPtr newWndProcPtr;
        protected WndProcDelegate newWndProc;
        protected ushort pressAndHoldAtomID;
        protected Dictionary<int, int> winTouchToInternalId = new Dictionary<int, int>();

        protected float offsetX, offsetY, scaleX, scaleY;

        protected ObjectPool<TouchPointer> touchPool;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPointerHandler"/> class.
        /// </summary>
        /// <param name="pressPointer"> A function called when a new pointer is detected. As <see cref="InputSource.pressPointer(Vector2)"/> this function must accept a Vector2 position of the new pointer and return an instance of <see cref="Pointer"/>. </param>
        /// <param name="movePointer"> A function called when a pointer is moved. As <see cref="InputSource.movePointer"/> this function must accept an int id and a Vector2 position. </param>
        /// <param name="releasePointer"> A function called when a pointer is lifted off. As <see cref="InputSource.releasePointer"/> this function must accept an int id. </param>
        /// <param name="cancelPointer"> A function called when a pointer is cancelled. As <see cref="InputSource.cancelPointer"/> this function must accept an int id. </param>
        public WindowsPointerHandler(AddPointerDelegate addPointer, MovePointerDelegate movePointer, PressPointerDelegate pressPointer, ReleasePointerDelegate releasePointer, RemovePointerDelegate removePointer, CancelPointerDelegate cancelPointer)
        {
            this.addPointer = addPointer;
            this.movePointer = movePointer;
            this.pressPointer = pressPointer;
            this.releasePointer = releasePointer;
            this.removePointer = removePointer;
            this.cancelPointer = cancelPointer;

            touchPool = new ObjectPool<TouchPointer>(10, () => new TouchPointer(this), null, (t) => t.INTERNAL_Reset());

            hMainWindow = GetActiveWindow();
            disablePressAndHold();
            initScaling();
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public void UpdateInput() {}

        /// <inheritdoc />
        public virtual bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            var touch = pointer as TouchPointer;
            if (touch == null) return false;

            int internalTouchId = -1;
            foreach (var t in winTouchToInternalId)
            {
                if (t.Value == touch.Id)
                {
                    internalTouchId = t.Key;
                    break;
                }
            }
            if (internalTouchId > -1)
            {
                cancelPointer(touch.Id);
                if (shouldReturn) winTouchToInternalId[internalTouchId] = internalReturnTouchPointer(touch, touch.Position).Id;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            foreach (var i in winTouchToInternalId) cancelPointer(i.Value);

            enablePressAndHold();
            unregisterWindowProc();
        }

        #endregion

        #region Internal methods

        public virtual void INTERNAL_DiscardPointer(Pointer pointer)
        {
            var p = pointer as TouchPointer;
            if (p == null) return;

            touchPool.Release(p);
        }

        #endregion

        #region Protected methods

        protected Pointer internalAddTouchPointer(Vector2 position, uint flags = 0)
        {
            var pointer = touchPool.Get();
            addPointer(pointer, position, true);
            pressPointer(pointer.Id);
            pointer.Flags |= flags;
            return pointer;
        }

        private TouchPointer internalReturnTouchPointer(TouchPointer pointer, Vector2 position)
        {
            var newPointer = touchPool.Get();
            newPointer.CopyFrom(pointer);
            addPointer(newPointer, position, false);
            pressPointer(newPointer.Id);
            return newPointer;
        }

        protected void registerWindowProc(WndProcDelegate windowProc)
        {
            newWndProc = windowProc;
            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);
        }

        protected void unregisterWindowProc()
        {
            SetWindowLongPtr(hMainWindow, -4, oldWndProcPtr);
            hMainWindow = IntPtr.Zero;
            oldWndProcPtr = IntPtr.Zero;
            newWndProcPtr = IntPtr.Zero;

            newWndProc = null;
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
                return;
            }

            int width, height;
            getNativeMonitorResolution(out width, out height);
            float scale = Mathf.Max(Screen.width / ((float) width), Screen.height / ((float) height));
            offsetX = (width - Screen.width / scale) * .5f;
            offsetY = (height - Screen.height / scale) * .5f;
            scaleX = scale;
            scaleY = scale;
        }

        #endregion

        #region Private functions

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

        #endregion

        #region p/invoke

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

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
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

        [DllImport("user32.dll")]
        public static extern IntPtr EnableMouseInPointer(bool value);

        #endregion
    }
}

#endif