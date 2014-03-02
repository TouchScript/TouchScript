using System;
using System.Windows;
using Microsoft.Phone.Info;

namespace TouchScript.WindowsPhone
{
    // http://developer.nokia.com/resources/library/Lumia/optimising-for-large-screen-phones/resolution-specific-considerations.html
    // http://blogs.windows.com/windows_phone/b/wpdev/archive/2013/11/22/taking-advantage-of-large-screen-windows-phones.aspx
    public class WP8Utils
    {
        public enum Resolutions
        {
            WVGA,
            WXGA,
            HD720p,
            HD1080p
        }

        private static bool IsWvga
        {
            get { return Application.Current.Host.Content.ScaleFactor == 100; }
        }

        private static bool IsWxga
        {
            get { return Application.Current.Host.Content.ScaleFactor == 160; }
        }

        private static bool Is720p
        {
            get { return Application.Current.Host.Content.ScaleFactor == 150 && !Is1080p; }
        }

        private static bool Is1080p
        {
            get
            {
                updateSize();
                return screenSize.Width == 1080;
            }
        }

        private static Size screenSize;

        public static Resolutions CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolutions.WVGA;
                if (IsWxga) return Resolutions.WXGA;
                if (Is720p) return Resolutions.HD720p;
                if (Is1080p) return Resolutions.HD1080p;
                throw new InvalidOperationException("Unknown resolution");
            }
        }

        public static bool GetExtendedScreenInfo(out int width, out int height, out float dpi)
        {
            updateSize();
            if (screenSize.Width == 0)
            {
                dpi = width = height = 0;
                return false;
            }

            dpi = Convert.ToSingle(DeviceExtendedProperties.GetValue("RawDpiX"));
            width = (int)screenSize.Width;
            height = (int)screenSize.Height;

            return true;
        }

        private static void updateSize()
        {
            if (screenSize.Width == 0)
            {
                try
                {
                    screenSize = (Size)DeviceExtendedProperties.GetValue("PhysicalScreenResolution");
                } catch (Exception)
                {
                    screenSize.Width = 0;
                }
            }
        }
    }
}
