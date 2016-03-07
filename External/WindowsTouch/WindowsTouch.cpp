/*
* @author Valentin Simonov / http://va.lent.in/
*/

#include "WindowsTouch.h"

extern "C" 
{

	void __stdcall Init(TOUCH_API api, PointerBeganFuncPtr began, PointerMovedFuncPtr moved,
		PointerEndedFuncPtr ended, PointerCancelledFuncPtr cancelled)
	{
		_pointerBeganFunc = began;
		_pointerMovedFunc = moved;
		_pointerEndedFunc = ended;
		_pointerCancelledFunc = cancelled;
		_api = api;

		_currentWindow = GetActiveWindow();
		if (api == WIN8)
		{
			_oldWindowProc = SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)wndProc8);
		}
		else
		{
			RegisterTouchWindow(_currentWindow, 0);
			_oldWindowProc = SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)wndProc7);
		}
	}

	void __stdcall Dispose()
	{
		if (_oldWindowProc)
		{
			SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)_oldWindowProc);
			_oldWindowProc = 0;
			if (_api == WIN7)
			{
				UnregisterTouchWindow(_currentWindow);
			}
		}
	}

	void __stdcall SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY)
	{
		_screenWidth = width;
		_screenHeight = height;
		_offsetX = offsetX;
		_offsetY = offsetY;
		_scaleX = scaleX;
		_scaleY = scaleY;
	}

}

LRESULT CALLBACK wndProc8(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_TOUCH:
		CloseTouchInputHandle((HTOUCHINPUT)lParam);
		break;
	case WM_POINTERDOWN:
	case WM_POINTERUP:
	case WM_POINTERUPDATE:
		decodeWin8Touches(msg, wParam, lParam);
		break;
	default:
		return CallWindowProc((WNDPROC)_oldWindowProc, hwnd, msg, wParam, lParam);
	}
	return 0;
}

LRESULT CALLBACK wndProc7(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_TOUCH:
		decodeWin7Touches(msg, wParam, lParam);
		break;
	default:
		return CallWindowProc((WNDPROC)_oldWindowProc, hwnd, msg, wParam, lParam);
	}
	return 0;
}

void decodeWin8Touches(UINT msg, WPARAM wParam, LPARAM lParam)
{
	int pointerId = GET_POINTERID_WPARAM(wParam);

	POINTER_INFO pointerInfo;
	if (!GetPointerInfo(pointerId, &pointerInfo)) return;

	POINT p;
	p.x = pointerInfo.ptPixelLocation.x;
	p.y = pointerInfo.ptPixelLocation.y;
	ScreenToClient(_currentWindow, &p);

	switch (msg)
	{
	case WM_POINTERDOWN:
		if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELED) != 0) return;
		_pointerBeganFunc(pointerId, pointerInfo.pointerType, Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY));
		break;
	case WM_POINTERUP:
		if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELED) != 0)
		{
			_pointerCancelledFunc(pointerId);
		}
		else {
			_pointerEndedFunc(pointerId);
		}
		break;
	case WM_POINTERUPDATE:
		if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELED) != 0)
		{
			_pointerCancelledFunc(pointerId);
		}
		else {
			_pointerMovedFunc(pointerId, Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY));
		}
		break;
	}
}

void decodeWin7Touches(UINT msg, WPARAM wParam, LPARAM lParam)
{
	UINT cInputs = LOWORD(wParam);
	PTOUCHINPUT pInputs = new TOUCHINPUT[cInputs];

	if (!pInputs) return;
	if (!GetTouchInputInfo((HTOUCHINPUT)lParam, cInputs, pInputs, sizeof(TOUCHINPUT))) return;

	for (UINT i = 0; i < cInputs; i++)
	{
		TOUCHINPUT touch = pInputs[i];

		POINT p;
		p.x = touch.x / 100;
		p.y = touch.y / 100;
		ScreenToClient(_currentWindow, &p);

		if ((touch.dwFlags & TOUCHEVENTF_DOWN) != 0)
		{
			_pointerBeganFunc(touch.dwID, PT_TOUCH, Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY));
		}
		else if ((touch.dwFlags & TOUCHEVENTF_UP) != 0)
		{
			_pointerEndedFunc(touch.dwID);
		}
		else if ((touch.dwFlags & TOUCHEVENTF_MOVE) != 0)
		{
			_pointerMovedFunc(touch.dwID, Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY));
		}
	}

	CloseTouchInputHandle((HTOUCHINPUT)lParam);
	delete[] pInputs;
}