/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TouchScript.Layers
{
    /// <summary>
    /// <para>A delegate which can be set to <see cref="TouchLayer.Delegate"/> and control what this layer can or can not do.</para>
    /// <seealso cref="TouchLayer"/>
    /// </summary>
    public interface ILayerDelegate
    {

        /// <summary>
        /// Returns whether a layer should receive the touch.
        /// </summary>
        /// <param name="layer"> The layer. </param>
        /// <param name="touch"> The touch. </param>
        /// <returns> <c>true</c> if it should; <c>false</c> otherwise. </returns>
        bool ShouldReceiveTouch(TouchLayer layer, TouchPoint touch);

    }
}
