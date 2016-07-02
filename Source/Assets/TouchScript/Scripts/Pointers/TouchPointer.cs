/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class TouchPointer : Pointer
    {

        #region Constructor

        public TouchPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Touch;
        }

        #endregion

    }
}
