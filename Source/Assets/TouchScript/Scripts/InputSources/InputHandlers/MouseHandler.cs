/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Unity mouse handling implementation which can be embedded and controlled from other (input) classes.
    /// </summary>
    public class MouseHandler : IInputSource, IDisposable
    {
        #region Public properties

        #endregion

        #region Private variables

        private Action<Pointer, Vector2, bool> beginPointer;
        private Action<int, Vector2> movePointer;
        private Action<int> endPointer;
        private Action<int> cancelPointer;

        private MousePointer mousePointer, fakeMousePointer;
        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseHandler" /> class.
        /// </summary>
        /// <param name="beginPointer">A function called when a new pointer is detected. As <see cref="InputSource.beginPointer" /> this function must accept a Vector2 position of the new pointer and return an instance of <see cref="Pointer" />.</param>
        /// <param name="movePointer">A function called when a pointer is moved. As <see cref="InputSource.movePointer" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="endPointer">A function called when a pointer is lifted off. As <see cref="InputSource.endPointer" /> this function must accept an int id.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled. As <see cref="InputSource.cancelPointer" /> this function must accept an int id.</param>
        public MouseHandler(Action<Pointer, Vector2, bool> beginPointer, Action<int, Vector2> movePointer, Action<int> endPointer, Action<int> cancelPointer)
        {
            this.beginPointer = beginPointer;
            this.movePointer = movePointer;
            this.endPointer = endPointer;
            this.cancelPointer = cancelPointer;

            mousePointer = new MousePointer(this);
            fakeMousePointer = new MousePointer(this);
        }

        #region Public methods

        /// <summary>
        /// Immediately ends all pointers.
        /// </summary>
        public void EndPointers()
        {
            if (mousePointer.Id != Pointer.INVALID_POINTER)
            {
                endPointer(mousePointer.Id);
            }
            if (fakeMousePointer.Id != Pointer.INVALID_POINTER)
            {
                endPointer(fakeMousePointer.Id);
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void UpdateInput()
        {
            // If mouse button was pressed and released during the same frame,
            // we need to figure out what happened first.
            var upHandled = false;
            if (Input.GetMouseButtonUp(0))
            {
                // Release happened first?
                if (mousePointer.Id != Pointer.INVALID_POINTER)
                {
                    endPointer(mousePointer.Id);
                    upHandled = true;
                }
            }

            // Need to end fake pointer
            if (fakeMousePointer.Id != Pointer.INVALID_POINTER && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endPointer(fakeMousePointer.Id);
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointer.Id == Pointer.INVALID_POINTER)
                    beginPointer(fakeMousePointer, new Vector2(pos.x, pos.y), true);
                else if (mousePointer.Id == Pointer.INVALID_POINTER) beginPointer(mousePointer, new Vector2(pos.x, pos.y), true);
            }
            else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointer.Id != Pointer.INVALID_POINTER)
                    {
                        if (mousePointer.Id == Pointer.INVALID_POINTER) movePointer(fakeMousePointer.Id, new Vector2(pos.x, pos.y));
                        else movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
                    }
                    else if (mousePointer.Id != Pointer.INVALID_POINTER) movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
                }
            }

            // Release mouse if we haven't done it yet
            if (Input.GetMouseButtonUp(0) && !upHandled && mousePointer.Id != Pointer.INVALID_POINTER)
            {
                endPointer(mousePointer.Id);
            }
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool @return)
        {
            if (pointer.Equals(mousePointer))
            {
                cancelPointer(mousePointer.Id);
                if (@return) beginPointer(mousePointer, pointer.Position, false);
                return true;
            }
            else if (pointer.Equals(fakeMousePointer))
            {
                cancelPointer(fakeMousePointer.Id);
                if (@return) beginPointer(fakeMousePointer, pointer.Position, false);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (mousePointer.Id != Pointer.INVALID_POINTER) cancelPointer(mousePointer.Id);
            if (fakeMousePointer.Id != Pointer.INVALID_POINTER) cancelPointer(fakeMousePointer.Id);
        }

        #endregion

        #region Internal methods

        public void INTERNAL_ReleasePointer(Pointer pointer)
        {
            if (pointer.Equals(mousePointer))
            {
                mousePointer.INTERNAL_Reset();
            } else if (pointer.Equals(fakeMousePointer))
            {
                fakeMousePointer.INTERNAL_Reset();
            }
        }

        #endregion

        #region Private functions

        #endregion
    }
}