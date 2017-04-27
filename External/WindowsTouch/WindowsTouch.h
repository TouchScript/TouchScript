/*
* @author Valentin Simonov / http://va.lent.in/
*/

#define WINVER				_WIN32_WINNT_WIN7
#define _WIN32_WINNT		_WIN32_WINNT_WIN7

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

// <Windows 8 touch API>

#define WM_POINTERUPDATE	0x0245
#define WM_POINTERDOWN		0x0246
#define WM_POINTERUP		0x0247

#define POINTER_FLAG_CANCELED		0x00008000 // Pointer is departing in an abnormal manner

#define GET_POINTERID_WPARAM(wParam)	(LOWORD(wParam))

typedef UINT32 POINTER_FLAGS;

typedef enum {
	PT_POINTER				= 0x00000001,
	PT_TOUCH				= 0x00000002,
	PT_PEN					= 0x00000003,
	PT_MOUSE				= 0x00000004,
	PT_TOUCHPAD				= 0x00000005
} POINTER_INPUT_TYPE;

typedef enum {
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
} POINTER_BUTTON_CHANGE_TYPE;

typedef struct {
	POINTER_INPUT_TYPE    pointerType;
	UINT32          pointerId;
	UINT32          frameId;
	POINTER_FLAGS   pointerFlags;
	HANDLE          sourceDevice;
	HWND            hwndTarget;
	POINT           ptPixelLocation;
	POINT           ptHimetricLocation;
	POINT           ptPixelLocationRaw;
	POINT           ptHimetricLocationRaw;
	DWORD           dwTime;
	UINT32          historyCount;
	INT32           InputData;
	DWORD           dwKeyStates;
	UINT64          PerformanceCount;
	POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
} POINTER_INFO;

typedef BOOL (WINAPI *GET_POINTER_INFO)(UINT32 pointerId, POINTER_INFO *pointerInfo);

GET_POINTER_INFO	GetPointerInfo;

// </Windows 8 touch API>

typedef void(__stdcall * PointerBeganFuncPtr)(int id, POINTER_INPUT_TYPE type, unsigned int buttons, Vector2 position);
typedef void(__stdcall * PointerMovedFuncPtr)(int id, POINTER_INPUT_TYPE type, unsigned int buttonsSet, unsigned int buttonsClear, Vector2 position);
typedef void(__stdcall * PointerEndedFuncPtr)(int id, POINTER_INPUT_TYPE type, unsigned int buttons);
typedef void(__stdcall * PointerCancelledFuncPtr)(int id, POINTER_INPUT_TYPE type);

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