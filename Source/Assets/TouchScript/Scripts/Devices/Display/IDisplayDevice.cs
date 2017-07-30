/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Devices.Display
{
    /// <summary>
    /// Represents a device which is used to display touch interface. Incapsulating such properties as <see cref="DPI"/>.
    /// </summary>
    /// <remarks>
    /// <para>TouchScript uses display device to calculate gesture properties based on device's DPI. This makes it possible to have the same experience on mobile devices with high DPI and large touch surfaces which have low DPI.</para>
    /// <para>Current instance of <see cref="IDisplayDevice"/> can be accessed via <see cref="ITouchManager.DisplayDevice"/>.</para>
    /// </remarks>
    public interface IDisplayDevice
    {
        /// <summary>
        /// Name of the display device.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// DPI of the game based on <see cref="NativeResolution"/> and <see cref="NativeDPI"/>.
        /// </summary>
        float DPI { get; }

        /// <summary>
        /// Native DPI of the display device.
        /// </summary>
        float NativeDPI { get; }

        /// <summary>
        /// Native resolution of the display device.
        /// </summary>
        Vector2 NativeResolution { get; }

        /// <summary>
        /// Forces to recalculate <see cref="DPI"/>.
        /// </summary>
        void UpdateDPI();
    }
}