/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Pointers;

namespace TouchScript.Layers
{
    /// <summary>
    /// <para>A delegate which can be set to <see cref="TouchLayer.Delegate"/> and control what this layer can or can not do.</para>
    /// <seealso cref="TouchLayer"/>
    /// </summary>
    public interface ILayerDelegate
    {

        /// <summary>
        /// Returns whether a layer should receive the pointer.
        /// </summary>
        /// <param name="layer"> The layer. </param>
        /// <param name="pointer"> The pointer. </param>
        /// <returns> <c>true</c> if it should; <c>false</c> otherwise. </returns>
        bool ShouldReceivePointer(TouchLayer layer, IPointer pointer);

    }
}
