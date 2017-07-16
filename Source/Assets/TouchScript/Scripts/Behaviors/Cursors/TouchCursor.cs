/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;

namespace TouchScript.Behaviors.Cursors
{
    public class TouchCursor : TextPointerCursor<TouchPointer>
    {
        #region Public properties

        public bool ShowPressure = false;

        public bool ShowRotation = false;

        #endregion

        #region Public methods

        #endregion

        #region Protected methods

        protected override void generateText(TouchPointer pointer, StringBuilder str)
        {
            base.generateText(pointer, str);

            if (ShowPressure)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Pressure: ");
                str.AppendFormat("{0:0.000}", pointer.Pressure);
            }
            if (ShowRotation)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Rotation: ");
                str.Append(pointer.Rotation);
            }
        }

        protected override bool shouldShowText()
        {
            return base.shouldShowText() || ShowPressure || ShowRotation;
        }

        protected override uint gethash(TouchPointer pointer)
        {
            var hash = base.gethash(pointer);

            if (ShowPressure == true) hash += (uint) (pointer.Pressure * 1024) << 8;
            if (ShowRotation == true) hash += (uint) (pointer.Rotation * 1024) << 16;

            return hash;
        }

        #endregion
    }
}