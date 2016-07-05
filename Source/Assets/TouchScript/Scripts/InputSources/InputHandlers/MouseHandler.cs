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

        private AddPointerDelegate addPointer;
        private MovePointerDelegate movePointer;
        private PressPointerDelegate pressPointer;
        private ReleasePointerDelegate releasePointer;
        private RemovePointerDelegate removePointer;
        private CancelPointerDelegate cancelPointer;

        private ObjectPool<MousePointer> mousePool;
        private MousePointer mousePointer, fakeMousePointer;
        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseHandler" /> class.
        /// </summary>
        /// <param name="pressPointer">A function called when a new pointer is detected. As <see cref="InputSource.pressPointer" /> this function must accept a Vector2 position of the new pointer and return an instance of <see cref="Pointer" />.</param>
        /// <param name="movePointer">A function called when a pointer is moved. As <see cref="InputSource.movePointer" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="releasePointer">A function called when a pointer is lifted off. As <see cref="InputSource.releasePointer" /> this function must accept an int id.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled. As <see cref="InputSource.cancelPointer" /> this function must accept an int id.</param>
        public MouseHandler(AddPointerDelegate addPointer, MovePointerDelegate movePointer, PressPointerDelegate pressPointer, ReleasePointerDelegate releasePointer, RemovePointerDelegate removePointer, CancelPointerDelegate cancelPointer)
        {
            this.addPointer = addPointer;
            this.movePointer = movePointer;
            this.pressPointer = pressPointer;
            this.releasePointer = releasePointer;
            this.removePointer = removePointer;
            this.cancelPointer = cancelPointer;

            mousePool = new ObjectPool<MousePointer>(4, () => new MousePointer(this), null, (t) => t.INTERNAL_Reset());

            mousePointPos = Input.mousePosition;
            mousePointer = internalAddPointer(mousePointPos);
        }

        #region Public methods

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void UpdateInput()
        {
            if (fakeMousePointer != null
                && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                fakeMousePointer.Flags = fakeMousePointer.Flags & ~Pointer.FLAG_INCONTACT;
                releasePointer(fakeMousePointer.Id);
                removePointer(fakeMousePointer.Id);
                fakeMousePointer = null;
            }

            var pos = Input.mousePosition;
            if (mousePointPos != pos)
            {
                mousePointPos = pos;
                movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
            }

            var flags = mousePointer.Flags;
            var buttonFlags = flags & Pointer.FLAG_INCONTACT;
            if (buttonFlags == 0)
            {
                // Hovering point
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    // But there was a click this frame
                    var newFlags = getMouseButtonFlags();
                    mousePointer.Flags = (flags & ~Pointer.FLAG_INCONTACT) | newFlags;
                    pressPointer(mousePointer.Id);
                    if (newFlags == 0)
                    {
                        // And release the same frame
                        releasePointer(mousePointer.Id);
                        if (emulateSecondMousePointer
                        && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                        && fakeMousePointer == null)
                        {
                            fakeMousePointer = internalAddPointer(mousePointPos, flags | Pointer.FLAG_ARTIFICIAL);
                            pressPointer(fakeMousePointer.Id);
                        }
                    }
                }
            }
            else
            {
                var newFlags = getMouseButtonFlags();
                mousePointer.Flags = (flags & ~Pointer.FLAG_INCONTACT) | newFlags;
                if (newFlags == 0)
                {
                    // Released this frame
                    releasePointer(mousePointer.Id);
                    if (emulateSecondMousePointer 
                        && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                        && fakeMousePointer == null)
                    {
                        fakeMousePointer = internalAddPointer(mousePointPos, flags | Pointer.FLAG_ARTIFICIAL);
                        pressPointer(fakeMousePointer.Id);
                    }
                }
            }
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            if (pointer.Equals(mousePointer))
            {
                cancelPointer(mousePointer.Id);
                if (shouldReturn) mousePointer = internalReturnPointer(mousePointer, pointer.Position);
                else mousePointer = internalAddPointer(mousePointPos); // can't totally cancell mouse pointer
                return true;
            }
            if (pointer.Equals(fakeMousePointer))
            {
                cancelPointer(fakeMousePointer.Id);
                if (shouldReturn) fakeMousePointer = internalReturnPointer(fakeMousePointer, pointer.Position);
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
                cancelPointer(mousePointer.Id);
                mousePointer = null;
            }
            if (fakeMousePointer != null)
            {
                cancelPointer(fakeMousePointer.Id);
                fakeMousePointer = null;
            }
        }

        #endregion

        #region Internal methods

        public void INTERNAL_DiscardPointer(Pointer pointer)
        {
            var p = pointer as MousePointer;
            if (p == null) return;

            mousePool.Release(p);
        }

        #endregion

        #region Private functions

        private uint getMouseButtonFlags()
        {
            uint pressedButtons = 0;
            if (Input.GetMouseButton(0)) pressedButtons |= Pointer.FLAG_FIRST_BUTTON;
            if (Input.GetMouseButton(1)) pressedButtons |= Pointer.FLAG_SECOND_BUTTON;
            if (Input.GetMouseButton(2)) pressedButtons |= Pointer.FLAG_THIRD_BUTTON;
            return pressedButtons;
        }

        private MousePointer internalAddPointer(Vector2 position, uint flags = 0)
        {
            var pointer = mousePool.Get();
            addPointer(pointer, position);
            pointer.Flags |= flags;
            return pointer;
        }

        private MousePointer internalReturnPointer(MousePointer pointer, Vector2 position)
        {
            var newPointer = mousePool.Get();
//            newPointer.CopyFrom(pointer);
			newPointer.Flags = pointer.Flags;
            addPointer(newPointer, position, false);
            if ((newPointer.Flags & Pointer.FLAG_INCONTACT) != 0) pressPointer(newPointer.Id);
            return newPointer;
        }

        #endregion
    }
}