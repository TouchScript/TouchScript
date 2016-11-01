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

#endregion

#region Constructor

        /// <inheritdoc />
        public Windows8PointerHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer) : base(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer)
        {
            mousePool = new ObjectPool<MousePointer>(4, () => new MousePointer(this), null, (t) => t.INTERNAL_Reset());
            penPool = new ObjectPool<PenPointer>(2, () => new PenPointer(this), null, (t) => t.INTERNAL_Reset());

            mousePointer = internalAddMousePointer(Vector3.zero);

            init(TOUCH_API.WIN8);
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

    }

    public class Windows7PointerHandler : WindowsPointerHandler
    {
        private int touchInputSize;

        /// <inheritdoc />
        public Windows7PointerHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer) : base(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer)
        {
            init(TOUCH_API.WIN7);
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

        protected delegate void NativePointerDown(int id, POINTER_INPUT_TYPE type, Pointer.PointerButtonState buttons, Vector2 position);
        protected delegate void NativePointerUpdate(int id, POINTER_INPUT_TYPE type, Pointer.PointerButtonState buttonsSet, Pointer.PointerButtonState buttonsClear, Vector2 position);
        protected delegate void NativePointerUp(int id, POINTER_INPUT_TYPE type, Pointer.PointerButtonState buttons);
        protected delegate void NativePointerCancel(int id, POINTER_INPUT_TYPE type);

        #endregion

        #region Public properties

        /// <inheritdoc />
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        #endregion

        #region Protected variables

        private NativePointerDown nativePointerDownDelegate;
        private NativePointerUpdate nativePointerUpdateDelegate;
        private NativePointerUp nativePointerUpDelegate;
        private NativePointerCancel nativePointerCancelDelegate;

        protected PointerDelegate addPointer;
        protected PointerDelegate updatePointer;
        protected PointerDelegate pressPointer;
        protected PointerDelegate releasePointer;
        protected PointerDelegate removePointer;
        protected PointerDelegate cancelPointer;

        protected IntPtr hMainWindow;
        protected ushort pressAndHoldAtomID;
        protected Dictionary<int, TouchPointer> winTouchToInternalId = new Dictionary<int, TouchPointer>();

        protected ObjectPool<TouchPointer> touchPool;
        protected ObjectPool<MousePointer> mousePool;
        protected ObjectPool<PenPointer> penPool;
        protected MousePointer mousePointer;
        protected PenPointer penPointer;

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

            nativePointerDownDelegate = nativePointerDown;
            nativePointerUpdateDelegate = nativePointerUpdate;
            nativePointerUpDelegate = nativePointerUp;
            nativePointerCancelDelegate = nativePointerCancel;

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
            DisposePlugin();
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

        protected MousePointer internalAddMousePointer(Vector2 position)
        {
            var pointer = mousePool.Get();
            pointer.Position = remapCoordinates(position);
            addPointer(pointer);
            return pointer;
        }

        protected MousePointer internalReturnMousePointer(MousePointer pointer)
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

        protected PenPointer internalAddPenPointer(Vector2 position)
        {
            var pointer = penPool.Get();
            pointer.Position = remapCoordinates(position);
            addPointer(pointer);
            return pointer;
        }

        protected PenPointer internalReturnPenPointer(PenPointer pointer)
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

        protected void init(TOUCH_API api)
        {
            Init(api, nativePointerDownDelegate, nativePointerUpdateDelegate, nativePointerUpDelegate, nativePointerCancelDelegate);
        }

        protected Vector2 remapCoordinates(Vector2 position)
        {
            if (CoordinatesRemapper != null) return CoordinatesRemapper.Remap(position);
            return position;
        }

        #endregion

        #region Private functions

        private void disablePressAndHold()
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

        private void enablePressAndHold()
        {
            if (pressAndHoldAtomID != 0)
            {
                RemoveProp(hMainWindow, PRESS_AND_HOLD_ATOM);
                GlobalDeleteAtom(pressAndHoldAtomID);
            }
        }

        private void initScaling()
        {
            if (!Screen.fullScreen)
            {
                SetScreenParams(Screen.width, Screen.height, 0, 0, 1, 1);
                return;
            }

            int width, height;
            getNativeMonitorResolution(out width, out height);
            float scale = Mathf.Max(Screen.width / ((float)width), Screen.height / ((float)height));
            SetScreenParams(Screen.width, Screen.height, (width - Screen.width / scale) * .5f, (height - Screen.height / scale) * .5f, scale, scale);
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

        #endregion

        #region Pointer callbacks

        private void nativePointerDown(int id, POINTER_INPUT_TYPE type, Pointer.PointerButtonState buttons, Vector2 position)
        {
            switch (type)
            {
                case POINTER_INPUT_TYPE.PT_MOUSE:
                {
                    mousePointer.Buttons = buttons;
                    pressPointer(mousePointer);
                }
                break;
                case POINTER_INPUT_TYPE.PT_TOUCH:
                {
                    winTouchToInternalId.Add(id, internalAddTouchPointer(position));
                }
                break;
                case POINTER_INPUT_TYPE.PT_PEN:
                {
                    penPointer.Buttons = buttons;
                    pressPointer(penPointer);
                }
                break;
            }
        }

        private void nativePointerUpdate(int id, POINTER_INPUT_TYPE type, Pointer.PointerButtonState buttonsSet, Pointer.PointerButtonState buttonsClear, Vector2 position)
        {
            //            int existingId;
            //            if (winTouchToInternalId.TryGetValue(id, out existingId))
            //            {
            //                moveTouch(existingId, position);
            //            }
            Pointer pointer = null;
            switch (type)
            {
                case POINTER_INPUT_TYPE.PT_MOUSE:
                    pointer = mousePointer;
                    break;
                case POINTER_INPUT_TYPE.PT_TOUCH:
                    TouchPointer touchPointer;
                    if (winTouchToInternalId.TryGetValue(id, out touchPointer)) pointer = touchPointer;
                    break;
                case POINTER_INPUT_TYPE.PT_PEN:
                    if (penPointer == null) internalAddPenPointer(position);
                    pointer = penPointer;
                    break;
            }
            if (pointer != null)
            {
                pointer.Position = position;
                pointer.Buttons &= ~buttonsClear;
                pointer.Buttons |= buttonsSet;
                updatePointer(pointer);
            }
        }

        private void nativePointerUp(int id, POINTER_INPUT_TYPE type, Pointer.PointerButtonState buttons)
        {
            //            int existingId;
            //            if (winTouchToInternalId.TryGetValue(id, out existingId))
            //            {
            //                winToInternalId.Remove(id);
            //                endTouch(existingId);
            //            }
            switch (type)
            {
                case POINTER_INPUT_TYPE.PT_MOUSE:
                    mousePointer.Buttons = buttons;
                    releasePointer(mousePointer);
                    break;
                case POINTER_INPUT_TYPE.PT_TOUCH:
                    TouchPointer touchPointer;
                    if (winTouchToInternalId.TryGetValue(id, out touchPointer))
                    {
                        winTouchToInternalId.Remove(id);
                        internalRemoveTouchPointer(touchPointer);
                    }
                    break;
                case POINTER_INPUT_TYPE.PT_PEN:
                    penPointer.Buttons = buttons;
                    releasePointer(penPointer);
                    break;
            }
        }

        private void nativePointerCancel(int id, POINTER_INPUT_TYPE type)
        {
            //            int existingId;
            //            if (winTouchToInternalId.TryGetValue(id, out existingId))
            //            {
            //                winToInternalId.Remove(id);
            //                cancelTouch(existingId);
            //            }

            switch (type)
            {
                case POINTER_INPUT_TYPE.PT_MOUSE:
                    cancelPointer(mousePointer);
                    mousePointer = internalAddMousePointer(mousePointer.Position); // can't totally cancell mouse pointer
                    break;
                case POINTER_INPUT_TYPE.PT_TOUCH:
                    TouchPointer touchPointer;
                    if (winTouchToInternalId.TryGetValue(id, out touchPointer))
                    {
                        winTouchToInternalId.Remove(id);
                        cancelPointer(touchPointer);
                    }
                    break;
                case POINTER_INPUT_TYPE.PT_PEN:
                    cancelPointer(penPointer);
                    penPointer = internalAddPenPointer(penPointer.Position); // can't totally cancell mouse pointer;
                    break;
            }
        }

        #endregion

        #region p/invoke

        protected enum TOUCH_API
        {
            WIN7,
            WIN8
        }

        private const int TABLET_DISABLE_PRESSANDHOLD = 0x00000001;
        private const int TABLET_DISABLE_PENTAPFEEDBACK = 0x00000008;
        private const int TABLET_DISABLE_PENBARRELFEEDBACK = 0x00000010;
        private const int TABLET_DISABLE_FLICKS = 0x00010000;

        private const int MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
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

        protected enum POINTER_INPUT_TYPE
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
        }

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        private static extern void Init(TOUCH_API api, NativePointerDown nativePointerDown, NativePointerUpdate nativePointerUpdate, NativePointerUp nativePointerUp, NativePointerCancel nativePointerCancel);

        [DllImport("WindowsTouch", EntryPoint = "Dispose", CallingConvention = CallingConvention.StdCall)]
        private static extern void DisposePlugin();

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        private static extern void SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("Kernel32.dll")]
        private static extern ushort GlobalAddAtom(string lpString);

        [DllImport("Kernel32.dll")]
        private static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("user32.dll")]
        private static extern int SetProp(IntPtr hWnd, string lpString, int hData);

        [DllImport("user32.dll")]
        private static extern int RemoveProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        public static extern IntPtr EnableMouseInPointer(bool value);

        #endregion
    }
}

#endif