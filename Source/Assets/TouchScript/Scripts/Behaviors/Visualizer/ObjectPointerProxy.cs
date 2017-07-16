/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;

namespace TouchScript.Behaviors.Visualizer
{
    public class ObjectPointerProxy : TextPointerProxy<ObjectPointer>
    {
        #region Public properties

        public bool ShowObjectId = false;

        public bool ShowSize = false;

        public bool ShowAngle = false;

        #endregion

        #region Public methods

        #endregion

        #region Protected methods

        protected override void generateText(ObjectPointer pointer, StringBuilder str)
        {
            base.generateText(pointer, str);

            if (ShowObjectId)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("ObjectId: ");
                str.Append(pointer.ObjectId);
            }
            if (ShowSize)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Size: ");
                str.Append(pointer.Width);
                str.Append("x");
                str.Append(pointer.Height);
            }
            if (ShowAngle)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Angle: ");
                str.Append(pointer.Angle);
            }
        }

        protected override bool shouldShowText()
        {
            return base.shouldShowText() || ShowObjectId || ShowSize || ShowAngle;
        }

        protected override uint gethash(ObjectPointer pointer)
        {
            var hash = base.gethash(pointer);

            if (ShowSize == true) hash += (uint) (pointer.Width * 1024 + pointer.Height * 1024 * 1024) << 8;
            if (ShowAngle == true) hash += (uint) (pointer.Angle * 1024) << 24;

            return hash;
        }

        #endregion
    }
}