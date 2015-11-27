/*
 * @author Valentin Simonov / http://va.lent.in/
 * @author Andrew David Griffiths
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes Windows 8 touch events.
    /// Known issues:
    /// <list type="bullet">
    ///     <item>DOES NOT WORK IN EDITOR.</item>
    /// </list>
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Windows 8 Touch Input")]
    public sealed class Win8TouchInput : InputSource
    {
        #region Constants

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

        private Dictionary<int, int> winToInternalId = new Dictionary<int, int>();
        private bool isInitialized = false;

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void OnEnable()
        {
            // "WindowsEditor" in the Editor
            if (Application.platform != RuntimePlatform.WindowsPlayer)
            {
                enabled = false;
                return;
            }

            // disable mouse
            var inputs = FindObjectsOfType<MouseInput>();
            var count = inputs.Length;
            for (var i = 0; i < count; i++)
            {
                inputs[i].enabled = false;
            }

            base.OnEnable();
            init();
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

                hMainWindow = IntPtr.Zero;
                oldWndProcPtr = IntPtr.Zero;
                newWndProcPtr = IntPtr.Zero;

                newWndProc = null;
            }

            foreach (var i in winToInternalId)
            {
                cancelTouch(i.Value);
            }

            base.OnDisable();
        }

        #endregion

        #region Private functions

        private void init()
        {
            hMainWindow = GetActiveWindow();

            newWndProc = wndProc;
            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);

            EnableMouseInPointer(true);

            pressAndHoldAtomID = GlobalAddAtom(PRESS_AND_HOLD_ATOM);
            SetProp(hMainWindow, PRESS_AND_HOLD_ATOM, 1);

            isInitialized = true;
        }

        private IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_TOUCH:
                    CloseTouchInputHandle(lParam); // don't let Unity handle this
                    return IntPtr.Zero;
                case WM_POINTERDOWN:
                case WM_POINTERUP:
                case WM_POINTERUPDATE:
                    decodeTouches(msg, wParam, lParam);
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

        private void decodeTouches(uint msg, IntPtr wParam, IntPtr lParam)
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
                    winToInternalId.Add(pointerId, beginTouch(new Vector2(p.X, Screen.height - p.Y), tags).Id);
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

        #endregion

        #region p/invoke

        // Touch event window message constants [winuser.h]
        private const int WM_CLOSE = 0x0010;
        private const int WM_TOUCH = 0x0240;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERUPDATE = 0x0245;


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

        // Touch API defined structures [winuser.h]
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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPointerInfo(int pointerID, ref POINTER_INFO pPointerInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr EnableMouseInPointer(bool value);

        [DllImport("Kernel32.dll")]
        private static extern ushort GlobalAddAtom(string lpString);

        [DllImport("Kernel32.dll")]
        private static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("user32.dll")]
        private static extern int SetProp(IntPtr hWnd, string lpString, int hData);

        [DllImport("user32.dll")]
        private static extern int RemoveProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern void CloseTouchInputHandle(IntPtr lParam);

        private int HIWORD(int value)
        {
            return (int)(value >> 0xf);
        }

        private int LOWORD(int value)
        {
            return (int)(value & 0xffff);
        }

        #endregion
    }
}
