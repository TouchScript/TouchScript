#include "WindowsTouch.h"

extern "C" 
{

	void __stdcall Init(TOUCH_API api, PointerDownFuncPtr down, PointerUpdateFuncPtr update, PointerUpFuncPtr up)
	{
		_pointerDownFunc = down;
		_pointerUpdateFunc = update;
		_pointerUpFunc = up;

		_currentWindow = GetActiveWindow();
		if (api == WIN8)
		{
			_oldWindowProc = SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)wndProc8);
		}
	}

	void __stdcall Dispose()
	{
		if (_oldWindowProc)
		{
			SetWindowLongPtr(_currentWindow, GWLP_WNDPROC, (LONG_PTR)_oldWindowProc);
			_oldWindowProc = 0;
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

	void __stdcall TestPing(PingFuncPtr ping)
	{
		_ping = ping;
		_ping(42);
	}

}

LRESULT CALLBACK wndProc8(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_TOUCH:
		CloseTouchInputHandle((HTOUCHINPUT)lParam);
		return 0;
	case WM_POINTERDOWN:
	case WM_POINTERUP:
	case WM_POINTERUPDATE:
		decodeWin8Touches(msg, wParam, lParam);
		return 0;
	default:
		return CallWindowProc((WNDPROC)_oldWindowProc, hwnd, msg, wParam, lParam);
	}
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
		_pointerDownFunc(pointerId, pointerInfo.pointerType, Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY), pointerInfo.pointerFlags);
		break;
	case WM_POINTERUP:
		_pointerUpFunc(pointerId, pointerInfo.pointerFlags);
		break;
	case WM_POINTERUPDATE:
		_pointerUpdateFunc(pointerId, Vector2(((float)p.x - _offsetX) * _scaleX, _screenHeight - ((float)p.y - _offsetY) * _scaleY), pointerInfo.pointerFlags);
		break;
	}
}