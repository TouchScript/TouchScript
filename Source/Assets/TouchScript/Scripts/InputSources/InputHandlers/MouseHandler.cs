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
        /// Immediately ends all pointers.
        /// </summary>
        public void EndPointers()
        {
            if (mousePointer != null)
            {
                releasePointer(mousePointer.Id);
                mousePointer = null;
            }
            if (fakeMousePointer != null)
            {
                releasePointer(fakeMousePointer.Id);
                fakeMousePointer = null;
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void UpdateInput()
        {
            var pos = Input.mousePosition;
            if (mousePointPos != pos)
            {
                mousePointPos = pos;
                movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
            }

            //// If mouse button was pressed and released during the same frame,
            //// we need to figure out what happened first.
            //var upHandled = false;
            //if (Input.GetMouseButtonUp(0))
            //{
            //    // Release happened first?
            //    if (mousePointer != null)
            //    {
            //        releasePointer(mousePointer.Id);
            //        mousePointer = null;
            //        upHandled = true;
            //    }
            //}

            //// Need to end fake pointer
            //if (emulateSecondMousePointer
            //    && fakeMousePointer != null
            //    && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            //{
            //    releasePointer(fakeMousePointer.Id);
            //    fakeMousePointer = null;
            //}

            //if (Input.GetMouseButtonDown(0))
            //{
            //    var pos = Input.mousePosition;
            //    if (emulateSecondMousePointer
            //        && fakeMousePointer == null
            //        && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            //    {
            //        fakeMousePointer = internalAddPointer(new Vector2(pos.x, pos.y), Pointer.FLAG_FIRST_BUTTON | Pointer.FLAG_ARTIFICIAL);
            //    }
            //    else if (mousePointer == null)
            //    {
            //        mousePointer = internalAddPointer(new Vector2(pos.x, pos.y), Pointer.FLAG_FIRST_BUTTON);
            //    }
            //}
            //else if (Input.GetMouseButton(0))
            //{
            //    var pos = Input.mousePosition;
            //    if (mousePointPos != pos)
            //    {
            //        mousePointPos = pos;
            //        if (emulateSecondMousePointer
            //            && fakeMousePointer != null)
            //        {
            //            if (mousePointer == null) movePointer(fakeMousePointer.Id, new Vector2(pos.x, pos.y));
            //            else movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
            //        }
            //        else if (mousePointer != null) movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
            //    }
            //}

            //// Release mouse if we haven't done it yet
            //if (Input.GetMouseButtonUp(0) && !upHandled && mousePointer != null)
            //{
            //    releasePointer(mousePointer.Id);
            //    mousePointer = null;
            //}
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool @return)
        {
            if (pointer.Equals(mousePointer))
            {
                cancelPointer(mousePointer.Id);
                if (@return) mousePointer = internalReturnPointer(mousePointer, pointer.Position);
                else mousePointer = null;
                return true;
            }
            if (pointer.Equals(fakeMousePointer))
            {
                cancelPointer(fakeMousePointer.Id);
                if (@return) fakeMousePointer = internalReturnPointer(fakeMousePointer, pointer.Position);
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
            newPointer.CopyFrom(pointer);
            //pressPointer(newPointer, position, false);
            return newPointer;
        }

        #endregion
    }
}