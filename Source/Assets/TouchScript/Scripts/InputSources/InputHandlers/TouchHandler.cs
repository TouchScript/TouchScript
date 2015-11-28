/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.InputSources.InputHandlers
{
    public class TouchHandler : IDisposable
    {

        #region Private variables

        private Func<Vector2, ITouch> beginTouch;
        private Action<int, Vector2> moveTouch;
        private Action<int> endTouch;
        private Action<int> cancelTouch;

        private Dictionary<int, TouchState> touchStates = new Dictionary<int, TouchState>();
        private HashSet<int> touchIds = new HashSet<int>();

        #endregion

        public TouchHandler(Func<Vector2, ITouch> beginTouch, Action<int, Vector2> moveTouch, Action<int> endTouch,
            Action<int> cancelTouch)
        {
            this.beginTouch = beginTouch;
            this.moveTouch = moveTouch;
            this.endTouch = endTouch;
            this.cancelTouch = cancelTouch;
        }

        #region Public methods

        public void Update()
        {
            for (var i = 0; i < Input.touchCount; ++i)
            {
                var t = Input.GetTouch(i);

                switch (t.phase)
                {
                    case TouchPhase.Began:
                        if (touchIds.Contains(t.fingerId))
                        {
                            // Ending previous touch (missed a frame)
                            endTouch(t.fingerId);
                            int id = beginTouch(t.position).Id;
                            touchStates[t.fingerId] = new TouchState(id, t.phase, t.position);
                        }
                        else
                        {
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position).Id;
                            touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));
                        }
                        break;
                    case TouchPhase.Moved:
                        if (touchIds.Contains(t.fingerId))
                        {
                            var ts = touchStates[t.fingerId];
                            touchStates[t.fingerId] = new TouchState(ts.Id, t.phase, t.position);
                            moveTouch(ts.Id, t.position);
                        }
                        else
                        {
                            // Missed began phase
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position).Id;
                            touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));
                        }
                        break;
                    case TouchPhase.Ended:
                        if (touchIds.Contains(t.fingerId))
                        {
                            var ts = touchStates[t.fingerId];
                            touchIds.Remove(t.fingerId);
                            touchStates.Remove(t.fingerId);
                            endTouch(ts.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            int id = beginTouch(t.position).Id;
                            endTouch(id);
                        }
                        break;
                    case TouchPhase.Canceled:
                        if (touchIds.Contains(t.fingerId))
                        {
                            var ts = touchStates[t.fingerId];
                            touchIds.Remove(t.fingerId);
                            touchStates.Remove(t.fingerId);
                            endTouch(ts.Id);
                        }
                        else
                        {
                            // Missed one finger begin-end transition
                            int id = beginTouch(t.position).Id;
                            cancelTouch(id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (touchIds.Contains(t.fingerId)) { }
                        else
                        {
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position).Id;
                            touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));
                        }
                        break;
                }
            }
        }

        public void Dispose()
        {
            foreach (var touchState in touchStates) cancelTouch(touchState.Value.Id);
            touchStates.Clear();
        }

        #endregion

        internal struct TouchState
        {
            public int Id;
            public TouchPhase Phase;
            public Vector2 Position;

            public TouchState(int id, TouchPhase phase, Vector2 position)
            {
                Id = id;
                Phase = phase;
                Position = position;
            }
        }
    }
}
