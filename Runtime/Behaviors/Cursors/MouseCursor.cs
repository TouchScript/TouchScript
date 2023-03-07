/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Behaviors.Cursors.UI;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Cursor for mouse pointers.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_MouseCursor.htm")]
    public class MouseCursor : TextPointerCursor<MousePointer>
    {
        #region Public properties

        /// <summary>
        /// Default cursor sub object.
        /// </summary>
        public TextureSwitch DefaultCursor;

        /// <summary>
        /// Pressed cursor sub object.
        /// </summary>
        public TextureSwitch PressedCursor;

        /// <summary>
        /// Should the value of <see cref="Pointer.Buttons"/> be shown on the cursor.
        /// </summary>
        public bool ShowButtons = false;

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void updateOnce(IPointer pointer)
        {
            switch (state)
            {
                case CursorState.Released:
                case CursorState.Over:
                    if (DefaultCursor != null) DefaultCursor.Show();
                    if (PressedCursor != null) PressedCursor.Hide();
                    break;
                case CursorState.Pressed:
                case CursorState.OverPressed:
                    if (DefaultCursor != null) DefaultCursor.Hide();
                    if (PressedCursor != null) PressedCursor.Show();
                    break;
            }

            base.updateOnce(pointer);
        }

        /// <inheritdoc />
        protected override void generateText(MousePointer pointer, StringBuilder str)
        {
            base.generateText(pointer, str);

            if (ShowButtons)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Buttons: ");
                PointerUtils.PressedButtonsToString(pointer.Buttons, str);
            }
        }

        /// <inheritdoc />
        protected override bool textIsVisible()
        {
            return base.textIsVisible() || ShowButtons;
        }

        /// <inheritdoc />
        protected override uint gethash(MousePointer pointer)
        {
            var hash = base.gethash(pointer);

            if (ShowButtons) hash += (uint) (pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed);

            return hash;
        }

        #endregion
    }
}