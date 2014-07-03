/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.InputSources
{
    internal class MouseHandler
    {

        private Func<Vector2, int> beginTouch;
        private Action<int, Vector2> moveTouch;
        private Action<int> endTouch;
        private Action<int> cancelTouch;

        private int mousePointId = -1;
        private int fakeMousePointId = -1;
        private Vector3 mousePointPos = Vector3.zero;

        public MouseHandler(Func<Vector2, int> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch,
            Action<int> cancelTouch)
        {
            this.beginTouch = beginTouch;
            this.moveTouch = moveTouch;
            this.endTouch = endTouch;
            this.cancelTouch = cancelTouch;

            mousePointId = -1;
            fakeMousePointId = -1;
        }

        public void Update()
        {
            var upHandled = false;
            if (Input.GetMouseButtonUp(0))
            {
                if (mousePointId != -1)
                {
                    endTouch(mousePointId);
                    mousePointId = -1;
                    upHandled = true;
                }
            }

            if (fakeMousePointId > -1 && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endTouch(fakeMousePointId);
                fakeMousePointId = -1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointId == -1)
                {
                    if (fakeMousePointId == -1) fakeMousePointId = beginTouch(new Vector2(pos.x, pos.y));
                }
                else
                {
                    if (mousePointId == -1) mousePointId = beginTouch(new Vector2(pos.x, pos.y));
                }
            }
            else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointId > -1 && mousePointId == -1)
                    {
                        moveTouch(fakeMousePointId, new Vector2(pos.x, pos.y));
                    }
                    else
                    {
                        moveTouch(mousePointId, new Vector2(pos.x, pos.y));
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && !upHandled)
            {
                endTouch(mousePointId);
                mousePointId = -1;
            }
        }

        public void Destroy()
        {
            if (mousePointId != -1) cancelTouch(mousePointId);
            if (fakeMousePointId != -1) cancelTouch(fakeMousePointId);
        }

    }
}
