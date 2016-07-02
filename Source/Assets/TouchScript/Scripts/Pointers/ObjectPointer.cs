/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class ObjectPointer : Pointer
    {

        #region Constructor

        public ObjectPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Object;
        }

        #endregion

    }
}
