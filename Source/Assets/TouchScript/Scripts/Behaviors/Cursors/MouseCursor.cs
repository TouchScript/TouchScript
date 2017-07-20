/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    public class MouseCursor : TextPointerCursor<MousePointer>
    {
        #region Public properties

        public GameObject DefaultCursor;
        public GameObject PressedCursor;

        public bool ShowButtons = false;

        #endregion

        #region Public methods

        #endregion

        #region Protected methods

        protected override void updateOnce(IPointer pointer)
        {
            switch (state)
            {
                case ProxyState.Released:
                case ProxyState.Over:
                    if (DefaultCursor != null) DefaultCursor.SetActive(true);
                    if (PressedCursor != null) PressedCursor.SetActive(false);
                    break;
                case ProxyState.Pressed:
                case ProxyState.OverPressed:
                    if (DefaultCursor != null) DefaultCursor.SetActive(false);
                    if (PressedCursor != null) PressedCursor.SetActive(true);
                    break;
            }

            base.updateOnce(pointer);
        }

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

        protected override bool shouldShowText()
        {
            return base.shouldShowText() || ShowButtons;
        }

        protected override uint gethash(MousePointer pointer)
        {
            var hash = base.gethash(pointer);

            if (ShowButtons == true) hash += (uint) (pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed);

            return hash;
        }

        #endregion
    }
}