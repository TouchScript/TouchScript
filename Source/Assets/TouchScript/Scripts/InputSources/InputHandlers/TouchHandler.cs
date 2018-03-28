/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Unity touch handling implementation which can be embedded and controlled from other (input) classes.
    /// </summary>
    public class TouchHandler : IInputHandler, IDisposable
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

        private IInputSource input;
        private PointerDelegate addPointer;
        private PointerDelegate updatePointer;
        private PointerDelegate pressPointer;
        private PointerDelegate releasePointer;
        private PointerDelegate removePointer;
        private PointerDelegate cancelPointer;

        private ObjectPool<TouchPointer> touchPool;
        // Unity fingerId -> TouchScript touch info
        private Dictionary<int, TouchState> systemToInternalId = new Dictionary<int, TouchState>(10);
        private int pointersNum;

#if UNITY_5_6_OR_NEWER
		private CustomSampler updateSampler;
#endif

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHandler" /> class.
        /// </summary>
        /// <param name="input">An input source to init new pointers with.</param>
        /// <param name="addPointer">A function called when a new pointer is detected.</param>
        /// <param name="updatePointer">A function called when a pointer is moved or its parameter is updated.</param>
        /// <param name="pressPointer">A function called when a pointer touches the surface.</param>
        /// <param name="releasePointer">A function called when a pointer is lifted off.</param>
        /// <param name="removePointer">A function called when a pointer is removed.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled.</param>
        public TouchHandler(IInputSource input, PointerDelegate addPointer, PointerDelegate updatePointer, PointerDelegate pressPointer, PointerDelegate releasePointer, PointerDelegate removePointer, PointerDelegate cancelPointer)
        {
            this.input = input;
            this.addPointer = addPointer;
            this.updatePointer = updatePointer;
            this.pressPointer = pressPointer;
            this.releasePointer = releasePointer;
            this.removePointer = removePointer;
            this.cancelPointer = cancelPointer;

            touchPool = new ObjectPool<TouchPointer>(10, newPointer, null, resetPointer, "TouchHandler/Touch");
            touchPool.Name = "Touch";

#if UNITY_5_6_OR_NEWER
			updateSampler = CustomSampler.Create("[TouchScript] Update Touch");
#endif
        }

        #region Public methods

        /// <inheritdoc />
        public bool UpdateInput()
        {
#if UNITY_5_6_OR_NEWER
			updateSampler.Begin();
#endif

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
                            systemToInternalId[t.fingerId] = new TouchState(internalAddPointer(t.position));
                        }
                        else
                        {
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position)));
                        }
                        break;
                    case TouchPhase.Moved:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            if (touchState.Phase != TouchPhase.Canceled)
                            {
                                touchState.Pointer.Position = remapCoordinates(t.position);
                                updatePointer(touchState.Pointer);
                            }
                        }
                        else
                        {
                            // Missed began phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position)));
                        }
                        break;
                    // NOTE: Unity touch on Windows reports Cancelled as Ended
                    // when a touch goes out of display boundary
                    case TouchPhase.Ended:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalRemovePointer(touchState.Pointer);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            var pointer = internalAddPointer(t.position);
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
                            var pointer = internalAddPointer(t.position);
                            internalCancelPointer(pointer);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState)) {}
                        else
                        {
                            // Missed begin phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalAddPointer(t.position)));
                        }
                        break;
                }
            }

#if UNITY_5_6_OR_NEWER
			updateSampler.End();
#endif

            return Input.touchCount > 0;
        }

        /// <inheritdoc />
        public void UpdateResolution(int width, int height) {}

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
        public bool DiscardPointer(Pointer pointer)
        {
            var p = pointer as TouchPointer;
            if (p == null) return false;

            touchPool.Release(p);
            return true;
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var touchState in systemToInternalId)
            {
                if (touchState.Value.Phase != TouchPhase.Canceled) internalCancelPointer(touchState.Value.Pointer);
            }
            systemToInternalId.Clear();
        }

        #endregion

        #region Private functions

        private Pointer internalAddPointer(Vector2 position)
        {
            pointersNum++;
            var pointer = touchPool.Get();
            pointer.Position = remapCoordinates(position);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            addPointer(pointer);
            pressPointer(pointer);
            return pointer;
        }

        private TouchPointer internalReturnPointer(TouchPointer pointer)
        {
            pointersNum++;
            var newPointer = touchPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            pressPointer(newPointer);
            return newPointer;
        }

        private void internalRemovePointer(Pointer pointer)
        {
            pointersNum--;
            pointer.Buttons &= ~Pointer.PointerButtonState.FirstButtonPressed;
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonUp;
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

        private void resetPointer(Pointer p)
        {
            p.INTERNAL_Reset();
        }

        private TouchPointer newPointer()
        {
            return new TouchPointer(input);
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