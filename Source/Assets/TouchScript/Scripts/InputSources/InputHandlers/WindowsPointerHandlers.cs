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
                        if ((mousePointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) != 0)
                        {
                            var pressed = (uint) (mousePointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed);
                            mousePointer.Buttons |= (Pointer.PointerButtonState) (pressed << 2); // add up state
                            mousePointer.Buttons &= ~Pointer.PointerButtonState.AnyButtonPressed; // remove pressed state
                            releasePointer(mousePointer);
                        }
                        removePointer(mousePointer);
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
        public Windows8PointerHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer) : base(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer)
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
                cancelPointer(mousePointer);
                if (shouldReturn) mousePointer = internalReturnMousePointer(mousePointer);
                else mousePointer = internalAddMousePointer(pointer.Position); // can't totally cancell mouse pointer
                return true;
            }
            if (pointer.Equals(penPointer))
            {
                cancelPointer(penPointer);
                if (shouldReturn) penPointer = internalReturnPenPointer(penPointer);
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
                cancelPointer(mousePointer);
                mousePointer = null;
            }
            if (penPointer != null)
            {
                cancelPointer(penPointer);
                penPointer = null;
            }

            EnableMouseInPointer(false);

            base.Dispose();
        }

#endregion

#region Internal methods

        /// <inheritdoc />
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

            switch (msg)
            {
                case WM_POINTERDOWN:
                {
                    if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED) break;
                    var position = new Vector2((p.X - offsetX)*scaleX, Screen.height - (p.Y - offsetY)*scaleY);

                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                        {
                            var button = (((int)pointerInfo.ButtonChangeType - 1) / 2) * 3;
                            mousePointer.Buttons |= (Pointer.PointerButtonState)(1 << (button + 1)); // add down
                            mousePointer.Buttons |= (Pointer.PointerButtonState)(1 << button); // add pressed
                            pressPointer(mousePointer);
                        }
                            break;
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                        {
                            winTouchToInternalId.Add(pointerId, internalAddTouchPointer(position));
                        }
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                        {
                            var button = (((int)pointerInfo.ButtonChangeType - 1) / 2) * 3;
                            penPointer.Buttons |= (Pointer.PointerButtonState)(1 << (button + 1)); // add down
                            penPointer.Buttons |= (Pointer.PointerButtonState)(1 << button); // add pressed
                            pressPointer(penPointer);
                        }
                            break;
                    }
                }
                    break;

                case WM_POINTERUP:
                {
                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                            if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                            {
                                cancelPointer(mousePointer);
                                mousePointer = internalAddMousePointer(mousePointer.Position); // can't totally cancell mouse pointer
                            }
                            else
                            {
                            var button = ((int)pointerInfo.ButtonChangeType / 2 - 1) * 3;
                            mousePointer.Buttons |= (Pointer.PointerButtonState)(1 << (button + 2)); // add up
                            mousePointer.Buttons &= ~(Pointer.PointerButtonState)(1 << button); // remove pressed
                            releasePointer(mousePointer);
                            }
                            break;
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                            TouchPointer touchPointer;
                            if (winTouchToInternalId.TryGetValue(pointerId, out touchPointer))
                            {
                                winTouchToInternalId.Remove(pointerId);
                                if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED) cancelPointer(touchPointer);
                                else
                                {
                                    internalRemoveTouchPointer(touchPointer);
                                }
                            }
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                            if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                            {
                                cancelPointer(penPointer);
                                penPointer = internalAddPenPointer(penPointer.Position); // can't totally cancell mouse pointer;
                            }
                            else
                            {
                                var button = ((int)pointerInfo.ButtonChangeType / 2 - 1) * 3;
                                penPointer.Buttons |= (Pointer.PointerButtonState)(1 << (button + 2)); // add up
                                penPointer.Buttons &= ~(Pointer.PointerButtonState)(1 << button); // remove pressed
                                releasePointer(penPointer);
                            }
                            break;
                    }
                }
                    break;
                case WM_POINTERUPDATE:
                {
                    Pointer pointer = null;
                    var position = new Vector2((p.X - offsetX)*scaleX, Screen.height - (p.Y - offsetY)*scaleY);
                    switch (pointerInfo.pointerType)
                    {
                        case POINTER_INPUT_TYPE.PT_MOUSE:
                            pointer = mousePointer;
                            break;
                        case POINTER_INPUT_TYPE.PT_TOUCH:
                            TouchPointer touchPointer;
                            if (winTouchToInternalId.TryGetValue(pointerId, out touchPointer)) pointer = touchPointer;
                            break;
                        case POINTER_INPUT_TYPE.PT_PEN:
                            if (penPointer == null) internalAddPenPointer(position);
                            pointer = penPointer;
                            break;
                    }
                    if (pointer != null)
                    {
                        if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELLED) == POINTER_FLAG_CANCELLED)
                        {
                            cancelPointer(pointer);
                            switch (pointerInfo.pointerType)
                            {
                                case POINTER_INPUT_TYPE.PT_MOUSE:
                                    mousePointer = internalAddMousePointer(pointer.Position); // can't totally cancell mouse pointer
                                    break;
                                case POINTER_INPUT_TYPE.PT_TOUCH:
                                    winTouchToInternalId.Remove(pointerId);
                                    break;
                                case POINTER_INPUT_TYPE.PT_PEN:
                                    penPointer = internalAddPenPointer(pointer.Position); // can't totally cancell mouse pointer;
                                    break;
                            }
                        }
                        else
                        {
                            if (pointerInfo.ButtonChangeType != POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_NONE)
                            {
                                var change = (int)pointerInfo.ButtonChangeType;
                                if (change % 2 == 0) // up
                                {
                                    var button = (change / 2 - 1) * 3;
                                    pointer.Buttons |= (Pointer.PointerButtonState)(1 << (button + 2)); // add up
                                    pointer.Buttons &= ~(Pointer.PointerButtonState)(1 << button); // remove pressed
                                } else // down
                                {
                                    var button = ((change - 1) / 2) * 3;
                                    pointer.Buttons |= (Pointer.PointerButtonState)(1 << (button + 1)); // add down
                                    pointer.Buttons |= (Pointer.PointerButtonState)(1 << button); // add pressed
                                }
                            }
                            pointer.Position = position;
                            updatePointer(pointer);
                        }
                    }
                }
                    break;
            }
        }

        private MousePointer internalAddMousePointer(Vector2 position)
        {
            var pointer = mousePool.Get();
            pointer.Position = remapCoordinates(position);
            addPointer(pointer);
            return pointer;
        }

        private MousePointer internalReturnMousePointer(MousePointer pointer)
        {
            var newPointer = mousePool.Get();
            newPointer.CopyFrom(pointer);
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            if ((newPointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) != 0)
            {
                // Adding down state this frame
                newPointer.Buttons |= (Pointer.PointerButtonState)((uint)(newPointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) << 1);
                pressPointer(newPointer);
            }
            return newPointer;
        }

        private PenPointer internalAddPenPointer(Vector2 position)
        {
            var pointer = penPool.Get();
            pointer.Position = remapCoordinates(position);
            addPointer(pointer);
            return pointer;
        }

        private PenPointer internalReturnPenPointer(PenPointer pointer)
        {
            var newPointer = penPool.Get();
            newPointer.CopyFrom(pointer);
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            if ((newPointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) != 0)
            {
                // Adding down state this frame
                newPointer.Buttons |= (Pointer.PointerButtonState)((uint)(newPointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) << 1);
                pressPointer(newPointer);
            }
            return newPointer;
        }
    }

    public class Windows7PointerHandler : WindowsPointerHandler
    {
        private int touchInputSize;

        /// <inheritdoc />
        public Windows7PointerHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer) : base(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer)
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
                    p.X = touch.x/100;
                    p.Y = touch.y/100;
                    ScreenToClient(hMainWindow, ref p);

                    winTouchToInternalId.Add(touch.dwID, internalAddTouchPointer(new Vector2((p.X - offsetX)*scaleX, Screen.height - (p.Y - offsetY)*scaleY)));
                }
                else if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_UP) != 0)
                {
                    TouchPointer touchPointer;
                    if (winTouchToInternalId.TryGetValue(touch.dwID, out touchPointer))
                    {
                        winTouchToInternalId.Remove(touch.dwID);
                        internalRemoveTouchPointer(touchPointer);
                    }
                }
                else if ((touch.dwFlags & (int) TOUCH_EVENT.TOUCHEVENTF_MOVE) != 0)
                {
                    TouchPointer touchPointer;
                    if (winTouchToInternalId.TryGetValue(touch.dwID, out touchPointer))
                    {
                        POINT p = new POINT();
                        p.X = touch.x/100;
                        p.Y = touch.y/100;
                        ScreenToClient(hMainWindow, ref p);

                        touchPointer.Position = remapCoordinates(new Vector2((p.X - offsetX)*scaleX, Screen.height - (p.Y - offsetY)*scaleY));
                        updatePointer(touchPointer);
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

#region Public properties

        /// <inheritdoc />
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

#endregion

#region Protected variables

        protected PointerDelegate addPointer;
        protected PointerDelegate updatePointer;
        protected PointerDelegate pressPointer;
        protected PointerDelegate releasePointer;
        protected PointerDelegate removePointer;
        protected PointerDelegate cancelPointer;

        protected IntPtr hMainWindow;
        protected IntPtr oldWndProcPtr;
        protected IntPtr newWndProcPtr;
        protected WndProcDelegate newWndProc;
        protected ushort pressAndHoldAtomID;
        protected Dictionary<int, TouchPointer> winTouchToInternalId = new Dictionary<int, TouchPointer>();

        protected float offsetX, offsetY, scaleX, scaleY;

        protected ObjectPool<TouchPointer> touchPool;

#endregion

#region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPointerHandler"/> class.
        /// </summary>
        /// <param name="addPointer">A function called when a new pointer is detected.</param>
        /// <param name="updatePointer">A function called when a pointer is moved or its parameter is updated.</param>
        /// <param name="pressPointer">A function called when a pointer touches the surface.</param>
        /// <param name="releasePointer">A function called when a pointer is lifted off.</param>
        /// <param name="removePointer">A function called when a pointer is removed.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled.</param>
        public WindowsPointerHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer)
        {
            this.addPointer = addPointer;
            this.updatePointer = updatePointer;
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
        public void UpdateInput()
        {
        }

        /// <inheritdoc />
        public virtual bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            var touch = pointer as TouchPointer;
            if (touch == null) return false;

            int internalTouchId = -1;
            foreach (var t in winTouchToInternalId)
            {
                if (t.Value == touch)
                {
                    internalTouchId = t.Key;
                    break;
                }
            }
            if (internalTouchId > -1)
            {
                cancelPointer(touch);
                winTouchToInternalId.Remove(internalTouchId);
                if (shouldReturn) winTouchToInternalId[internalTouchId] = internalReturnTouchPointer(touch);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            foreach (var i in winTouchToInternalId) cancelPointer(i.Value);
            winTouchToInternalId.Clear();

            enablePressAndHold();
            unregisterWindowProc();
        }

#endregion

#region Internal methods

        /// <inheritdoc />
        public virtual void INTERNAL_DiscardPointer(Pointer pointer)
        {
            var p = pointer as TouchPointer;
            if (p == null) return;

            touchPool.Release(p);
        }

#endregion

#region Protected methods

        protected TouchPointer internalAddTouchPointer(Vector2 position)
        {
            var pointer = touchPool.Get();
            pointer.Position = remapCoordinates(position);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            addPointer(pointer);
            pressPointer(pointer);
            return pointer;
        }

        protected TouchPointer internalReturnTouchPointer(TouchPointer pointer)
        {
            var newPointer = touchPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            pressPointer(newPointer);
            return newPointer;
        }

        protected void internalRemoveTouchPointer(TouchPointer pointer)
        {
            pointer.Buttons &= ~Pointer.PointerButtonState.FirstButtonPressed;
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonUp;
            releasePointer(pointer);
            removePointer(pointer);
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
            float scale = Mathf.Max(Screen.width/((float) width), Screen.height/((float) height));
            offsetX = (width - Screen.width/scale)*.5f;
            offsetY = (height - Screen.height/scale)*.5f;
            scaleX = scale;
            scaleY = scale;
        }

        protected Vector2 remapCoordinates(Vector2 position)
        {
            if (CoordinatesRemapper != null) return CoordinatesRemapper.Remap(position);
            return position;
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