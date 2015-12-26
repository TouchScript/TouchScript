/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// An object which changes touch coordinates coming from an input source.
    /// </summary>
    /// <remarks>
    /// If your input device is not fully aligned with display device you can use a remapper to carefully retarget touch positions to "calibrate" input with image.
    /// </remarks>
    public interface ICoordinatesRemapper
    {
        /// <summary>
        /// Remaps touch input.
        /// </summary>
        /// <param name="input">Original coordinates.</param>
        /// <returns>Changed coordinates.</returns>
        Vector2 Remap(Vector2 input);
    }
}