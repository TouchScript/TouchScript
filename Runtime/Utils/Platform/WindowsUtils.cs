/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TouchScript.Utils.Platform
{
    /// <summary>
    /// Utility methods on Windows.
    /// </summary>
    public static class WindowsUtils
    {
        // disables press and hold (right-click) gesture
        public const int TABLET_DISABLE_PRESSANDHOLD = 0x00000001;
        // disables UI feedback on pen up (waves)
        public const int TABLET_DISABLE_PENTAPFEEDBACK = 0x00000008;
        // disables UI feedback on pen button down (circle)
        public const int TABLET_DISABLE_PENBARRELFEEDBACK = 0x00000010;
        // disables pen flicks (back, forward, drag down, drag up);
        public const int TABLET_DISABLE_FLICKS = 0x00010000;

        public const int MONITOR_DEFAULTTONEAREST = 2;

        /// <summary>
        /// Retrieves the native monitor resolution.
        /// </summary>
        /// <param name="width">Output width.</param>
        /// <param name="height">Output height.</param>
        public static void GetNativeMonitorResolution(out int width, out int height)
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

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("Kernel32.dll")]
        public static extern ushort GlobalAddAtom(string lpString);

        [DllImport("Kernel32.dll")]
        public static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("user32.dll")]
        public static extern int SetProp(IntPtr hWnd, string lpString, int hData);

        [DllImport("user32.dll")]
        public static extern int RemoveProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        public static extern IntPtr EnableMouseInPointer(bool value);
    }
}

#endif