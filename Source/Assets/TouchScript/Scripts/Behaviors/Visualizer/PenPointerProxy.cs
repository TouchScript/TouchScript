/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.Visualizer
{
    public class PenPointerProxy : TextPointerProxy<PenPointer>
    {
        #region Public properties

        public GameObject DefaultCursor;
        public GameObject PressedCursor;

        public bool ShowButtons = false;

        public bool ShowPressure = false;

        public bool ShowRotation = false;

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

        protected override void generateText(PenPointer pointer, StringBuilder str)
        {
            base.generateText(pointer, str);

            if (ShowButtons)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Buttons: ");
                PointerUtils.ButtonsToString(pointer.Buttons, str);
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

        protected override bool shouldShowText()
        {
            return base.shouldShowText() || ShowButtons || ShowPressure || ShowRotation;
        }

        protected override uint gethash(PenPointer pointer)
        {
            var hash = base.gethash(pointer);

            if (ShowButtons == true) hash += (uint) (pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed);
            if (ShowPressure == true) hash += (uint) (pointer.Pressure * 1024) << 8;
            if (ShowRotation == true) hash += (uint) (pointer.Rotation * 1024) << 16;

            return hash;
        }

        #endregion
    }
}