/*
* @author Valentin Simonov / http://va.lent.in/
*/

#include "WindowsTouch.h"

extern "C" 
{

	void __stdcall Init(TOUCH_API api, LogFuncPtr logFunc, PointerDelegatePtr delegate)
	{
		_log = logFunc;
		_delegate = delegate;
		_api = api;

		_currentWindow = FindWindowA("UnityWndClass", NULL);
		if (api == WIN8)
		{
			HINSTANCE h = LoadLibrary(TEXT("user32.dll"));
			GetPointerInfo = (GET_POINTER_INFO) GetProcAddress(h, "GetPointerInfo");
			GetPointerTouchInfo = (GET_POINTER_TOUCH_INFO) GetProcAddress(h, "GetPointerTouchInfo");
			GetPointerPenInfo = (GET_POINTER_PEN_INFO)GetProcAddress(h, "GetPointerPenInfo");

			_oldWindowProc = SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)wndProc8);
			log(L"Initialized WIN8 input.");
		}
		else
		{
			RegisterTouchWindow(_currentWindow, 0);
			_oldWindowProc = SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)wndProc7);
			log(L"Initialized WIN7 input.");
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
	case WM_POINTERENTER:
	case WM_POINTERLEAVE:
	case WM_POINTERDOWN:
	case WM_POINTERUP:
	case WM_POINTERUPDATE:
	case WM_POINTERCAPTURECHANGED:
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

	Vector2 position = Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY);
	PointerData data {};
	data.pointerFlags = pointerInfo.pointerFlags;
	data.changedButtons = pointerInfo.ButtonChangeType;

	if ((pointerInfo.pointerFlags & POINTER_FLAG_CANCELED) != 0
		|| msg == WM_POINTERCAPTURECHANGED) msg = POINTER_CANCELLED;

	switch (pointerInfo.pointerType)
	{
	case PT_MOUSE:
		break;
	case PT_TOUCH:
		POINTER_TOUCH_INFO touchInfo;
		GetPointerTouchInfo(pointerId, &touchInfo);
		data.flags = touchInfo.touchFlags;
		data.mask = touchInfo.touchMask;
		data.rotation = touchInfo.orientation;
		data.pressure = touchInfo.pressure;
		break;
	case PT_PEN:
		POINTER_PEN_INFO penInfo;
		GetPointerPenInfo(pointerId, &penInfo);
		data.flags = penInfo.penFlags;
		data.mask = penInfo.penMask;
		data.rotation = penInfo.rotation;
		data.pressure = penInfo.pressure;
		data.tiltX = penInfo.tiltX;
		data.tiltY = penInfo.tiltY;
		break;
	}

	_delegate(pointerId, msg, pointerInfo.pointerType, position, data);
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

		Vector2 position = Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY);
		PointerData data {};

		if ((touch.dwFlags & TOUCHEVENTF_DOWN) != 0)
		{
			msg = WM_POINTERDOWN;
			data.changedButtons = POINTER_CHANGE_FIRSTBUTTON_DOWN;
		}
		else if ((touch.dwFlags & TOUCHEVENTF_UP) != 0)
		{
			msg = WM_POINTERLEAVE;
			data.changedButtons = POINTER_CHANGE_FIRSTBUTTON_UP;
		}
		else if ((touch.dwFlags & TOUCHEVENTF_MOVE) != 0)
		{
			msg = WM_POINTERUPDATE;
		}

		_delegate(touch.dwID, msg, PT_TOUCH, position, data);
	}

	CloseTouchInputHandle((HTOUCHINPUT)lParam);
	delete[] pInputs;
}

void log(const wchar_t* str)
{
#if _DEBUG
	BSTR bstr = SysAllocString(str);
	_log(bstr);
	SysFreeString(bstr);
#endif
}