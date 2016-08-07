/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Unity mouse handling implementation which can be embedded and controlled from other (input) classes.
    /// </summary>
    public class MouseHandler : IInputSource, IDisposable
    {
        #region Public properties

        /// <inheritdoc />
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether second pointer emulation using ALT+CLICK should be enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if second pointer emulation is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EmulateSecondMousePointer
        {
            get { return emulateSecondMousePointer; }
            set
            {
                emulateSecondMousePointer = value;
                if (fakeMousePointer != null) CancelPointer(fakeMousePointer, false);
            }
        }

        #endregion

        #region Private variables

        private bool emulateSecondMousePointer = true;

        private PointerDelegate addPointer;
        private PointerDelegate updatePointer;
        private PointerDelegate pressPointer;
        private PointerDelegate releasePointer;
        private PointerDelegate removePointer;
        private PointerDelegate cancelPointer;

        private ObjectPool<MousePointer> mousePool;
        private MousePointer mousePointer, fakeMousePointer;
        private Vector3 mousePointPos = Vector3.zero;
        private DelayedFakePointer addFakePointer;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseHandler" /> class.
        /// </summary>
        /// <param name="addPointer">A function called when a new pointer is detected.</param>
        /// <param name="updatePointer">A function called when a pointer is moved or its parameter is updated.</param>
        /// <param name="pressPointer">A function called when a pointer touches the surface.</param>
        /// <param name="releasePointer">A function called when a pointer is lifted off.</param>
        /// <param name="removePointer">A function called when a pointer is removed.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled.</param>
        public MouseHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer)
        {
            this.addPointer = addPointer;
            this.updatePointer = updatePointer;
            this.pressPointer = pressPointer;
            this.releasePointer = releasePointer;
            this.removePointer = removePointer;
            this.cancelPointer = cancelPointer;

            mousePool = new ObjectPool<MousePointer>(4, () => new MousePointer(this), null, (t) => t.INTERNAL_Reset());

            addFakePointer.ShouldAdd = false;
            mousePointPos = Input.mousePosition;
            mousePointer = internalAddPointer(mousePointPos);
        }

        #region Public methods

        /// <inheritdoc />
        public void UpdateInput()
        {
            if (addFakePointer.ShouldAdd)
            {
                addFakePointer.ShouldAdd = false;
                fakeMousePointer = internalAddPointer(addFakePointer.Position, addFakePointer.Buttons, addFakePointer.Flags);
                pressPointer(fakeMousePointer);
            }

            if (fakeMousePointer != null
                && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                releasePointer(fakeMousePointer);
                removePointer(fakeMousePointer);
                fakeMousePointer = null;
            }

            var pos = Input.mousePosition;
            if (mousePointPos != pos)
            {
                mousePointPos = pos;
                mousePointer.Position = remapCoordinates(new Vector2(pos.x, pos.y));
                updatePointer(mousePointer);
            }

            var buttons = mousePointer.Buttons;
            var newButtons = getMouseButtons();

            if (buttons == newButtons) return; // nothing new happened

            // pressed something
            if (buttons == Pointer.PointerButtonState.Nothing)
            {
                // pressed and released this frame
                if ((newButtons & Pointer.PointerButtonState.AnyButtonPressed) == 0)
                {
                    // Add pressed buttons for processing
                    mousePointer.Buttons = newButtons | (Pointer.PointerButtonState) ((uint) (newButtons & Pointer.PointerButtonState.AnyButtonDown) >> 1);
                    pressPointer(mousePointer);
                    releasePointer(mousePointer);
                    tryAddFakePointer(newButtons);
                }
                // pressed this frame
                else
                {
                    mousePointer.Buttons = newButtons;
                    pressPointer(mousePointer);
                }
            }
            // released or button state changed
            else
            {
                // released this frame
                if ((newButtons & Pointer.PointerButtonState.AnyButtonPressed) == 0)
                {
                    mousePointer.Buttons = newButtons;
                    releasePointer(mousePointer);
                    tryAddFakePointer(newButtons);
                }
                // button state changed this frame
                else
                {
                    mousePointer.Buttons = newButtons;
                    updatePointer(mousePointer);
                }
            }
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            if (pointer.Equals(mousePointer))
            {
                cancelPointer(mousePointer);
                if (shouldReturn) mousePointer = internalReturnPointer(mousePointer);
                else mousePointer = internalAddPointer(mousePointPos); // can't totally cancell mouse pointer
                return true;
            }
            if (pointer.Equals(fakeMousePointer))
            {
                cancelPointer(fakeMousePointer);
                if (shouldReturn) fakeMousePointer = internalReturnPointer(fakeMousePointer);
                else fakeMousePointer = null;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (mousePointer != null)
            {
                cancelPointer(mousePointer);
                mousePointer = null;
            }
            if (fakeMousePointer != null)
            {
                cancelPointer(fakeMousePointer);
                fakeMousePointer = null;
            }
        }

        #endregion

        #region Internal methods

        /// <inheritdoc />
        public void INTERNAL_DiscardPointer(Pointer pointer)
        {
            var p = pointer as MousePointer;
            if (p == null) return;

            mousePool.Release(p);
        }

        #endregion

        #region Private functions

        private Pointer.PointerButtonState getMouseButtons()
        {
            Pointer.PointerButtonState buttons = Pointer.PointerButtonState.Nothing;

            if (Input.GetMouseButton(0)) buttons |= Pointer.PointerButtonState.FirstButtonPressed;
            if (Input.GetMouseButtonDown(0)) buttons |= Pointer.PointerButtonState.FirstButtonDown;
            if (Input.GetMouseButtonUp(0)) buttons |= Pointer.PointerButtonState.FirstButtonUp;

            if (Input.GetMouseButton(1)) buttons |= Pointer.PointerButtonState.SecondButtonPressed;
            if (Input.GetMouseButtonDown(1)) buttons |= Pointer.PointerButtonState.SecondButtonDown;
            if (Input.GetMouseButtonUp(1)) buttons |= Pointer.PointerButtonState.SecondButtonUp;

            if (Input.GetMouseButton(2)) buttons |= Pointer.PointerButtonState.ThirdButtonPressed;
            if (Input.GetMouseButtonDown(2)) buttons |= Pointer.PointerButtonState.ThirdButtonDown;
            if (Input.GetMouseButtonUp(2)) buttons |= Pointer.PointerButtonState.ThirdButtonUp;

            return buttons;
        }

        private void tryAddFakePointer(Pointer.PointerButtonState newButtons)
        {
            if (emulateSecondMousePointer
                        && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                        && fakeMousePointer == null)
            {
                var up = (uint)(newButtons & Pointer.PointerButtonState.AnyButtonUp);
                addFakePointer.ShouldAdd = true;
                addFakePointer.Flags = mousePointer.Flags | Pointer.FLAG_ARTIFICIAL;
                addFakePointer.Buttons = newButtons
                    & ~Pointer.PointerButtonState.AnyButtonUp // remove up state from fake pointer
                    | (Pointer.PointerButtonState)(up >> 1) // Add down state from pressed buttons
                    | (Pointer.PointerButtonState)(up >> 2); // Add pressed state from pressed buttons
                addFakePointer.Position = mousePointPos;
            }
        }

        private MousePointer internalAddPointer(Vector2 position, Pointer.PointerButtonState buttons = Pointer.PointerButtonState.Nothing, uint flags = 0)
        {
            var pointer = mousePool.Get();
            pointer.Position = remapCoordinates(position);
            pointer.Buttons |= buttons;
			pointer.Flags |= flags;
            addPointer(pointer);
            return pointer;
        }

        private MousePointer internalReturnPointer(MousePointer pointer)
        {
            var newPointer = mousePool.Get();
            newPointer.CopyFrom(pointer);
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            if ((newPointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) != 0)
            {
                // Adding down state this frame
                newPointer.Buttons |= (Pointer.PointerButtonState) ((uint) (newPointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) << 1);
                pressPointer(newPointer);
            }
            return newPointer;
        }

        private Vector2 remapCoordinates(Vector2 position)
        {
            if (CoordinatesRemapper != null) return CoordinatesRemapper.Remap(position);
            return position;
        }

        #endregion

        private struct DelayedFakePointer
        {
            public bool ShouldAdd;
            public uint Flags;
            public Pointer.PointerButtonState Buttons;
            public Vector2 Position;
        }

    }
}