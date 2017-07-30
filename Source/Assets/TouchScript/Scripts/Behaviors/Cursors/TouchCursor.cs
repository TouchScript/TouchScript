/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Cursor for touch pointers.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_TouchCursor.htm")]
    public class TouchCursor : TextPointerCursor<TouchPointer>
    {
        #region Public properties

        /// <summary>
        /// Should the value of <see cref="TouchPointer.Pressure"/> be shown on the cursor.
        /// </summary>
        public bool ShowPressure = false;

        /// <summary>
        /// Should the value of <see cref="TouchPointer.Rotation"/> be shown on the cursor.
        /// </summary>
        public bool ShowRotation = false;

        #endregion

        #region Protected methods

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override bool textIsVisible()
        {
            return base.textIsVisible() || ShowPressure || ShowRotation;
        }

        /// <inheritdoc />
        protected override uint gethash(TouchPointer pointer)
        {
            var hash = base.gethash(pointer);

            if (ShowPressure) hash += (uint) (pointer.Pressure * 1024) << 8;
            if (ShowRotation) hash += (uint) (pointer.Rotation * 1024) << 16;

            return hash;
        }

        #endregion
    }
}