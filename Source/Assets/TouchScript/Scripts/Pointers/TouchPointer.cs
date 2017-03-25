/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    /// <summary>
    /// A pointer of type <see cref="Pointer.PointerType.Touch"/>.
    /// </summary>
    public class TouchPointer : Pointer
    {

        #region Public properties

        public uint Orientation { get; set; }

        public float Pressure { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPointer"/> class.
        /// </summary>
        public TouchPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Touch;
        }

        #endregion

        #region Internal functions

        //internal override void INTERNAL_Reset()
        //{
        //    base.INTERNAL_Reset();
        //}

        #endregion

    }
}
