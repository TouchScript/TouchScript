using UnityEngine;

namespace TouchScript.WindowsPhone.Devices.Display
{
    public class WindowsPhoneDisplayDevice : DisplayDevice
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            int width, height;
            if (!WP8Utils.GetExtendedScreenInfo(out width, out height, out dpi))
            {
                dpi = Screen.dpi;
            }
            if (dpi <= 0) dpi = 160f;
        }
    }
}
