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
        /// Gets a value indicating whether there any active touches.
        /// </summary>
        /// <value> <c>true</c> if this instance has active touches; otherwise, <c>false</c>. </value>
        public bool HasTouches
        {
            get { return touchesNum > 0; }
        }

        #endregion

        #region Private variables

        private Func<Vector2, Tags, bool, TouchPoint> beginTouch;
        private Action<int, Vector2> moveTouch;
        private Action<int> endTouch;
        private Action<int> cancelTouch;

        private Tags tags;
        private Dictionary<int, TouchState> systemToInternalId = new Dictionary<int, TouchState>();
        private int touchesNum;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHandler" /> class.
        /// </summary>
        /// <param name="tags">Tags to add to touches.</param>
        /// <param name="beginTouch">A function called when a new touch is detected. As <see cref="InputSource.beginTouch" /> this function must accept a Vector2 position of the new touch and return an instance of <see cref="TouchPoint" />.</param>
        /// <param name="moveTouch">A function called when a touch is moved. As <see cref="InputSource.moveTouch" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="endTouch">A function called when a touch is lifted off. As <see cref="InputSource.endTouch" /> this function must accept an int id.</param>
        /// <param name="cancelTouch">A function called when a touch is cancelled. As <see cref="InputSource.cancelTouch" /> this function must accept an int id.</param>
        public TouchHandler(Tags tags, Func<Vector2, Tags, bool, TouchPoint> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch, Action<int> cancelTouch)
        {
            this.tags = tags;
            this.beginTouch = beginTouch;
            this.moveTouch = moveTouch;
            this.endTouch = endTouch;
            this.cancelTouch = cancelTouch;
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
                            internalEndTouch(touchState.Id);
                            systemToInternalId[t.fingerId] = new TouchState(internalBeginTouch(t.position).Id);
                        }
                        else
                        {
                            systemToInternalId.Add(t.fingerId, new TouchState(internalBeginTouch(t.position).Id));
                        }
                        break;
                    case TouchPhase.Moved:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState) && touchState.Phase != TouchPhase.Canceled)
                        {
                            moveTouch(touchState.Id, t.position);
                        }
                        else
                        {
                            // Missed began phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalBeginTouch(t.position).Id));
                        }
                        break;
                    case TouchPhase.Ended:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalEndTouch(touchState.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            internalEndTouch(internalBeginTouch(t.position).Id);
                        }
                        break;
                    case TouchPhase.Canceled:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            if (touchState.Phase != TouchPhase.Canceled) internalCancelTouch(touchState.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            internalCancelTouch(internalBeginTouch(t.position).Id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (systemToInternalId.TryGetValue(t.fingerId, out touchState)) {}
                        else
                        {
                            // Missed begin phase
                            systemToInternalId.Add(t.fingerId, new TouchState(internalBeginTouch(t.position).Id));
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        public bool CancelTouch(TouchPoint touch, bool @return)
        {
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
                if (@return)
                {
                    cancelTouch(touch.Id);
                    systemToInternalId[fingerId] = new TouchState(beginTouch(touch.Position, touch.Tags, false).Id);
                }
                else
                {
                    systemToInternalId[fingerId] = new TouchState(touch.Id, TouchPhase.Canceled);
                    internalCancelTouch(touch.Id);
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
                if (touchState.Value.Phase != TouchPhase.Canceled) internalCancelTouch(touchState.Value.Id);
            }
            systemToInternalId.Clear();
        }

        #endregion

        #region Private functions

        private TouchPoint internalBeginTouch(Vector2 position)
        {
            touchesNum++;
            return beginTouch(position, tags, true);
        }

        private void internalEndTouch(int id)
        {
            touchesNum--;
            endTouch(id);
        }

        private void internalCancelTouch(int id)
        {
            touchesNum--;
            cancelTouch(id);
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