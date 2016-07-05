/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Unity touch handling implementation which can be embedded and controlled from other (input) classes.
    /// </summary>
    public class TouchHandler : IInputSource, IDisposable
    {
        #region Public properties

        /// <summary>
        /// Gets a value indicating whether there any active pointers.
        /// </summary>
        /// <value> <c>true</c> if this instance has active pointers; otherwise, <c>false</c>. </value>
        public bool HasPointers
        {
            get { return pointersNum > 0; }
        }

        #endregion

        #region Private variables

        private AddPointerDelegate addPointer;
        private MovePointerDelegate movePointer;
        private PressPointerDelegate pressPointer;
        private ReleasePointerDelegate releasePointer;
        private RemovePointerDelegate removePointer;
        private CancelPointerDelegate cancelPointer;

        private ObjectPool<TouchPointer> touchPool;
        private Dictionary<int, TouchState> systemToInternalId = new Dictionary<int, TouchState>();
        private int pointersNum;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHandler" /> class.
        /// </summary>
        /// <param name="beginPointer">A function called when a new pointer is detected. As <see cref="InputSource.pressPointer" /> this function must accept a Vector2 position of the new pointer and return an instance of <see cref="Pointer" />.</param>
        /// <param name="movePointer">A function called when a pointer is moved. As <see cref="InputSource.movePointer" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="endPointer">A function called when a pointer is lifted off. As <see cref="InputSource.releasePointer" /> this function must accept an int id.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled. As <see cref="InputSource.cancelPointer" /> this function must accept an int id.</param>
        public TouchHandler(AddPointerDelegate addPointer, MovePointerDelegate movePointer, PressPointerDelegate pressPointer, ReleasePointerDelegate releasePointer, RemovePointerDelegate removePointer, CancelPointerDelegate cancelPointer)
        {
            this.addPointer = addPointer;
            this.movePointer = movePointer;
            this.pressPointer = pressPointer;
            this.releasePointer = releasePointer;
            this.removePointer = removePointer;
            this.cancelPointer = cancelPointer;

            touchPool = new ObjectPool<TouchPointer>(10, () => new TouchPointer(this), null, (t) => t.INTERNAL_Reset());
            touchPool.Name = "Touch";
        }

        #region Public methods

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void UpdateInput()
        {
            for (var i = 0; i < Input.touchCount; ++i)
            {
                var t = Input.GetTouch(i);

                TouchState touchState;
                switch (t.phase)
                {
                    case TouchPhase.Began:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState) && touchState.Phase != TouchPhase.Canceled)
                        {
                            // Ending previous touch (missed a frame)
                            internalRemovePointer(touchState.Id);
                            systemToInternalId[t.fingerId] = new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON).Id);
                        }
                        else
                        {
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON).Id));
                        }
                        break;
                    case TouchPhase.Moved:
						if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
						{
							if (touchState.Phase != TouchPhase.Canceled) movePointer(touchState.Id, t.position);
						}
                        else
                        {
                            // Missed began phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON).Id));
                        }
                        break;
                    case TouchPhase.Ended:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalRemovePointer(touchState.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            internalRemovePointer(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON).Id);
                        }
                        break;
                    case TouchPhase.Canceled:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalCancelPointer(touchState.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            internalCancelPointer(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON).Id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState)) {}
                        else
                        {
                            // Missed begin phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON).Id));
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            var touch = pointer as TouchPointer;
            if (touch == null) return false;

            int fingerId = -1;
            foreach (var touchState in systemToInternalId)
            {
                if (touchState.Value.Id == touch.Id && touchState.Value.Phase != TouchPhase.Canceled)
                {
                    fingerId = touchState.Key;
                    break;
                }
            }
            if (fingerId > -1)
            {
                internalCancelPointer(touch.Id);
                if (shouldReturn) systemToInternalId[fingerId] = new TouchState(internalReturnPointer(touch, touch.Position).Id);
                else systemToInternalId[fingerId] = new TouchState(touch.Id, TouchPhase.Canceled);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var touchState in systemToInternalId)
            {
                if (touchState.Value.Phase != TouchPhase.Canceled) internalCancelPointer(touchState.Value.Id);
            }
            systemToInternalId.Clear();
        }

        #endregion

        #region Internal methods

        public void INTERNAL_DiscardPointer(Pointer pointer)
        {
            var p = pointer as TouchPointer;
            if (p == null) return;

            touchPool.Release(p);
        }

        #endregion

        #region Private functions

        private Pointer internalAddPointer(Vector2 position, uint flags = 0)
        {
            pointersNum++;
            var pointer = touchPool.Get();
            addPointer(pointer, position, true);
            pressPointer(pointer.Id);
            pointer.Flags |= flags;
            return pointer;
        }

        private TouchPointer internalReturnPointer(TouchPointer pointer, Vector2 position)
        {
            pointersNum++;
            var newPointer = touchPool.Get();
            newPointer.CopyFrom(pointer);
            addPointer(newPointer, position, false);
            pressPointer(newPointer.Id);
            return newPointer;
        }

        private void internalRemovePointer(int id)
        {
            pointersNum--;
            releasePointer(id);
            removePointer(id);
        }

        private void internalCancelPointer(int id)
        {
            pointersNum--;
            cancelPointer(id);
        }

        #endregion

        private struct TouchState
        {
            public int Id;
            public TouchPhase Phase;

            public TouchState(int id, TouchPhase phase = TouchPhase.Began)
            {
                Id = id;
                Phase = phase;
            }

        }

    }
}