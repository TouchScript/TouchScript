/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class TouchPointer : Pointer
    {

        public TouchPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Touch;
        }

    }
}
