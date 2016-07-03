/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    public class ObjectPointer : Pointer
    {
        #region Public properties

        public int ObjectId { get; internal set; }

        public float Width { get; internal set; }

        public float Height { get; internal set; }

        public float Angle { get; internal set; }

        #endregion

        #region Constructor

        public ObjectPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Object;
        }

        #endregion

        #region Public methods

        public override void CopyFrom(Pointer target)
        {
            base.CopyFrom(target);
            var obj = target as ObjectPointer;
            if (obj == null) return;

            ObjectId = obj.ObjectId;
            Width = obj.Width;
            Height = obj.Height;
            Angle = obj.Angle;
        }

        #endregion
    }
}