/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.UI
{

    /// <summary>
    /// This component listens for pointer events and dispatches <see cref="Over"/> event when the first touch enters the area of the GameObject it is attached to and <see cref="Out"/> event when the last touch leaves it.
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/OverHelper")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_UI_OverHelper.htm")]
    public class OverHelper : MonoBehaviour
    {

        #region Events

        /// <summary>
        /// Occurs when the first (non-pressed) touch enters the area of the GameObject.
        /// </summary>
        public event EventHandler Over;

        /// <summary>
        /// Occurs when the last touch leaves the area of the GameObject.
        /// </summary>
        public event EventHandler Out;

        #endregion

        #region Private variable

        private HashSet<int> pointers = new HashSet<int>(); 

        #endregion

        #region Unity methods

        private void OnEnable()
        {
            TouchManager.Instance.PointersAdded += pointersAddedHandler;
            TouchManager.Instance.PointersUpdated += pointersUpdatedHandler;
            TouchManager.Instance.PointersReleased += pointersReleasedHandler;
            TouchManager.Instance.PointersRemoved += pointersRemovedHandler;
            TouchManager.Instance.PointersCancelled += pointersRemovedHandler;
        }

        private void OnDisable()
        {
			if (TouchManager.Instance == null) return;
            TouchManager.Instance.PointersAdded -= pointersAddedHandler;
            TouchManager.Instance.PointersUpdated -= pointersUpdatedHandler;
            TouchManager.Instance.PointersReleased -= pointersReleasedHandler;
            TouchManager.Instance.PointersRemoved -= pointersRemovedHandler;
            TouchManager.Instance.PointersCancelled -= pointersRemovedHandler;
        }

        #endregion

        #region Private functions

        private void dispatchOver()
        {
            if (Over != null) Over.InvokeHandleExceptions(this, EventArgs.Empty);
        }

        private void dispatchOut()
        {
            if (Out != null) Out.InvokeHandleExceptions(this, EventArgs.Empty);
        }

        #endregion

        #region Callbacks

        private void pointersAddedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            var over = pointers.Count;
            var p = pointerEventArgs.Pointers;
            var count = p.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = p[i];
                if (PointerUtils.IsPointerOnTarget(pointer, transform)) pointers.Add(pointer.Id);
            }

            if (over == 0 && pointers.Count > 0) dispatchOver();
        }

        private void pointersUpdatedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            var over = pointers.Count;
            var p = pointerEventArgs.Pointers;
            var count = p.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = p[i];
                if ((pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) != 0) continue; // we ignore pressed pointers
                if (PointerUtils.IsPointerOnTarget(pointer, transform)) pointers.Add(pointer.Id);
                else pointers.Remove(pointer.Id);
            }

            if (over == 0 && pointers.Count > 0) dispatchOver();
            else if (over > 0 && pointers.Count == 0) dispatchOut();
        }

        private void pointersReleasedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            var over = pointers.Count;
            var p = pointerEventArgs.Pointers;
            var count = p.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = p[i];
                if (PointerUtils.IsPointerOnTarget(pointer, transform)) pointers.Add(pointer.Id);
                else pointers.Remove(pointer.Id);
            }

            if (over == 0 && pointers.Count > 0) dispatchOver();
            else if (over > 0 && pointers.Count == 0) dispatchOut();
        }

        private void pointersRemovedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            var over = pointers.Count;
            var p = pointerEventArgs.Pointers;
            var count = p.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = p[i];
                pointers.Remove(pointer.Id);
            }

            if (over > 0 && pointers.Count == 0) dispatchOut();
        }

        #endregion

    }
}
