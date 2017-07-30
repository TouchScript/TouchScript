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
using TouchScript.Utils.Platform;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Windows 8 pointer handling implementation which can be embedded to other (input) classes. Uses WindowsTouch.dll to query native touches with WM_TOUCH or WM_POINTER APIs.
    /// </summary>
    public class Windows8PointerHandler : WindowsPointerHandler
    {
        #region Public properties

        /// <summary>
        /// Should the primary pointer also dispatch a mouse pointer.
        /// </summary>
        public bool MouseInPointer
        {
            get { return mouseInPointer; }
            set
            {
                WindowsUtils.EnableMouseInPointer(value);
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
                            mousePointer.Buttons = PointerUtils.UpPressedButtons(mousePointer.Buttons);
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
            mousePool = new ObjectPool<MousePointer>(4, () => new MousePointer(this), null, resetPointer);
            penPool = new ObjectPool<PenPointer>(2, () => new PenPointer(this), null, resetPointer);

            mousePointer = internalAddMousePointer(Vector3.zero);

            init(TOUCH_API.WIN8);
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool UpdateInput()
        {
            base.UpdateInput();
            return true;
        }

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            if (pointer.Equals(mousePointer))
            {
                cancelPointer(mousePointer);
                if (shouldReturn) mousePointer = internalReturnMousePointer(mousePointer);
                else mousePointer = internalAddMousePointer(pointer.Position); // can't totally cancel mouse pointer
                return true;
            }
            if (pointer.Equals(penPointer))
            {
                cancelPointer(penPointer);
                if (shouldReturn) penPointer = internalReturnPenPointer(penPointer);
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

            WindowsUtils.EnableMouseInPointer(false);

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
        /// <inheritdoc />
        public Windows7PointerHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer) : base(addPointer, updatePointer, pressPointer, releasePointer, removePointer, cancelPointer)
        {
            init(TOUCH_API.WIN7);
        }

        #region Public methods

        /// <inheritdoc />
        public override bool UpdateInput()
        {
            base.UpdateInput();
            return winTouchToInternalId.Count > 0;
        }

        #endregion
    }

    /// <summary>
    /// Base class for Windows 8 and Windows 7 input handlers.
    /// </summary>
    public abstract class WindowsPointerHandler : IInputSource, IDisposable
    {
        #region Consts

        /// <summary>
        /// Windows constant to turn off press and hold visual effect.
        /// </summary>
        public const string PRESS_AND_HOLD_ATOM = "MicrosoftTabletPenServiceProperty";

        /// <summary>
        /// The method delegate used to pass data from the native DLL.
        /// </summary>
        /// <param name="id">Pointer id.</param>
        /// <param name="evt">Current event.</param>
        /// <param name="type">Pointer type.</param>
        /// <param name="position">Pointer position.</param>
        /// <param name="data">Pointer data.</param>
        protected delegate void NativePointerDelegate(int id, PointerEvent evt, PointerType type, Vector2 position, PointerData data);

        /// <summary>
        /// The method delegate used to pass log messages from the native DLL.
        /// </summary>
        /// <param name="log">The log message.</param>
        protected delegate void NativeLog([MarshalAs(UnmanagedType.BStr)] string log);

        #endregion

        #region Public properties

        /// <inheritdoc />
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        #endregion

        #region Private variables

        private NativePointerDelegate nativePointerDelegate;
        private NativeLog nativeLogDelegate;

        protected PointerDelegate addPointer;
        protected PointerDelegate updatePointer;
        protected PointerDelegate pressPointer;
        protected PointerDelegate releasePointer;
        protected PointerDelegate removePointer;
        protected PointerDelegate cancelPointer;

        protected IntPtr hMainWindow;
        protected ushort pressAndHoldAtomID;
        protected Dictionary<int, TouchPointer> winTouchToInternalId = new Dictionary<int, TouchPointer>(10);

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

            nativeLogDelegate = nativeLog;
            nativePointerDelegate = nativePointer;

            touchPool = new ObjectPool<TouchPointer>(10, () => new TouchPointer(this), null, resetPointer);

            hMainWindow = WindowsUtils.GetActiveWindow();
            disablePressAndHold();
            setScaling();
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public virtual bool UpdateInput()
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void UpdateResolution()
        {
            setScaling();
            if (mousePointer != null) TouchManager.Instance.CancelPointer(mousePointer.Id);
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

        /// <summary>
        /// Releases resources.
        /// </summary>
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
                newPointer.Buttons = PointerUtils.DownPressedButtons(newPointer.Buttons);
                pressPointer(newPointer);
            }
            return newPointer;
        }

        protected PenPointer internalAddPenPointer(Vector2 position)
        {
            if (penPointer != null) throw new InvalidOperationException("One pen pointer is already registered! Trying to add another one.");
            var pointer = penPool.Get();
            pointer.Position = remapCoordinates(position);
            addPointer(pointer);
            return pointer;
        }

        protected void internalRemovePenPointer(PenPointer pointer)
        {
            removePointer(pointer);
            penPointer = null;
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
                newPointer.Buttons = PointerUtils.DownPressedButtons(newPointer.Buttons);
                pressPointer(newPointer);
            }
            return newPointer;
        }

        protected void init(TOUCH_API api)
        {
            Init(api, nativeLogDelegate, nativePointerDelegate);
        }

        protected Vector2 remapCoordinates(Vector2 position)
        {
            if (CoordinatesRemapper != null) return CoordinatesRemapper.Remap(position);
            return position;
        }

        protected void resetPointer(Pointer p)
        {
            p.INTERNAL_Reset();
        }

        #endregion

        #region Private functions

        private void disablePressAndHold()
        {
            // https://msdn.microsoft.com/en-us/library/bb969148(v=vs.85).aspx
            pressAndHoldAtomID = WindowsUtils.GlobalAddAtom(PRESS_AND_HOLD_ATOM);
            WindowsUtils.SetProp(hMainWindow, PRESS_AND_HOLD_ATOM,
                WindowsUtils.TABLET_DISABLE_PRESSANDHOLD | // disables press and hold (right-click) gesture
                WindowsUtils.TABLET_DISABLE_PENTAPFEEDBACK | // disables UI feedback on pen up (waves)
                WindowsUtils.TABLET_DISABLE_PENBARRELFEEDBACK | // disables UI feedback on pen button down (circle)
                WindowsUtils.TABLET_DISABLE_FLICKS // disables pen flicks (back, forward, drag down, drag up);
                );
        }

        private void enablePressAndHold()
        {
            if (pressAndHoldAtomID != 0)
            {
                WindowsUtils.RemoveProp(hMainWindow, PRESS_AND_HOLD_ATOM);
                WindowsUtils.GlobalDeleteAtom(pressAndHoldAtomID);
            }
        }

        private void setScaling()
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            if (!Screen.fullScreen)
            {
                SetScreenParams(screenWidth, screenHeight, 0, 0, 1, 1);
                return;
            }

            int width, height;
            WindowsUtils.GetNativeMonitorResolution(out width, out height);
            float scale = Mathf.Max(screenWidth / ((float) width), screenHeight / ((float) height));
            SetScreenParams(screenWidth, screenHeight, (width - screenWidth / scale) * .5f, (height - screenHeight / scale) * .5f, scale, scale);
        }

        #endregion

        #region Pointer callbacks

        private void nativeLog(string log)
        {
            Debug.Log("[WindowsTouch.dll]: " + log);
        }

        private void nativePointer(int id, PointerEvent evt, PointerType type, Vector2 position, PointerData data)
        {
            switch (type)
            {
                case PointerType.Mouse:
                    switch (evt)
                    {
                        // Enter and Exit are not used - mouse is always present
                        // TODO: how does it work with 2+ mice?
                        case PointerEvent.Enter:
                            throw new NotImplementedException("This is not supposed to be called o.O");
                        case PointerEvent.Leave:
                            break;
                        case PointerEvent.Down:
                            mousePointer.Buttons = updateButtons(mousePointer.Buttons, data.PointerFlags, data.ChangedButtons);
                            pressPointer(mousePointer);
                            break;
                        case PointerEvent.Up:
                            mousePointer.Buttons = updateButtons(mousePointer.Buttons, data.PointerFlags, data.ChangedButtons);
                            releasePointer(mousePointer);
                            break;
                        case PointerEvent.Update:
                            mousePointer.Position = position;
                            mousePointer.Buttons = updateButtons(mousePointer.Buttons, data.PointerFlags, data.ChangedButtons);
                            updatePointer(mousePointer);
                            break;
                        case PointerEvent.Cancelled:
                            cancelPointer(mousePointer);
                            // can't cancel the mouse pointer, it is always present
                            mousePointer = internalAddMousePointer(mousePointer.Position);
                            break;
                    }
                    break;
                case PointerType.Touch:
                    TouchPointer touchPointer;
                    switch (evt)
                    {
                        case PointerEvent.Enter:
                            break;
                        case PointerEvent.Leave:
                            // Sometimes Windows might not send Up, so have to execute touch release logic here.
                            // Has been working fine on test devices so far.
                            if (winTouchToInternalId.TryGetValue(id, out touchPointer))
                            {
                                winTouchToInternalId.Remove(id);
                                internalRemoveTouchPointer(touchPointer);
                            }
                            break;
                        case PointerEvent.Down:
                            touchPointer = internalAddTouchPointer(position);
                            touchPointer.Rotation = getTouchRotation(ref data);
                            touchPointer.Pressure = getTouchPressure(ref data);
                            winTouchToInternalId.Add(id, touchPointer);
                            break;
                        case PointerEvent.Up:
                            break;
                        case PointerEvent.Update:
                            if (!winTouchToInternalId.TryGetValue(id, out touchPointer)) return;
                            touchPointer.Position = position;
                            touchPointer.Rotation = getTouchRotation(ref data);
                            touchPointer.Pressure = getTouchPressure(ref data);
                            updatePointer(touchPointer);
                            break;
                        case PointerEvent.Cancelled:
                            if (winTouchToInternalId.TryGetValue(id, out touchPointer))
                            {
                                winTouchToInternalId.Remove(id);
                                cancelPointer(touchPointer);
                            }
                            break;
                    }
                    break;
                case PointerType.Pen:
                    switch (evt)
                    {
                        case PointerEvent.Enter:
                            penPointer = internalAddPenPointer(position);
                            penPointer.Pressure = getPenPressure(ref data);
                            penPointer.Rotation = getPenRotation(ref data);
                            break;
                        case PointerEvent.Leave:
                            if (penPointer == null) break;
                            internalRemovePenPointer(penPointer);
                            break;
                        case PointerEvent.Down:
                            if (penPointer == null) break;
                            penPointer.Buttons = updateButtons(penPointer.Buttons, data.PointerFlags, data.ChangedButtons);
                            penPointer.Pressure = getPenPressure(ref data);
                            penPointer.Rotation = getPenRotation(ref data);
                            pressPointer(penPointer);
                            break;
                        case PointerEvent.Up:
                            if (penPointer == null) break;
                            mousePointer.Buttons = updateButtons(penPointer.Buttons, data.PointerFlags, data.ChangedButtons);
                            releasePointer(penPointer);
                            break;
                        case PointerEvent.Update:
                            if (penPointer == null) break;
                            penPointer.Position = position;
                            penPointer.Pressure = getPenPressure(ref data);
                            penPointer.Rotation = getPenRotation(ref data);
                            penPointer.Buttons = updateButtons(penPointer.Buttons, data.PointerFlags, data.ChangedButtons);
                            updatePointer(penPointer);
                            break;
                        case PointerEvent.Cancelled:
                            if (penPointer == null) break;
                            cancelPointer(penPointer);
                            break;
                    }
                    break;
            }
        }

        private Pointer.PointerButtonState updateButtons(Pointer.PointerButtonState current, PointerFlags flags, ButtonChangeType change)
        {
            var currentUpDown = ((uint) current) & 0xFFFFFC00;
            var pressed = ((uint) flags >> 4) & 0x1F;
            var newUpDown = 0U;
            if (change != ButtonChangeType.None) newUpDown = 1U << (10 + (int) change);
            var combined = (Pointer.PointerButtonState) (pressed | newUpDown | currentUpDown);
            return combined;
        }

        private float getTouchPressure(ref PointerData data)
        {
            var reliable = (data.Mask & (uint) TouchMask.Pressure) > 0;
            if (reliable) return data.Pressure / 1024f;
            return TouchPointer.DEFAULT_PRESSURE;
        }

        private float getTouchRotation(ref PointerData data)
        {
            var reliable = (data.Mask & (uint) TouchMask.Orientation) > 0;
            if (reliable) return data.Rotation / 180f * Mathf.PI;
            return TouchPointer.DEFAULT_ROTATION;
        }

        private float getPenPressure(ref PointerData data)
        {
            var reliable = (data.Mask & (uint) PenMask.Pressure) > 0;
            if (reliable) return data.Pressure / 1024f;
            return PenPointer.DEFAULT_PRESSURE;
        }

        private float getPenRotation(ref PointerData data)
        {
            var reliable = (data.Mask & (uint) PenMask.Rotation) > 0;
            if (reliable) return data.Rotation / 180f * Mathf.PI;
            return PenPointer.DEFAULT_ROTATION;
        }

        #endregion

        #region p/invoke

        protected enum TOUCH_API
        {
            WIN7,
            WIN8
        }

        protected enum PointerEvent : uint
        {
            Enter = 0x0249,
            Leave = 0x024A,
            Update = 0x0245,
            Down = 0x0246,
            Up = 0x0247,
            Cancelled = 0x1000
        }

        protected enum PointerType
        {
            Pointer = 0x00000001,
            Touch = 0x00000002,
            Pen = 0x00000003,
            Mouse = 0x00000004,
            TouchPad = 0x00000005
        }

        [Flags]
        protected enum PointerFlags
        {
            None = 0x00000000,
            New = 0x00000001,
            InRange = 0x00000002,
            InContact = 0x00000004,
            FirstButton = 0x00000010,
            SecondButton = 0x00000020,
            ThirdButton = 0x00000040,
            FourthButton = 0x00000080,
            FifthButton = 0x00000100,
            Primary = 0x00002000,
            Confidence = 0x00004000,
            Canceled = 0x00008000,
            Down = 0x00010000,
            Update = 0x00020000,
            Up = 0x00040000,
            Wheel = 0x00080000,
            HWheel = 0x00100000,
            CaptureChanged = 0x00200000,
            HasTransform = 0x00400000
        }

        protected enum ButtonChangeType
        {
            None,
            FirstDown,
            FirstUp,
            SecondDown,
            SecondUp,
            ThirdDown,
            ThirdUp,
            FourthDown,
            FourthUp,
            FifthDown,
            FifthUp
        }

        [Flags]
        protected enum TouchFlags
        {
            None = 0x00000000
        }

        [Flags]
        protected enum TouchMask
        {
            None = 0x00000000,
            ContactArea = 0x00000001,
            Orientation = 0x00000002,
            Pressure = 0x00000004
        }

        [Flags]
        protected enum PenFlags
        {
            None = 0x00000000,
            Barrel = 0x00000001,
            Inverted = 0x00000002,
            Eraser = 0x00000004
        }

        [Flags]
        protected enum PenMask
        {
            None = 0x00000000,
            Pressure = 0x00000001,
            Rotation = 0x00000002,
            TiltX = 0x00000004,
            TiltY = 0x00000008
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct PointerData
        {
            public PointerFlags PointerFlags;
            public uint Flags;
            public uint Mask;
            public ButtonChangeType ChangedButtons;
            public uint Rotation;
            public uint Pressure;
            public int TiltX;
            public int TiltY;
        }

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        private static extern void Init(TOUCH_API api, NativeLog log, NativePointerDelegate pointerDelegate);

        [DllImport("WindowsTouch", EntryPoint = "Dispose", CallingConvention = CallingConvention.StdCall)]
        private static extern void DisposePlugin();

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        private static extern void SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY);

        #endregion
    }
}

#endif