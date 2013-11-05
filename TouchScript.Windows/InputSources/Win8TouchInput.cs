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
    public class Win8TouchInput : InputSourceWindows
    {
        private delegate int WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #region Private fields

        private IntPtr hMainWindow;
        private IntPtr oldWndProcPtr;
        private IntPtr newWndProcPtr;

        private WndProcDelegate newWndProc;

        private Dictionary<int, int> winToInternalId = new Dictionary<int, int>();
        private bool isInitialized = false;

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();
            init();
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            if (isInitialized)
            {
                SetWindowLong(hMainWindow, -4, oldWndProcPtr);

                hMainWindow = IntPtr.Zero;
                oldWndProcPtr = IntPtr.Zero;
                newWndProcPtr = IntPtr.Zero;

                newWndProc = null;
            }
            base.OnDestroy();
        }

        #endregion

        #region Private functions

        private void init()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer) return;

            hMainWindow = GetForegroundWindow();

            newWndProc = new WndProcDelegate(wndProc);
            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            oldWndProcPtr = SetWindowLong(hMainWindow, -4, newWndProcPtr);

            isInitialized = true;
        }

        private int wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
				case WM_POINTERDOWN:
				case WM_POINTERUP:
				case WM_POINTERUPDATE:
					decodeTouches(msg, wParam, lParam);
					break;
                case WM_CLOSE:
                    SetWindowLong(hWnd, -4, oldWndProcPtr);
                    SendMessage(hWnd, WM_CLOSE, 0, 0);
                    return 0;
            }
            return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
        }
		
		private void decodeTouches(uint msg, IntPtr wParam, IntPtr lParam)
		{			
			int xPos = LOWORD(lParam.ToInt32());
			int yPos = HIWORD(lParam.ToInt32());
			int pointerId = LOWORD(wParam.ToInt32());
			
			POINTER_INFO pointerInfo = new POINTER_INFO();
			if (!GetPointerInfo(pointerId, ref pointerInfo))
			{
				return;

			}

			POINT p = new POINT();
            p.X = xPos;
            p.Y = yPos;
            ScreenToClient(hMainWindow, ref p);
	
			int existingId;
			
			switch (msg)
			{
				case WM_POINTERDOWN:
					winToInternalId.Add(pointerId, beginTouch(new Vector2(p.X, Screen.height - p.Y)));		
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
		private const int WM_POINTERDOWN = 0x0246;
		private const int WM_POINTERUP = 0x0247;
		private const int WM_POINTERUPDATE = 0x0245;

		
		private enum POINTER_INPUT_TYPE
		{
			PT_POINTER  = 0x00000001,
			PT_TOUCH    = 0x00000002,
			PT_PEN      = 0x00000003,
			PT_MOUSE    = 0x00000004,
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
        [StructLayout(LayoutKind.Sequential, Pack=1)]
        private struct POINTER_INFO
        {
		  public POINTER_INPUT_TYPE         pointerType;
		  public UInt32                     pointerId;
		  public UInt32                     frameId;
		  public UInt32			              pointerFlags;
		  public IntPtr                     sourceDevice;
		  public IntPtr                       hwndTarget;
		  public POINT                      ptPixelLocation;
		  public POINT                      ptHimetricLocation;
		  public POINT                      ptPixelLocationRaw;
		  public POINT                      ptHimetricLocationRaw;
		  public UInt32                       dwTime;
		  public UInt32                      historyCount;
		  public Int32                       inputData;
		  public UInt32                       dwKeyStates;
		  public UInt64                     PerformanceCount;
		  public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("coredll.dll", EntryPoint = "SendMessage", SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPointerInfo(int pointerID, ref POINTER_INFO pPointerInfo);
		
        private int HIWORD(int value)
        {
            return (int)(value >> 16);
        }

        private int LOWORD(int value)
        {
            return (int)(value & 0xffff);
        }

        #endregion
    }
}