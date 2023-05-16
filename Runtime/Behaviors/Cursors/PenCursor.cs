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
    /// Cursor for pen pointers.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_PenCursor.htm")]
    public class PenCursor : TextPointerCursor<PenPointer>
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

        /// <summary>
        /// Should the value of <see cref="PenPointer.Pressure"/> be shown on the cursor.
        /// </summary>
        public bool ShowPressure = false;

        /// <summary>
        /// Should the value of <see cref="PenPointer.Pressure"/> be shown on the cursor.
        /// </summary>
        public bool ShowRotation = false;

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void UpdateOnce(IPointer pointer)
        {
            switch (State)
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

            base.UpdateOnce(pointer);
        }

        /// <inheritdoc />
        protected override void GenerateText(PenPointer pointer, StringBuilder str)
        {
            base.GenerateText(pointer, str);

            if (ShowButtons)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Buttons: ");
                PointerUtils.PressedButtonsToString(pointer.Buttons, str);
            }
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
        protected override bool TextIsVisible()
        {
            return base.TextIsVisible() || ShowButtons || ShowPressure || ShowRotation;
        }

        /// <inheritdoc />
        protected override uint GetHash(PenPointer pointer)
        {
            var hash = base.GetHash(pointer);

            if (ShowButtons) hash += (uint) (pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed);
            if (ShowPressure) hash += (uint) (pointer.Pressure * 1024) << 8;
            if (ShowRotation) hash += (uint) (pointer.Rotation * 1024) << 16;

            return hash;
        }

        #endregion
    }
}