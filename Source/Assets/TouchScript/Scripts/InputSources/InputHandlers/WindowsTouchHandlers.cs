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

    #region Windows 8

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

            init(TOUCH_API.WIN8);
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

    #endregion

    #region Windows 7

    public class Windows7TouchHandler : WindowsTouchHandler
    {
        private int touchInputSize;

        /// <inheritdoc />
        public Windows7TouchHandler(Tags tags, Func<Vector2, Tags, bool, TouchPoint> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch, Action<int> cancelTouch) : base(tags, beginTouch, moveTouch, endTouch, cancelTouch)
        {
            init(TOUCH_API.WIN7);
        }

    }

    #endregion

    #region Base Windows touch handler

    public abstract class WindowsTouchHandler : IDisposable
    {

        #region Consts

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

        protected delegate void PointerBegan(int id, POINTER_INPUT_TYPE type, Vector2 position);
        protected delegate void PointerMoved(int id, Vector2 position);
        protected delegate void PointerEnded(int id);
        protected delegate void PointerCancelled(int id);

        #endregion

        #region Private variables

        private PointerBegan pointerBeganDelegate;
        private PointerMoved pointerMovedDelegate;
        private PointerEnded pointerEndedDelegate;
        private PointerCancelled pointerCancelledDelegate;

        private Func<Vector2, Tags, bool, TouchPoint> beginTouch;
        private Action<int, Vector2> moveTouch;
        private Action<int> endTouch;
        private Action<int> cancelTouch;

        private Tags tags;
        private IntPtr hMainWindow;
        private ushort pressAndHoldAtomID;
        private Dictionary<int, int> winToInternalId = new Dictionary<int, int>();

        #endregion

        #region Constructor

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

            pointerBeganDelegate = pointerBegan;
            pointerMovedDelegate = pointerMoved;
            pointerEndedDelegate = pointerEnded;
            pointerCancelledDelegate = pointerCancelled;

            hMainWindow = GetActiveWindow();
            disablePressAndHold();
            initScaling();
        }

        #endregion

        #region Public methods

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

        #endregion

        #region Protected methods

        protected void init(TOUCH_API api)
        {
            Init(api, pointerBeganDelegate, pointerMovedDelegate, pointerEndedDelegate, pointerCancelledDelegate);
        }

        protected virtual Tags getTagsForType(POINTER_INPUT_TYPE type)
        {
            return tags;
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
            float scale = Mathf.Max(Screen.width/((float) width), Screen.height/((float) height));
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

        private void pointerBegan(int id, POINTER_INPUT_TYPE type, Vector2 position)
        {
            winToInternalId.Add(id, beginTouch(position, getTagsForType(type), true).Id);
        }

        private void pointerMoved(int id, Vector2 position)
        {
            int existingId;
            if (winToInternalId.TryGetValue(id, out existingId))
            {
                moveTouch(existingId, position);
            }
        }

        private void pointerEnded(int id)
        {
            int existingId;
            if (winToInternalId.TryGetValue(id, out existingId))
            {
                winToInternalId.Remove(id);
                endTouch(existingId);
            }
        }

        private void pointerCancelled(int id)
        {
            int existingId;
            if (winToInternalId.TryGetValue(id, out existingId))
            {
                winToInternalId.Remove(id);
                cancelTouch(existingId);
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

        protected enum POINTER_INPUT_TYPE
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
        }

        [DllImport("WindowsTouch", CallingConvention = CallingConvention.StdCall)]
        private static extern void Init(TOUCH_API api, PointerBegan pointerBegan, PointerMoved pointerMoved, PointerEnded pointerEnded, PointerCancelled pointerCancelled);

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

        #endregion
    }

    #endregion

    #region Windows 8 mouse handler

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

    #endregion
}

#endif