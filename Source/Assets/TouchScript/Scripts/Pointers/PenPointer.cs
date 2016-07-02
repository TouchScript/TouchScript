/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class PenPointer : Pointer
    {

        #region Constructor

        public PenPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Pen;
        }

        #endregion

    }
}
