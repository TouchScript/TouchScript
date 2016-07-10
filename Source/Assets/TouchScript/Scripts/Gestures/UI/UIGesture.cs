/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Gestures.UI
{
    /// <summary>
    /// <para>Gesture which receives pointer input from TouchScript and routes it to Unity UI components on the same GameObject.</para>
    /// <para>Mostly needed for UI buttons to work with <see cref="TouchScript.Layers.UILayer"/>.</para>
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/UI Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_UI_UIGesture.htm")]
    public class UIGesture : Gesture
    {
        #region Protected variables

        /// <summary>
        /// Pointer id -> pointer data.
        /// </summary>
        protected Dictionary<int, PointerData> pointerData = new Dictionary<int, PointerData>();

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
        protected override void pointersPressed(IList<Pointer> pointers)
        {
            base.pointersPressed(pointers);

            if (NumPointers == pointers.Count) setState(GestureState.Began);

            for (var i = 0; i < pointers.Count; i++)
            {
                var pointer = pointers[i];
                var data = getPointerData(pointer);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerDownHandler);
            }
        }

        /// <inheritdoc />
        protected override void pointersUpdated(IList<Pointer> pointers)
        {
            base.pointersUpdated(pointers);

            for (var i = 0; i < pointers.Count; i++)
            {
                var pointer = pointers[i];
                var data = getPointerData(pointer);
                if (PointerUtils.IsPointerOnTarget(pointer, cachedTransform))
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
                setPointerData(pointer, data);
            }
        }

        /// <inheritdoc />
        protected override void pointersReleased(IList<Pointer> pointers)
        {
            base.pointersReleased(pointers);

            PointerData onTarget = new PointerData();
            for (var i = 0; i < pointers.Count; i++)
            {
                var pointer = pointers[i];
                var data = getPointerData(pointer);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerUpHandler);
                if (data.OnTarget) onTarget = data;
                removePointerData(pointer);
            }

            // One of the pointers was released ontop of the target
            if (onTarget.OnTarget) ExecuteEvents.Execute(gameObject, onTarget.Data, ExecuteEvents.pointerClickHandler);

            if (activePointers.Count == 0) setState(GestureState.Ended);
        }

        /// <inheritdoc />
        protected override void pointersCancelled(IList<Pointer> pointers)
        {
            base.pointersCancelled(pointers);

            for (var i = 0; i < pointers.Count; i++)
            {
                var pointer = pointers[i];
                var data = getPointerData(pointer);
                ExecuteEvents.Execute(gameObject, data.Data, ExecuteEvents.pointerUpHandler);
                removePointerData(pointer);
            }

            if (activePointers.Count == 0) setState(GestureState.Ended);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Gets or creates pointer data for pointer.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <returns> Pointer data. </returns>
        protected virtual PointerData getPointerData(Pointer pointer)
        {
            PointerData data;
            if (!pointerData.TryGetValue(pointer.Id, out data))
            {
                data = new PointerData
                {
                    OnTarget = true,
                    Data = new PointerEventData(EventSystem.current)
                    {
                        pointerId = pointer.Id,
                        pointerEnter = gameObject,
                        pointerPress = gameObject,
                        eligibleForClick = true,
                        delta = Vector2.zero,
                        dragging = false,
                        useDragThreshold = true,
                        position = pointer.Position,
                        pressPosition = pointer.Position,
                        pointerPressRaycast = pointer.Hit.RaycastResult,
                        pointerCurrentRaycast = pointer.Hit.RaycastResult
                    }
                };
                pointerData.Add(pointer.Id, data);
            }
            return data;
        }

        /// <summary>
        /// Sets pointer data for pointer.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        /// <param name="data"> The data. </param>
        protected virtual void setPointerData(Pointer pointer, PointerData data)
        {
            if (pointerData.ContainsKey(pointer.Id)) pointerData[pointer.Id] = data;
        }

        /// <summary>
        /// Removes pointer data for pointer.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        protected virtual void removePointerData(Pointer pointer)
        {
            pointerData.Remove(pointer.Id);
        }

        #endregion

        /// <summary>
        /// Pointer data value object.
        /// </summary>
        protected struct PointerData
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
            /// Initializes a new instance of the <see cref="PointerData"/> struct.
            /// </summary>
            /// <param name="onTarget">if set to <c>true</c> pointer is on target.</param>
            /// <param name="data">The data.</param>
            public PointerData(bool onTarget = false, PointerEventData data = null)
            {
                OnTarget = onTarget;
                Data = data;
            }
        }
    }
}