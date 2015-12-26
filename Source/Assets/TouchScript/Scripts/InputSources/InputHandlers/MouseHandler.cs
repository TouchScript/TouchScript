/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    /// <summary>
    /// Unity mouse handling implementation which can be embedded and controlled from other (input) classes.
    /// </summary>
    public class MouseHandler : IDisposable
    {
        #region Private variables

        private Func<Vector2, Tags, bool, TouchPoint> beginTouch;
        private Action<int, Vector2> moveTouch;
        private Action<int> endTouch;
        private Action<int> cancelTouch;

        private Tags tags;
        private int mousePointId = -1;
        private int fakeMousePointId = -1;
        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseHandler" /> class.
        /// </summary>
        /// <param name="tags">Tags to add to touches.</param>
        /// <param name="beginTouch">A function called when a new touch is detected. As <see cref="InputSource.beginTouch" /> this function must accept a Vector2 position of the new touch and return an instance of <see cref="TouchPoint" />.</param>
        /// <param name="moveTouch">A function called when a touch is moved. As <see cref="InputSource.moveTouch" /> this function must accept an int id and a Vector2 position.</param>
        /// <param name="endTouch">A function called when a touch is lifted off. As <see cref="InputSource.endTouch" /> this function must accept an int id.</param>
        /// <param name="cancelTouch">A function called when a touch is cancelled. As <see cref="InputSource.cancelTouch" /> this function must accept an int id.</param>
        public MouseHandler(Tags tags, Func<Vector2, Tags, bool, TouchPoint> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch, Action<int> cancelTouch)
        {
            this.tags = tags;
            this.beginTouch = beginTouch;
            this.moveTouch = moveTouch;
            this.endTouch = endTouch;
            this.cancelTouch = cancelTouch;

            mousePointId = -1;
            fakeMousePointId = -1;
        }

        #region Public methods

        /// <summary>
        /// Immediately ends all touches.
        /// </summary>
        public void EndTouches()
        {
            if (mousePointId != -1)
            {
                endTouch(mousePointId);
                mousePointId = -1;
            }
            if (fakeMousePointId != -1)
            {
                endTouch(fakeMousePointId);
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
                    endTouch(mousePointId);
                    mousePointId = -1;
                    upHandled = true;
                }
            }

            // Need to end fake pointer
            if (fakeMousePointId > -1 && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endTouch(fakeMousePointId);
                fakeMousePointId = -1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointId == -1)
                    fakeMousePointId = beginTouch(new Vector2(pos.x, pos.y), tags, true).Id;
                else if (mousePointId == -1) mousePointId = beginTouch(new Vector2(pos.x, pos.y), tags, true).Id;
            }
            else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointId != -1)
                    {
                        if (mousePointId == -1) moveTouch(fakeMousePointId, new Vector2(pos.x, pos.y));
                        else moveTouch(mousePointId, new Vector2(pos.x, pos.y));
                    }
                    else if (mousePointId != -1) moveTouch(mousePointId, new Vector2(pos.x, pos.y));
                }
            }

            // Release mouse if we haven't done it yet
            if (Input.GetMouseButtonUp(0) && !upHandled && mousePointId != -1)
            {
                endTouch(mousePointId);
                mousePointId = -1;
            }
        }

        /// <inheritdoc />
        public bool CancelTouch(TouchPoint touch, bool @return)
        {
            if (touch.Id == mousePointId)
            {
                cancelTouch(mousePointId);
                if (@return) mousePointId = beginTouch(touch.Position, tags, false).Id;
                else mousePointId = -1;
                return true;
            }
            if (touch.Id == fakeMousePointId)
            {
                cancelTouch(fakeMousePointId);
                if (@return) fakeMousePointId = beginTouch(touch.Position, tags, false).Id;
                else fakeMousePointId = -1;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (mousePointId != -1) cancelTouch(mousePointId);
            if (fakeMousePointId != -1) cancelTouch(fakeMousePointId);
        }

        #endregion
    }
}