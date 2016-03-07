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

typedef void(__stdcall * PointerDownFuncPtr)(int id, POINTER_INPUT_TYPE type, Vector2 position, UINT flags);
typedef void(__stdcall * PointerUpdateFuncPtr)(int id, Vector2 position, UINT flags);
typedef void(__stdcall * PointerUpFuncPtr)(int id, UINT flags);
typedef void(__stdcall * PingFuncPtr)(int value);

PointerDownFuncPtr		_pointerDownFunc;
PointerUpdateFuncPtr	_pointerUpdateFunc;
PointerUpFuncPtr		_pointerUpFunc;
HWND					_currentWindow;
int						_screenWidth;
int						_screenHeight;
float					_offsetX = 0;
float					_offsetY = 0;
float					_scaleX = 1;
float					_scaleY = 1;
LONG_PTR				_oldWindowProc;

PingFuncPtr				_ping;

extern "C" 
{
	EXPORT_API void __stdcall Init(TOUCH_API api, PointerDownFuncPtr down, PointerUpdateFuncPtr update, PointerUpFuncPtr up);
	EXPORT_API void __stdcall SetScreenParams(int width, int height, float offsetX, float offsetY, float scaleX, float scaleY);
	EXPORT_API void __stdcall Dispose();
	EXPORT_API void __stdcall TestPing(PingFuncPtr ping);
}

LRESULT CALLBACK wndProc8(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
void decodeWin8Touches(UINT msg, WPARAM wParam, LPARAM lParam);