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

typedef void(__stdcall * PointerBeganFuncPtr)(int id, POINTER_INPUT_TYPE type, Vector2 position);
typedef void(__stdcall * PointerMovedFuncPtr)(int id, Vector2 position);
typedef void(__stdcall * PointerEndedFuncPtr)(int id);
typedef void(__stdcall * PointerCancelledFuncPtr)(int id);

PointerBeganFuncPtr			_pointerBeganFunc;
PointerMovedFuncPtr			_pointerMovedFunc;
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
	EXPORT_API void __stdcall Init(TOUCH_API api, PointerBeganFuncPtr began, PointerMovedFuncPtr moved, 
		PointerEndedFuncPtr ended, PointerCancelledFuncPtr cancelled);
	EXPORT_API void __stdcall SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY);
	EXPORT_API void __stdcall Dispose();
}

LRESULT CALLBACK wndProc8(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK wndProc7(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
void decodeWin8Touches(UINT msg, WPARAM wParam, LPARAM lParam);
void decodeWin7Touches(UINT msg, WPARAM wParam, LPARAM lParam);