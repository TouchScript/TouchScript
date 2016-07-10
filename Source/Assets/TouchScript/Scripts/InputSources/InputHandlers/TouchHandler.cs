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

        /// <inheritdoc />
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

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

        private PointerDelegate addPointer;
        private PointerDelegate updatePointer;
        private PointerDelegate pressPointer;
        private PointerDelegate releasePointer;
        private PointerDelegate removePointer;
        private PointerDelegate cancelPointer;

        private ObjectPool<TouchPointer> touchPool;
        // Unity fingerId -> TouchScript touch info
        private Dictionary<int, TouchState> systemToInternalId = new Dictionary<int, TouchState>();
        private int pointersNum;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHandler" /> class.
        /// </summary>
        /// <param name="addPointer">A function called when a new pointer is detected.</param>
        /// <param name="updatePointer">A function called when a pointer is moved or its parameter is updated.</param>
        /// <param name="pressPointer">A function called when a pointer touches the surface.</param>
        /// <param name="releasePointer">A function called when a pointer is lifted off.</param>
        /// <param name="removePointer">A function called when a pointer is removed.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled.</param>
        public TouchHandler(PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer)
        {
            this.addPointer = addPointer;
            this.updatePointer = updatePointer;
            this.pressPointer = pressPointer;
            this.releasePointer = releasePointer;
            this.removePointer = removePointer;
            this.cancelPointer = cancelPointer;

            touchPool = new ObjectPool<TouchPointer>(10, () => new TouchPointer(this), null, (t) => t.INTERNAL_Reset());
            touchPool.Name = "Touch";
        }

        #region Public methods

        /// <inheritdoc />
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
                            internalRemovePointer(touchState.Pointer);
                            systemToInternalId[t.fingerId] = new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON));
                        }
                        else
                        {
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON)));
                        }
                        break;
                    case TouchPhase.Moved:
						if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
						{
						    if (touchState.Phase != TouchPhase.Canceled)
						    {
						        touchState.Pointer.Position = t.position;
                                updatePointer(touchState.Pointer);
						    }
						}
                        else
                        {
                            // Missed began phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON)));
                        }
                        break;
                    case TouchPhase.Ended:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalRemovePointer(touchState.Pointer);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            var pointer = internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON);
                            internalRemovePointer(pointer);
                        }
                        break;
                    case TouchPhase.Canceled:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalCancelPointer(touchState.Pointer);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            var pointer = internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON);
                            internalCancelPointer(pointer);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState)) {}
                        else
                        {
                            // Missed begin phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position, Pointer.FLAG_FIRST_BUTTON)));
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
                if (touchState.Value.Pointer == touch && touchState.Value.Phase != TouchPhase.Canceled)
                {
                    fingerId = touchState.Key;
                    break;
                }
            }
            if (fingerId > -1)
            {
                internalCancelPointer(touch);
                if (shouldReturn) systemToInternalId[fingerId] = new TouchState(internalReturnPointer(touch));
                else systemToInternalId[fingerId] = new TouchState(touch, TouchPhase.Canceled);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var touchState in systemToInternalId)
            {
                if (touchState.Value.Phase != TouchPhase.Canceled) internalCancelPointer(touchState.Value.Pointer);
            }
            systemToInternalId.Clear();
        }

        #endregion

        #region Internal methods

        /// <inheritdoc />
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
            pointer.Position = remapCoordinates(position);
            addPointer(pointer);
            pressPointer(pointer);
            pointer.Flags |= flags;
            return pointer;
        }

        private TouchPointer internalReturnPointer(TouchPointer pointer)
        {
            pointersNum++;
            var newPointer = touchPool.Get();
            newPointer.CopyFrom(pointer);
            addPointer(newPointer);
            pressPointer(newPointer);
            return newPointer;
        }

        private void internalRemovePointer(Pointer pointer)
        {
            pointersNum--;
            releasePointer(pointer);
            removePointer(pointer);
        }

        private void internalCancelPointer(Pointer pointer)
        {
            pointersNum--;
            cancelPointer(pointer);
        }

        private Vector2 remapCoordinates(Vector2 position)
        {
            if (CoordinatesRemapper != null) return CoordinatesRemapper.Remap(position);
            return position;
        }

        #endregion

        private struct TouchState
        {
            public Pointer Pointer;
            public TouchPhase Phase;

            public TouchState(Pointer pointer, TouchPhase phase = TouchPhase.Began)
            {
                Pointer = pointer;
                Phase = phase;
            }

        }

    }
}