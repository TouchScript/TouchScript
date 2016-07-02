/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Unity touch handling implementation which can be embedded and controlled from other (input) classes.
    /// </summary>
    public class TouchHandler : IDisposable
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

        private Func<Vector2, Tags, bool, Pointer> beginPointer;
        private Action<int, Vector2> movePointer;
        private Action<int> endPointer;
        private Action<int> cancelPointer;

        private Tags tags;
        private Dictionary<int, TouchState> systemToInternalId = new Dictionary<int, TouchState>();
        private int pointersNum;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHandler" /> class.
        /// </summary>
        /// <param name="tags">Tags to add to pointers.</param>
        /// <param name="beginPointer">A function called when a new pointer is detected. As <see cref="InputSource.beginPointer" /> this function must accept a Vector2 position of the new pointer and return an instance of <see cref="Pointer" />.</param>
        /// <param name="movePointer">A function called when a pointer is moved. As <see cref="InputSource.movePointer" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="endPointer">A function called when a pointer is lifted off. As <see cref="InputSource.endPointer" /> this function must accept an int id.</param>
        /// <param name="cancelPointer">A function called when a pointer is cancelled. As <see cref="InputSource.cancelPointer" /> this function must accept an int id.</param>
        public TouchHandler(Tags tags, Func<Vector2, Tags, bool, Pointer> beginPointer, Action<int, Vector2> movePointer, Action<int> endPointer, Action<int> cancelPointer)
        {
            this.tags = tags;
            this.beginPointer = beginPointer;
            this.movePointer = movePointer;
            this.endPointer = endPointer;
            this.cancelPointer = cancelPointer;
        }

        #region Public methods

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
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
                            internalEndPointer(touchState.Id);
                            systemToInternalId[t.fingerId] = new TouchState(internalBeginPointer(t.position).Id);
                        }
                        else
                        {
                            systemToInternalId.Add(t.fingerId, new TouchState(internalBeginPointer(t.position).Id));
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
                            systemToInternalId.Add(t.fingerId, new TouchState(internalBeginPointer(t.position).Id));
                        }
                        break;
                    case TouchPhase.Ended:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalEndPointer(touchState.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            internalEndPointer(internalBeginPointer(t.position).Id);
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
                            internalCancelPointer(internalBeginPointer(t.position).Id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState)) {}
                        else
                        {
                            // Missed begin phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalBeginPointer(t.position).Id));
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        public bool CancelPointer(Pointer pointer, bool @return)
        {
            int fingerId = -1;
            foreach (var touchState in systemToInternalId)
            {
                if (touchState.Value.Id == pointer.Id && touchState.Value.Phase != TouchPhase.Canceled)
                {
                    fingerId = touchState.Key;
                    break;
                }
            }
            if (fingerId > -1)
            {
                if (@return)
                {
                    cancelPointer(pointer.Id);
                    systemToInternalId[fingerId] = new TouchState(beginPointer(pointer.Position, pointer.Tags, false).Id);
                }
                else
                {
                    systemToInternalId[fingerId] = new TouchState(pointer.Id, TouchPhase.Canceled);
                    internalCancelPointer(pointer.Id);
                }
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

        #region Private functions

        private Pointer internalBeginPointer(Vector2 position)
        {
            pointersNum++;
            return beginPointer(position, tags, true);
        }

        private void internalEndPointer(int id)
        {
            pointersNum--;
            endPointer(id);
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