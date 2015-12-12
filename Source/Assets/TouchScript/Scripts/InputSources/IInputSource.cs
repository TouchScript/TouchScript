/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TouchScript.InputSources
{
    /// <summary>
    /// An object which represents an input source.
    /// </summary>
    /// <remarks>
    /// <para>In TouchScript all touch points (<see cref="TouchPoint"/>) come from input sources.</para>
    /// <para>If you want to feed the library with touches the best way to do it is to create a custom input source.</para>
    /// </remarks>
    public interface IInputSource
    {
        /// <summary>
        /// Gets or sets current coordinates remapper.
        /// </summary>
        /// <value>An object used to change coordinates of touch points coming from this input source.</value>
        ICoordinatesRemapper CoordinatesRemapper { get; set; }

        /// <summary>
        /// This method is called by <see cref="TouchManagerInstance"/> to synchronously update the input.
        /// </summary>
        void UpdateInput();

        void ReturnTouch(TouchPoint touch);
    }
}