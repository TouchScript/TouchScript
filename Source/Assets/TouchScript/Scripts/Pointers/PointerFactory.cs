/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public static class PointerFactory
    {

        public static Pointer Create(Pointer.PointerType type, IInputSource input)
        {
            switch (type)
            {
                case Pointer.PointerType.Touch:
                    return new TouchPointer(input);
                case Pointer.PointerType.Mouse:
                    return new MousePointer(input);
                case Pointer.PointerType.Pen:
                    return new PenPointer(input);
                case Pointer.PointerType.Object:
                    return new ObjectPointer(input);
            }
            return null;
        }

    }
}
