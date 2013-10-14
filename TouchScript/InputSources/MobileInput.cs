/*
 * @author Michael Holub
 */

using UnityEngine;
using System.Collections.Generic;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Mobile Input Source
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Mobile Input")]
    public class MobileInput : InputSource
    {
        #region Private variables

        private Dictionary<int, TouchState> touchStates = new Dictionary<int, TouchState>();
        private HashSet<int> touchIds = new HashSet<int>();

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();

            for (var i = 0; i < Input.touchCount; ++i)
            {
                var t = Input.GetTouch(i);

                switch (t.phase)
                {
                    case TouchPhase.Began:
                        if (touchIds.Contains(t.fingerId))
                        {
                            // ending previous touch (maybe we missed a frame)
                            endTouch(t.fingerId);
                            int id = beginTouch(t.position);
                            touchStates[t.fingerId] = new TouchState(id, t.phase, t.position);
                        } else
                        {
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position);
                            touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));
                        }
                        break;
                    case TouchPhase.Moved:
                        if (touchIds.Contains(t.fingerId))
                        {
                            var ts = touchStates[t.fingerId];
                            touchStates[t.fingerId] = new TouchState(ts.Id, t.phase, t.position);
                            moveTouch(ts.Id, t.position);
                        } else
                        {
                            // maybe we missed began phase
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position);
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
                        } else
                        {
                            // maybe we totally missed one finger begin-end transition
                            int id = beginTouch(t.position);
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
                        } else
                        {
                            // maybe we totally missed one finger begin-end transition
                            int id = beginTouch(t.position);
                            cancelTouch(id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (touchIds.Contains(t.fingerId))
                        {} else
                        {
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position);
                            touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));
                        }
                        break;
                }
            }
        }

        #endregion
    }

    internal struct TouchState
    {
        public int Id;
        public TouchPhase Phase;
        public Vector2 Position;

        public TouchState(int anId, TouchPhase aPhase, Vector2 aPosition)
        {
            Id = anId;
            Phase = aPhase;
            Position = aPosition;
        }
    }
}