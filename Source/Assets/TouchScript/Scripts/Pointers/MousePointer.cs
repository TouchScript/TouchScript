/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{

    /// <summary>
    /// A pointer of type <see cref="Pointer.PointerType.Mouse"/>.
    /// </summary>
    public class MousePointer : Pointer
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MousePointer"/> class.
        /// </summary>
        public MousePointer(IInputSource input) : base(input)
        {
            Type = PointerType.Mouse;
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
