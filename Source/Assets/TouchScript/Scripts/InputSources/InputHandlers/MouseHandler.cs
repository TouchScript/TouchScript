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
    public class MouseHandler : IDisposable
    {
        #region Private variables

        private Func<Vector2, Tags, bool, Pointer> beginPointer;
        private Action<int, Vector2> movePointer;
        private Action<int> endPointer;
        private Action<int> cancelPointer;

        private Tags tags;
        private int mousePointId = -1;
        private int fakeMousePointId = -1;
        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseHandler" /> class.
        /// </summary>
        /// <param name="tags">Tags to add to pointers.</param>
        /// <param name="beginPointer">A function called when a new pointer is detected. As <see cref="InputSource.beginPointer" /> this function must accept a Vector2 position of the new pointer and return an instance of <see cref="Pointer" />.</param>
        /// <param name="movePointer">A function called when a pointer is moved. As <see cref="InputSource.movePointer" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="endPointer">A function called when a pointer is lifted off. As <see cref="InputSource.endPointer" /> this function must accept an int id.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled. As <see cref="InputSource.cancelPointer" /> this function must accept an int id.</param>
        public MouseHandler(Tags tags, Func<Vector2, Tags, bool, Pointer> beginPointer, Action<int, Vector2> movePointer, Action<int> endPointer, Action<int> cancelPointer)
        {
            this.tags = tags;
            this.beginPointer = beginPointer;
            this.movePointer = movePointer;
            this.endPointer = endPointer;
            this.cancelPointer = cancelPointer;

            mousePointId = -1;
            fakeMousePointId = -1;
        }

        #region Public methods

        /// <summary>
        /// Immediately ends all pointers.
        /// </summary>
        public void EndPointers()
        {
            if (mousePointId != -1)
            {
                endPointer(mousePointId);
                mousePointId = -1;
            }
            if (fakeMousePointId != -1)
            {
                endPointer(fakeMousePointId);
                fakeMousePointId = -1;
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            // If mouse button was pressed and released during the same frame,
            // we need to figure out what happened first.
            var upHandled = false;
            if (Input.GetMouseButtonUp(0))
            {
                // Release happened first?
                if (mousePointId != -1)
                {
                    endPointer(mousePointId);
                    mousePointId = -1;
                    upHandled = true;
                }
            }

            // Need to end fake pointer
            if (fakeMousePointId > -1 && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endPointer(fakeMousePointId);
                fakeMousePointId = -1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointId == -1)
                    fakeMousePointId = beginPointer(new Vector2(pos.x, pos.y), tags, true).Id;
                else if (mousePointId == -1) mousePointId = beginPointer(new Vector2(pos.x, pos.y), tags, true).Id;
            }
            else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointId != -1)
                    {
                        if (mousePointId == -1) movePointer(fakeMousePointId, new Vector2(pos.x, pos.y));
                        else movePointer(mousePointId, new Vector2(pos.x, pos.y));
                    }
                    else if (mousePointId != -1) movePointer(mousePointId, new Vector2(pos.x, pos.y));
                }
            }

            // Release mouse if we haven't done it yet
            if (Input.GetMouseButtonUp(0) && !upHandled && mousePointId != -1)
            {
                endPointer(mousePointId);
                mousePointId = -1;
            }
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool @return)
        {
            if (pointer.Id == mousePointId)
            {
                cancelPointer(mousePointId);
                if (@return) mousePointId = beginPointer(pointer.Position, tags, false).Id;
                else mousePointId = -1;
                return true;
            }
            if (pointer.Id == fakeMousePointId)
            {
                cancelPointer(fakeMousePointId);
                if (@return) fakeMousePointId = beginPointer(pointer.Position, tags, false).Id;
                else fakeMousePointId = -1;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (mousePointId != -1) cancelPointer(mousePointId);
            if (fakeMousePointId != -1) cancelPointer(fakeMousePointId);
        }

        #endregion
    }
}