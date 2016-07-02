/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Pointers;

namespace TouchScript.InputSources
{
    /// <summary>
    /// An object which represents an input source.
    /// </summary>
    /// <remarks>
    /// <para>In TouchScript all pointer points (<see cref="Pointer"/>) come from input sources.</para>
    /// <para>If you want to feed pointers to the library the best way to do it is to create a custom input source.</para>
    /// </remarks>
    public interface IInputSource : INTERNAL_IInputSource
    {
        /// <summary>
        /// This method is called by <see cref="TouchManagerInstance"/> to synchronously update the input.
        /// </summary>
        void UpdateInput();

        /// <summary>
        /// Cancels the pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="return">if set to <c>true</c> returns the pointer back to the system with different id.</param>
        /// <returns><c>True</c> if the pointer belongs to this Input and was successfully cancelled; <c>false</c> otherwise.</returns>
        bool CancelPointer(Pointer pointer, bool @return);
    }

    public interface IRemapableInputSource
    {
        /// <summary>
        /// Gets or sets current coordinates remapper.
        /// </summary>
        /// <value>An object used to change coordinates of pointer points coming from this input source.</value>
        ICoordinatesRemapper CoordinatesRemapper { get; set; }
    }

    public interface INTERNAL_IInputSource
    {
        void INTERNAL_ReleasePointer(Pointer pointer);
    }
}