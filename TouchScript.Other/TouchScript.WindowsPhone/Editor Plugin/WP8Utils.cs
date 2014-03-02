using System;

namespace TouchScript.WindowsPhone
{
    public class WP8Utils
    {
        public enum Resolutions
        {
            WVGA,
            WXGA,
            HD720p,
            HD1080p
        }

        public static Resolutions CurrentResolution
        {
            get { return Resolutions.WVGA; }
        }

        public static bool GetExtendedScreenInfo(out int width, out int height, out float dpi)
        {
            dpi = 144;
            width = 480;
            height = 800;

            return false;
        }
    }
}