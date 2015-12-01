/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Gestures.UI
{

    [AddComponentMenu("TouchScript/Gestures/UI Gesture")]
    public class UIGesture : Gesture
    {

        #region Protected variables

        protected Dictionary<int, TouchData> pointerData = new Dictionary<int, TouchData>();

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        public override bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        public override bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count == touches.Count) setState(GestureState.Began);

            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var data = getPointerData(touch);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerDownHandler);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
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
        protected override void touchesEnded(IList<ITouch> touches)
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
            if (onTarget.OnTarget)
                ExecuteEvents.Execute(gameObject, onTarget.Data, ExecuteEvents.pointerClickHandler);

            if (activeTouches.Count == 0) setState(GestureState.Ended);
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<ITouch> touches)
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

        protected virtual TouchData getPointerData(ITouch touch)
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

        protected virtual void setPointerData(ITouch touch, TouchData data)
        {
            if (pointerData.ContainsKey(touch.Id)) pointerData[touch.Id] = data;
        }

        protected virtual void removePointerData(ITouch touch)
        {
            pointerData.Remove(touch.Id);
        }

        #endregion

        protected struct TouchData
        {
            public bool OnTarget;
            public PointerEventData Data;

            public TouchData(bool onTarget = false, PointerEventData data = null)
            {
                OnTarget = onTarget;
                Data = data;
            }

        }

    }
}
