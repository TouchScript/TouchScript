/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text.RegularExpressions;
#if UNITY_STANDALONE_WIN
using TouchScript.Utils.Platform;
#endif
using UnityEngine;

namespace TouchScript.Devices.Display
{
    /// <summary>
    /// Display device which tries to guess current DPI if it's not set by platform.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Devices_Display_GenericDisplayDevice.htm")]
    public class GenericDisplayDevice : DisplayDevice
    {
        private static bool IsLaptop
        {
            get
            {
                if (isLaptop == null)
                {
                    var gpuName = SystemInfo.graphicsDeviceName.ToLower();
                    var regex = new Regex(@"^(.*mobile.*|intel hd graphics.*|.*m\s*(series)?\s*(opengl engine)?)$", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(gpuName)) isLaptop = true;
                    else isLaptop = false;
                }
                return isLaptop == true;
            }
        }

        private static bool? isLaptop = null;

        /// <inheritdoc />
        public override void UpdateDPI()
        {
            if (Screen.fullScreen)
            {
                var res = Screen.currentResolution;
                dpi = Mathf.Max(res.width / nativeResolution.x, res.height / nativeResolution.y) * nativeDPI;
            }
            else
            {
                dpi = nativeDPI;
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            Name = Application.platform.ToString();
            if (IsLaptop) Name += " (Laptop)";

            updateNativeResulotion();
            updateNativeDPI();
            UpdateDPI();
        }

        private void updateNativeResulotion()
        {
            switch (Application.platform)
            {
                // Editors / windowed
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    // This has not been tested and is probably wrong.
                    if (getHighestResolution(out nativeResolution)) break;
                    var res = Screen.currentResolution;
                    nativeResolution = new Vector2(res.width, res.height);
                    break;
                // Mobiles / fullscreen
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
#if !UNITY_2018_2_OR_NEWER
                case RuntimePlatform.TizenPlayer:
#endif
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    // This has not been tested and is probably wrong.
                    if (getHighestResolution(out nativeResolution)) break;
                    res = Screen.currentResolution;
                    nativeResolution = new Vector2(res.width, res.height);
                    break;
                // PCs
                case RuntimePlatform.WindowsPlayer:
#if UNITY_STANDALONE_WIN
                    int width, height;
                    WindowsUtils.GetNativeMonitorResolution(out width, out height);
                    nativeResolution = new Vector2(width, height);
#endif
                    break;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WebGLPlayer:
                    // This has not been tested and is probably wrong.
                    if (getHighestResolution(out nativeResolution)) break;
                    res = Screen.currentResolution;
                    nativeResolution = new Vector2(res.width, res.height);
                    break;
                // Probably TVs
#if !UNITY_2017_3_OR_NEWER
                case RuntimePlatform.SamsungTVPlayer:
#endif
#if UNITY_5_6_OR_NEWER
                case RuntimePlatform.Switch:
#endif
#if !UNITY_2018_1_OR_NEWER
                case RuntimePlatform.WiiU:
#endif
                case RuntimePlatform.XboxOne:
                case RuntimePlatform.tvOS:
                    // This has not been tested and is probably wrong.
                    if (getHighestResolution(out nativeResolution)) break;
                    res = Screen.currentResolution;
                    nativeResolution = new Vector2(res.width, res.height);
                    break;
#if !UNITY_2018_3_OR_NEWER
                case RuntimePlatform.PSP2:
                    nativeResolution = new Vector2(960, 544);
                    break;
#endif
                default:
                    // This has not been tested and is probably wrong.
                    if (getHighestResolution(out nativeResolution)) break;
                    res = Screen.currentResolution;
                    nativeResolution = new Vector2(res.width, res.height);
                    break;
            }
        }

        private void updateNativeDPI()
        {
            nativeDPI = Screen.dpi;
            if (nativeDPI > float.Epsilon) return;

            var res = Screen.currentResolution;
            var width = Mathf.Max(res.width, res.height);
            var height = Mathf.Min(res.width, res.height);

            switch (Application.platform)
            {
                // Editors / windowed
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                // PCs
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WebGLPlayer:
                    // This has not been tested and is probably wrong.
                    // Let's guess
                    if (width >= 3840)
                    {
                        if (height <= 2160) dpi = 150; // 28-31"
                        else dpi = 200;
                    }
                    else if (width >= 2880 && height == 1800) dpi = 220; // 15" retina
                    else if (width >= 2560)
                    {
                        if (height >= 1600)
                        {
                            if (IsLaptop) dpi = 226; // 13.3" retina
                            else dpi = 101; // 30" display
                        }
                        else if (height >= 1440) dpi = 109; // 27" iMac
                    }
                    else if (width >= 2048)
                    {
                        if (height <= 1152) dpi = 100; // 23-27"
                        else dpi = 171; // 15" laptop
                    }
                    else if (width >= 1920)
                    {
                        if (height >= 1440) dpi = 110; // 24"
                        else if (height >= 1200) dpi = 90; // 26-27"
                        else if (height >= 1080)
                        {
                            if (IsLaptop) dpi = 130; // 15" - 18" laptop
                            else dpi = 92; // +-24" display
                        }
                    }
                    else if (width >= 1680) dpi = 129; // 15" laptop
                    else if (width >= 1600) dpi = 140; // 13" laptop
                    else if (width >= 1440)
                    {
                        if (height >= 1050) dpi = 125; // 14" laptop
                        else dpi = 110; // 13" air or 15" macbook pro
                    }
                    else if (width >= 1366) dpi = 125; // 10"-14" laptops
                    else if (width >= 1280) dpi = 110;
                    else dpi = 96;
                    break;
                // Mobiles / fullscreen
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
#if !UNITY_2018_2_OR_NEWER
                case RuntimePlatform.TizenPlayer:
#endif
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    // We just hope that mobiles report their DPI correctly
                    break;
                // Probably TVs
#if !UNITY_2017_3_OR_NEWER
                case RuntimePlatform.SamsungTVPlayer:
#endif
#if UNITY_5_6_OR_NEWER
                case RuntimePlatform.Switch:
#endif
#if !UNITY_2018_1_OR_NEWER
                case RuntimePlatform.WiiU:
#endif
                case RuntimePlatform.XboxOne:
                case RuntimePlatform.tvOS:
                    // This has not been tested and is probably wrong.
                    if (width >= 3840)
                    {
                        nativeDPI = 96;
                    }
                    else if (width >= 1920)
                    {
                        nativeDPI = 50;
                    }
                    else
                    {
                        nativeDPI = 40;
                    }
                    break;
#if !UNITY_2018_3_OR_NEWER
                case RuntimePlatform.PSP2:
                    nativeDPI = 220.68f;
                    break;
#endif
                default:
                    // This has not been tested and is probably wrong.
                    nativeDPI = 160;
                    break;
            }
        }

        private bool getHighestResolution(out Vector2 resolution)
        {
            resolution = new Vector2();

            var resolutions = Screen.resolutions;
            if (resolutions.Length == 0) return false;

            var r = resolutions[resolutions.Length - 1];
            resolution = new Vector2(r.width, r.height);
            return true;
        }
    }
}