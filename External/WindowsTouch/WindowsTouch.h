/*
* @author Valentin Simonov / http://va.lent.in/
*/

#include <windows.h>

#define EXPORT_API __declspec(dllexport) 

struct Vector2
{
	float x, y;

	Vector2(float x, float y)
	{
		this->x = x;
		this->y = y;
	}
};

typedef enum
{
	WIN7,
	WIN8
} TOUCH_API;

typedef void(__stdcall * MousePointerBeganFuncPtr)(int id, unsigned int buttons, Vector2 position);
typedef void(__stdcall * MousePointerMovedFuncPtr)(int id, unsigned int buttonsSet, unsigned int buttonsClear, Vector2 position);
typedef void(__stdcall * TouchPointerBeganFuncPtr)(int id, unsigned int buttons, unsigned int orientation, unsigned int pressure, Vector2 position);
typedef void(__stdcall * TouchPointerMovedFuncPtr)(int id, unsigned int buttonsSet, unsigned int orientation, unsigned int pressure, unsigned int buttonsClear, Vector2 position);
typedef void(__stdcall * PenPointerBeganFuncPtr)(int id, unsigned int buttons, Vector2 position);
typedef void(__stdcall * PenPointerMovedFuncPtr)(int id, unsigned int buttonsSet, unsigned int buttonsClear, Vector2 position);
typedef void(__stdcall * PointerEndedFuncPtr)(int id, POINTER_INPUT_TYPE type, unsigned int buttons);
typedef void(__stdcall * PointerCancelledFuncPtr)(int id, POINTER_INPUT_TYPE type);

MousePointerBeganFuncPtr	_mousePointerBeganFunc;
MousePointerMovedFuncPtr	_mousePointerMovedFunc;
TouchPointerBeganFuncPtr	_touchPointerBeganFunc;
TouchPointerMovedFuncPtr	_touchPointerMovedFunc;
PenPointerBeganFuncPtr		_penPointerBeganFunc;
PenPointerMovedFuncPtr		_penPointerMovedFunc;
PointerEndedFuncPtr			_pointerEndedFunc;
PointerCancelledFuncPtr		_pointerCancelledFunc;
HWND						_currentWindow;
int							_screenWidth;
int							_screenHeight;
float						_offsetX = 0;
float						_offsetY = 0;
float						_scaleX = 1;
float						_scaleY = 1;
TOUCH_API					_api;
LONG_PTR					_oldWindowProc;

extern "C" 
{
	EXPORT_API void __stdcall Init(TOUCH_API api, 
		MousePointerBeganFuncPtr mouseBegan, MousePointerMovedFuncPtr mouseMoved,
		TouchPointerBeganFuncPtr touchBegan, TouchPointerMovedFuncPtr touchMoved,
		PenPointerBeganFuncPtr penBegan, PenPointerMovedFuncPtr penMoved,
		PointerEndedFuncPtr ended, PointerCancelledFuncPtr cancelled);
	EXPORT_API void __stdcall SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY);
	EXPORT_API void __stdcall Dispose();
}

LRESULT CALLBACK wndProc8(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK wndProc7(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
void decodeWin8Touches(UINT msg, WPARAM wParam, LPARAM lParam);
void decodeWin7Touches(UINT msg, WPARAM wParam, LPARAM lParam);