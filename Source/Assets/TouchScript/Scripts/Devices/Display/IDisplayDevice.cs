/*
 * @author Valentin Simonov / http://va.lent.in/
 */

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
        /// Gets or sets the name of display device.
        /// </summary>
        /// <value> The name of display device. </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets DPI of display device.
        /// </summary>
        /// <value> DPI used by display device. </value>
        float DPI { get; set; }
    }
}