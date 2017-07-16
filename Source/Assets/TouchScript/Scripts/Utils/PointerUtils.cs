/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Hit;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Pointer utils.
    /// </summary>
    public static class PointerUtils
    {
        /// <summary>
        /// Determines whether the pointer is over its target GameObject.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsPointerOnTarget(Pointer pointer)
        {
            if (pointer == null) return false;
            return IsPointerOnTarget(pointer, pointer.GetPressData().Target);
        }

        /// <summary>
        /// Determines whether the pointer is over a specific GameObject.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <param name="target"> The target. </param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise.</returns>
        public static bool IsPointerOnTarget(IPointer pointer, Transform target)
        {
            HitData hit;
            return IsPointerOnTarget(pointer, target, out hit);
        }

        /// <summary>
        /// Determines whether the pointer is over a specific GameObject.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="target">The target.</param>
        /// <param name="hit">The hit.</param>
        /// <returns> <c>true</c> if the pointer is over the GameObject; <c>false</c> otherwise. </returns>
        public static bool IsPointerOnTarget(IPointer pointer, Transform target, out HitData hit)
        {
            hit = default(HitData);
            if (pointer == null || target == null) return false;
            hit = pointer.GetOverData();
            if (hit.Target == null) return false;
            return hit.Target.IsChildOf(target);
        }

        public static void ButtonsToString(Pointer.PointerButtonState buttons, StringBuilder builder)
        {
            if ((buttons & Pointer.PointerButtonState.FirstButtonPressed) != 0) builder.Append("1");
            else builder.Append("_");
            if ((buttons & Pointer.PointerButtonState.SecondButtonPressed) != 0) builder.Append("2");
            else builder.Append("_");
            if ((buttons & Pointer.PointerButtonState.ThirdButtonPressed) != 0) builder.Append("3");
            else builder.Append("_");
            if ((buttons & Pointer.PointerButtonState.FourthButtonPressed) != 0) builder.Append("4");
            else builder.Append("_");
            if ((buttons & Pointer.PointerButtonState.FifthButtonPressed) != 0) builder.Append("5");
            else builder.Append("_");
        }

        public static Pointer.PointerButtonState DownPressedButtons(Pointer.PointerButtonState buttons)
        {
            var btns = buttons & Pointer.PointerButtonState.AnyButtonPressed;
            if ((btns & Pointer.PointerButtonState.FirstButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.FirstButtonDown;
            if ((btns & Pointer.PointerButtonState.SecondButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.SecondButtonDown;
            if ((btns & Pointer.PointerButtonState.ThirdButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.ThirdButtonDown;
            if ((btns & Pointer.PointerButtonState.FourthButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.FourthButtonDown;
            if ((btns & Pointer.PointerButtonState.FifthButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.FifthButtonDown;
            return btns;
        }

        public static Pointer.PointerButtonState PressDownButtons(Pointer.PointerButtonState buttons)
        {
            var btns = buttons;
            if ((btns & Pointer.PointerButtonState.FirstButtonDown) != 0)
                btns |= Pointer.PointerButtonState.FirstButtonPressed;
            if ((btns & Pointer.PointerButtonState.SecondButtonDown) != 0)
                btns |= Pointer.PointerButtonState.SecondButtonPressed;
            if ((btns & Pointer.PointerButtonState.ThirdButtonDown) != 0)
                btns |= Pointer.PointerButtonState.ThirdButtonPressed;
            if ((btns & Pointer.PointerButtonState.FourthButtonDown) != 0)
                btns |= Pointer.PointerButtonState.FourthButtonPressed;
            if ((btns & Pointer.PointerButtonState.FifthButtonDown) != 0)
                btns |= Pointer.PointerButtonState.FifthButtonPressed;
            return btns;
        }

        public static Pointer.PointerButtonState UpPressedButtons(Pointer.PointerButtonState buttons)
        {
            var btns = Pointer.PointerButtonState.Nothing;
            if ((btns & Pointer.PointerButtonState.FirstButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.FirstButtonUp;
            if ((btns & Pointer.PointerButtonState.SecondButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.SecondButtonUp;
            if ((btns & Pointer.PointerButtonState.ThirdButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.ThirdButtonUp;
            if ((btns & Pointer.PointerButtonState.FourthButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.FourthButtonUp;
            if ((btns & Pointer.PointerButtonState.FifthButtonPressed) != 0)
                btns |= Pointer.PointerButtonState.FifthButtonUp;
            return btns;
        }
    }
}