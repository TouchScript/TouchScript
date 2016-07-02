/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class MousePointer : Pointer
    {

        public MousePointer(IInputSource input) : base(input)
        {
            Type = PointerType.Mouse;
        }

    }
}
