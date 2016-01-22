/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Gestures.UI
{
    /// <summary>
    /// <para>Gesture which receives touch input from TouchScript and routes it to Unity UI components on the same GameObject.</para>
    /// <para>Mostly needed for UI buttons to work with <see cref="TouchScript.Layers.UILayer"/>.</para>
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/UI Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_UI_UIGesture.htm")]
    public class UIGesture : Gesture
    {
        #region Protected variables

        /// <summary>
        /// Touch id -> pointer data.
        /// </summary>
        protected Dictionary<int, TouchData> pointerData = new Dictionary<int, TouchData>();

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        public override bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        public override bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (NumTouches == touches.Count) setState(GestureState.Began);

            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var data = getPointerData(touch);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerDownHandler);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var data = getPointerData(touch);
                if (TouchUtils.IsTouchOnTarget(touch, cachedTransform))
                {
                    if (!data.OnTarget)
                    {
                        data.OnTarget = true;
                        ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerEnterHandler);
                    }
                }
                else
                {
                    if (data.OnTarget)
                    {
                        data.OnTarget = false;
                        ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerExitHandler);
                    }
                }
                setPointerData(touch, data);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            TouchData onTarget = new TouchData();
            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var data = getPointerData(touch);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerUpHandler);
                if (data.OnTarget) onTarget = data;
                removePointerData(touch);
            }

            // One of the touches was released ontop of the target
            if (onTarget.OnTarget) ExecuteEvents.Execute(gameObject, onTarget.Data, ExecuteEvents.pointerClickHandler);

            if (activeTouches.Count == 0) setState(GestureState.Ended);
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            base.touchesCancelled(touches);

            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var data = getPointerData(touch);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerUpHandler);
                removePointerData(touch);
            }

            if (activeTouches.Count == 0) setState(GestureState.Ended);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Gets or creates pointer data for touch.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        /// <returns> Pointer data. </returns>
        protected virtual TouchData getPointerData(TouchPoint touch)
        {
            TouchData data;
            if (!pointerData.TryGetValue(touch.Id, out data))
            {
                data = new TouchData
                {
                    OnTarget = true,
                    Data = new PointerEventData(EventSystem.current)
                    {
                        pointerId = touch.Id,
                        pointerEnter = gameObject,
                        pointerPress = gameObject,
                        eligibleForClick = true,
                        delta = Vector2.zero,
                        dragging = false,
                        useDragThreshold = true,
                        position = touch.Position,
                        pressPosition = touch.Position,
                        pointerPressRaycast = touch.Hit.RaycastResult,
                        pointerCurrentRaycast = touch.Hit.RaycastResult
                    }
                };
                pointerData.Add(touch.Id, data);
            }
            return data;
        }

        /// <summary>
        /// Sets pointer data for touch.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        /// <param name="data"> The data. </param>
        protected virtual void setPointerData(TouchPoint touch, TouchData data)
        {
            if (pointerData.ContainsKey(touch.Id)) pointerData[touch.Id] = data;
        }

        /// <summary>
        /// Removes pointer data for touch.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        protected virtual void removePointerData(TouchPoint touch)
        {
            pointerData.Remove(touch.Id);
        }

        #endregion

        /// <summary>
        /// Touch pointer data value object.
        /// </summary>
        protected struct TouchData
        {
            /// <summary>
            /// Is the object over the target it first hit?
            /// </summary>
            public bool OnTarget;

            /// <summary>
            /// Pointer data for UI.
            /// </summary>
            public PointerEventData Data;

            /// <summary>
            /// Initializes a new instance of the <see cref="TouchData"/> struct.
            /// </summary>
            /// <param name="onTarget">if set to <c>true</c> touch is on target.</param>
            /// <param name="data">The data.</param>
            public TouchData(bool onTarget = false, PointerEventData data = null)
            {
                OnTarget = onTarget;
                Data = data;
            }
        }
    }
}