/*
 * @author Michael Holub
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Editor.Attributes;
using UnityEngine;
using System.Collections.Generic;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Mobile Input Source. Gathers touch input from built-in Unity's Input.Touches API. Though, should be used on mobile devices.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Mobile Input")]
    public sealed class MobileInput : InputSource
    {

        #region Public properties

        /// <summary>
        /// Indicates if this input source should be disabled on platforms which don't support touch input with Input.Touches.
        /// </summary>
        [ToggleLeft]
        public bool DisableOnNonTouchPlatforms = true;

        public Tags Tags = new Tags(Tags.INPUT_TOUCH);
        #endregion

        #region Private variables

        private Dictionary<int, TouchState> touchStates = new Dictionary<int, TouchState>();
        private HashSet<int> touchIds = new HashSet<int>();

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            if (DisableOnNonTouchPlatforms)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.MetroPlayerARM:
                    case RuntimePlatform.MetroPlayerX64:
                    case RuntimePlatform.MetroPlayerX86:
                    case RuntimePlatform.WP8Player:
                        break;
                    default:
                        // don't need mobile touch here
                        enabled = false;
                        return;
                }
            }

            base.OnEnable();

            touchStates.Clear();
            touchIds.Clear();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            foreach (var touchState in touchStates)
            {
                cancelTouch(touchState.Value.Id);
            }

            base.OnDisable();
        }

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
                            int id = beginTouch(t.position).Id;
                            touchStates[t.fingerId] = new TouchState(id, t.phase, t.position);
                        } else
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
                        } else
                        {
                            // maybe we missed began phase
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
                        } else
                        {
                            // maybe we totally missed one finger begin-end transition
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
                        } else
                        {
                            // maybe we totally missed one finger begin-end transition
                            int id = beginTouch(t.position).Id;
                            cancelTouch(id);
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (touchIds.Contains(t.fingerId))
                        {} else
                        {
                            touchIds.Add(t.fingerId);
                            int id = beginTouch(t.position).Id;
                            touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));
                        }
                        break;
                }
            }
        }

        protected override ITouch beginTouch(Vector2 position)
        {
            return beginTouch(position, new Tags(Tags));
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