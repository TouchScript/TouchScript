/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{

    /// <summary>
    /// A pointer of type <see cref="Pointer.PointerType.Pen"/>.
    /// </summary>
    public class PenPointer : Pointer
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PenPointer"/> class.
        /// </summary>
        public PenPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Pen;
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
