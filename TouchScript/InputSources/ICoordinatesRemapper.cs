/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Interface for objects which can remap screen coordinates.
    /// </summary>
    public interface ICoordinatesRemapper
    {
        /// <summary>
        /// Remaps the specified input.
        /// </summary>
        /// <param name="input">Original coordinates.</param>
        /// <returns>Changed coordinates.</returns>
        Vector2 Remap(Vector2 input);
    }
}