/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TouchScript.InputSources
{
    /// <summary>
    /// Abstract touch input source.
    /// </summary>
    public interface IInputSource
    {
        /// <summary>
        /// An object used to change coordinates of touch points coming from this input source.
        /// </summary>
        ICoordinatesRemapper CoordinatesRemapper { get; set; }
    }
}
