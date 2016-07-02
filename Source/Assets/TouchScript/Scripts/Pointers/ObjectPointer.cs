/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class ObjectPointer : Pointer
    {

        public ObjectPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Mouse;
        }

    }
}
