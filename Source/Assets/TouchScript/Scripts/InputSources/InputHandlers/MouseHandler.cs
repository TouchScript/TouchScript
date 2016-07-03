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

        #endregion

        #region Private variables

        private Action<Pointer, Vector2, bool> beginPointer;
        private Action<int, Vector2> movePointer;
        private Action<int> endPointer;
        private Action<int> cancelPointer;

        private ObjectPool<MousePointer> mousePool;
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

            mousePool = new ObjectPool<MousePointer>(4, () => new MousePointer(this), null, (t) => t.INTERNAL_Reset());
        }

        #region Public methods

        /// <summary>
        /// Immediately ends all pointers.
        /// </summary>
        public void EndPointers()
        {
            if (mousePointer != null)
            {
                endPointer(mousePointer.Id);
                mousePointer = null;
            }
            if (fakeMousePointer != null)
            {
                endPointer(fakeMousePointer.Id);
                fakeMousePointer = null;
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
                if (mousePointer != null)
                {
                    endPointer(mousePointer.Id);
                    mousePointer = null;
                    upHandled = true;
                }
            }

            // Need to end fake pointer
            if (fakeMousePointer != null && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endPointer(fakeMousePointer.Id);
                fakeMousePointer = null;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointer == null)
                    fakeMousePointer = internalBeginPointer(new Vector2(pos.x, pos.y), Pointer.FLAG_FIRST_BUTTON | Pointer.FLAG_ARTIFICIAL);
                else if (mousePointer == null)
                    mousePointer = internalBeginPointer(new Vector2(pos.x, pos.y), Pointer.FLAG_FIRST_BUTTON);
            }
            else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointer != null)
                    {
                        if (mousePointer == null) movePointer(fakeMousePointer.Id, new Vector2(pos.x, pos.y));
                        else movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
                    }
                    else if (mousePointer != null) movePointer(mousePointer.Id, new Vector2(pos.x, pos.y));
                }
            }

            // Release mouse if we haven't done it yet
            if (Input.GetMouseButtonUp(0) && !upHandled && mousePointer != null)
            {
                endPointer(mousePointer.Id);
                mousePointer = null;
            }
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

        public void INTERNAL_ReleasePointer(Pointer pointer)
        {
            var p = pointer as MousePointer;
            if (p == null) return;

            mousePool.Release(p);
        }

        #endregion

        #region Private functions

        private MousePointer internalBeginPointer(Vector2 position, uint flags)
        {
            var pointer = mousePool.Get();
            beginPointer(pointer, position, true);
            pointer.Flags |= flags;
            return pointer;
        }

        private MousePointer internalReturnPointer(MousePointer pointer, Vector2 position)
        {
            var newPointer = mousePool.Get();
            newPointer.CopyFrom(pointer);
            beginPointer(newPointer, position, false);
            return newPointer;
        }

        #endregion
    }
}