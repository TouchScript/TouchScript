/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    /// <summary>
    /// Static factory to create pointers.
    /// </summary>
    public static class PointerFactory
    {
        /// <summary>
        /// Creates a pointer of certain type attached to the input source.
        /// </summary>
        /// <param name="type">Pointer type to create.</param>
        /// <param name="input">Input source to attach the pointer to.</param>
        /// <returns></returns>
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