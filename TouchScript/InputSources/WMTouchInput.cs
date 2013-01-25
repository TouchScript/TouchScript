/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

public delegate bool EnumWindowsProcDelegate(IntPtr hWnd, IntPtr lParam);

namespace TouchScript.InputSources {
    internal enum TouchEvent : int {
        TOUCHEVENTF_MOVE = 0x0001,
        TOUCHEVENTF_DOWN = 0x0002,
        TOUCHEVENTF_UP = 0x0004,
        TOUCHEVENTF_INRANGE = 0x0008,
        TOUCHEVENTF_PRIMARY = 0x0010,
        TOUCHEVENTF_NOCOALESCE = 0x0020,
        TOUCHEVENTF_PEN = 0x0040
    }

    /// <summary>
    /// Processes Windows 7 touch events.
    /// Known issues:
    /// <list type="bullet">
    ///     <item>DOES NOT WORK IN EDITOR.</item>
    ///     <item>App crashes on exit.</item>
    /// </list>
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Windows 7 Touch Input")]
    public class WMTouchInput : InputSource {
        #region Private fields

        private IntPtr hMainWindow;
        private IntPtr oldWndProcPtr;
        private IntPtr newWndProcPtr;

        private WndProcDelegate newWndProc;

        private Dictionary<int, int> winToInternalId = new Dictionary<int, int>();
        private bool isInitialized = false;

        #endregion

        #region Unity

        protected override void Start() {
            base.Start();
            init();
        }

        protected override void OnDestroy() {
            if (isInitialized) {
                SetWindowLong(hMainWindow, -4, oldWndProcPtr);
                UnregisterTouchWindow(hMainWindow);

                hMainWindow = IntPtr.Zero;
                oldWndProcPtr = IntPtr.Zero;
                newWndProcPtr = IntPtr.Zero;

                newWndProc = null;
            }
            base.OnDestroy();
        }

        #endregion

        #region Private functions

        private void init() {
            if (Application.isEditor) return;

            touchInputSize = Marshal.SizeOf(typeof (TOUCHINPUT));

            hMainWindow = GetForegroundWindow();
            RegisterTouchWindow(hMainWindow, 0);

            newWndProc = new WndProcDelegate(wndProc);
            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            oldWndProcPtr = SetWindowLong(hMainWindow, -4, newWndProcPtr);

            isInitialized = true;
        }

        private IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
            if (msg == WM_TOUCH) decodeTouches(wParam, lParam);
            return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
        }

        private void decodeTouches(IntPtr wParam, IntPtr lParam) {
            int inputCount = LOWORD(wParam.ToInt32());
            TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];

            if (!GetTouchInputInfo(lParam, inputCount, inputs, touchInputSize)) {
                return;
            }

            for (int i = 0; i < inputCount; i++) {
                TOUCHINPUT touch = inputs[i];

                if ((touch.dwFlags & (int) TouchEvent.TOUCHEVENTF_DOWN) != 0) {
                    POINT p = new POINT();
                    p.X = touch.x/100;
                    p.Y = touch.y/100;
                    ScreenToClient(hMainWindow, ref p);

                    winToInternalId.Add(touch.dwID, beginTouch(new Vector2(p.X, Screen.height - p.Y)));
                } else if ((touch.dwFlags & (int) TouchEvent.TOUCHEVENTF_UP) != 0) {
                    int existingId;
                    if (winToInternalId.TryGetValue(touch.dwID, out existingId)) {
                        winToInternalId.Remove(touch.dwID);
                        endTouch(existingId);
                    }
                } else if ((touch.dwFlags & (int) TouchEvent.TOUCHEVENTF_MOVE) != 0) {
                    int existingId;
                    if (winToInternalId.TryGetValue(touch.dwID, out existingId)) {
                        POINT p = new POINT();
                        p.X = touch.x/100;
                        p.Y = touch.y/100;
                        ScreenToClient(hMainWindow, ref p);

                        moveTouch(existingId, new Vector2(p.X, Screen.height - p.Y));
                    }
                }
            }

            CloseTouchInputHandle(lParam);
        }

        #endregion

        #region p/invoke

        // Touch event window message constants [winuser.h]
        private const int WM_TOUCH = 0x0240;

        // Touch API defined structures [winuser.h]
        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT {
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

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

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

        private int touchInputSize;

        private int HIWORD(int value) {
            return (int) (value >> 16);
        }

        private int LOWORD(int value) {
            return (int) (value & 0xffff);
        }

        #endregion
    }
}