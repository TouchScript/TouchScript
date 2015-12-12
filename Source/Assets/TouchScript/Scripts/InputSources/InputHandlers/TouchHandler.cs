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
        private Dictionary<int, int> systemToInternalId = new Dictionary<int, int>();
        private int touchesNum;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHandler"/> class.
        /// </summary>
        /// <param name="beginTouch"> A function called when a new touch is detected. As <see cref="InputSource.beginTouch(Vector2)"/> this function must accept a Vector2 position of the new touch and return an instance of <see cref="TouchPoint"/>. </param>
        /// <param name="moveTouch"> A function called when a touch is moved. As <see cref="InputSource.moveTouch"/> this function must accept an int id and a Vector2 position. </param>
        /// <param name="endTouch"> A function called when a touch is lifted off. As <see cref="InputSource.endTouch"/> this function must accept an int id. </param>
        /// <param name="cancelTouch"> A function called when a touch is cancelled. As <see cref="InputSource.cancelTouch"/> this function must accept an int id. </param>
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

                int id;
                switch (t.phase)
                {
                    case TouchPhase.Began:
                        if (systemToInternalId.TryGetValue(t.fingerId, out id))
                        {
                            // Ending previous touch (missed a frame)
                            internalEndTouch(id);
                            id = internalBeginTouch(t.position).Id;
                            systemToInternalId[t.fingerId] = id;
                        }
                        else
                        {
                            id = internalBeginTouch(t.position).Id;
                            systemToInternalId.Add(t.fingerId, id);
                        }
                        break;
                    case TouchPhase.Moved:
                        if (systemToInternalId.TryGetValue(t.fingerId, out id))
                        {
                            moveTouch(id, t.position);
                        }
                        else
                        {
                            // Missed began phase
                            id = internalBeginTouch(t.position).Id;
                            systemToInternalId.Add(t.fingerId, id);
                        }
                        break;
                    case TouchPhase.Ended:
                        if (systemToInternalId.TryGetValue(t.fingerId, out id))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            internalEndTouch(id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            id = internalBeginTouch(t.position).Id;
                            internalEndTouch(id);
                        }
                        break;
                    case TouchPhase.Canceled:
                        if (systemToInternalId.TryGetValue(t.fingerId, out id))
                        {
                            systemToInternalId.Remove(t.fingerId);
                            internalCancelTouch(id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            id = internalBeginTouch(t.position).Id;
                            internalCancelTouch(id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (systemToInternalId.TryGetValue(t.fingerId, out id)) {}
                        else
                        {
                            // Missed begin phase
                            id = internalBeginTouch(t.position).Id;
                            systemToInternalId.Add(t.fingerId, id);
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        public bool ReturnTouch(TouchPoint touch)
        {
            int fingerId = -1;
            foreach (var touchState in systemToInternalId)
            {
                if (touchState.Value == touch.Id)
                {
                    fingerId = touchState.Key;
                    break;
                }
            }
            if (fingerId > -1)
            {
                systemToInternalId[fingerId] = beginTouch(touch.Position, touch.Tags, false).Id;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var touchState in systemToInternalId) internalCancelTouch(touchState.Value);
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
    }
}