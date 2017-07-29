/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using TouchScript.Hit;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pointer = TouchScript.Pointers.Pointer;
using UnityEngine.Profiling;

namespace TouchScript.Layers.UI
{
    /// <summary>
    /// An implementation of a Unity UI Input Module which lets TouchScript interact with the UI and EventSystem.
    /// </summary>
    internal sealed class TouchScriptInputModule : BaseInputModule
    {
        #region Public properties

        /// <summary>
        /// TouchScriptInputModule singleton instance.
        /// </summary>
        public static TouchScriptInputModule Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    var es = EventSystem.current;
                    if (es == null)
                    {
                        es = FindObjectOfType<EventSystem>();
                        if (es == null)
                        {
                            var go = new GameObject("EventSystem");
                            es = go.AddComponent<EventSystem>();
                        }
                    }
                    instance = es.GetComponent<TouchScriptInputModule>();
                    if (instance == null) instance = es.gameObject.AddComponent<TouchScriptInputModule>();
                }
                return instance;
            }
        }

        public string HorizontalAxis = "Horizontal";
        public string VerticalAxis = "Vertical";
        public string SubmitButton = "Submit";
        public string CancelButton = "Cancel";
        public float InputActionsPerSecond = 10;
        public float RepeatDelay = 0.5f;

        #endregion

        #region Private variables

        private static bool shuttingDown = false;
        private static TouchScriptInputModule instance;
        private static FieldInfo raycastersProp;
        private static PropertyInfo canvasProp;
        private static Dictionary<int, Canvas> raycasterCanvasCache = new Dictionary<int, Canvas>(10);

        private int refCount = 0;
        private UIStandardInputModule ui;

        #endregion

        #region Constructor

        private TouchScriptInputModule()
        {
            if (raycastersProp == null)
            {
                raycastersProp = Type.GetType(Assembly.CreateQualifiedName("UnityEngine.UI", "UnityEngine.EventSystems.RaycasterManager")).
                                     GetField("s_Raycasters", BindingFlags.NonPublic | BindingFlags.Static);
                canvasProp = typeof(GraphicRaycaster).GetProperty("canvas", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        #endregion

        #region Unity methods

        protected override void OnEnable()
        {
            base.OnEnable();

            if (instance == null) instance = this;
            else
            {
                if (instance == this) return;
                Destroy(this);
            }
        }

        protected override void OnDisable()
        {
            disable();
            if (instance == this) instance = null;
            base.OnDisable();
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns all UI raycasters in the scene.
        /// </summary>
        /// <returns> Array of raycasters. </returns>
        public List<BaseRaycaster> GetRaycasters()
        {
            return raycastersProp.GetValue(null) as List<BaseRaycaster>;
        }

        /// <summary>
        /// Returns a Canvas for a raycaster.
        /// </summary>
        /// <param name="raycaster">The raycaster.</param>
        /// <returns> The Canvas this raycaster is on. </returns>
        public Canvas GetCanvasForRaycaster(BaseRaycaster raycaster)
        {
            var id = raycaster.GetInstanceID();
            Canvas canvas;
            if (!raycasterCanvasCache.TryGetValue(id, out canvas))
            {
                canvas = canvasProp.GetValue(raycaster, null) as Canvas;
                raycasterCanvasCache.Add(id, canvas);
            }
            return canvas;
        }

        public override void Process()
        {
            if (ui != null) ui.Process();
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            if (ui != null) return ui.IsPointerOverGameObject(pointerId);
            return false;
        }

        public override bool ShouldActivateModule()
        {
            return true;
        }

        public override bool IsModuleSupported()
        {
            return true;
        }

        public override void DeactivateModule() {}

        public override void ActivateModule() {}

        public override void UpdateModule() {}

        #endregion

        #region Internal methods

        /// <summary>
        /// Marks that this object is used by some other object.
        /// </summary>
        internal void INTERNAL_Retain()
        {
            refCount++;
            if (refCount == 1) enable();
        }

        /// <summary>
        /// Releases a lock on this object.
        /// </summary>
        /// <returns> The number of objects still using this object. </returns>
        internal int INTERNAL_Release()
        {
            if (--refCount <= 0) disable();
            return refCount;
        }

        #endregion

        #region Private functions

        private void enable()
        {
            ui = new UIStandardInputModule(this);
            TouchManager.Instance.PointersUpdated += ui.ProcessUpdated;
            TouchManager.Instance.PointersPressed += ui.ProcessPressed;
            TouchManager.Instance.PointersReleased += ui.ProcessReleased;
            TouchManager.Instance.PointersRemoved += ui.ProcessRemoved;
            TouchManager.Instance.PointersCancelled += ui.ProcessCancelled;
        }

        private void disable()
        {
            if (TouchManager.Instance != null && ui != null)
            {
                TouchManager.Instance.PointersUpdated -= ui.ProcessUpdated;
                TouchManager.Instance.PointersPressed -= ui.ProcessPressed;
                TouchManager.Instance.PointersReleased -= ui.ProcessReleased;
                TouchManager.Instance.PointersRemoved -= ui.ProcessRemoved;
                TouchManager.Instance.PointersCancelled -= ui.ProcessCancelled;
            }
            refCount = 0;
        }

        #endregion

        #region Copy-pasted code from UI

        /// <summary>
        /// Basically, copied code from UI Input Module which handles all UI pointer processing logic.
        /// Last update: df1947cd (5.4f3)
        /// </summary>
        private class UIStandardInputModule
        {
            protected TouchScriptInputModule input;

			private CustomSampler uiSampler;

            public UIStandardInputModule(TouchScriptInputModule input)
            {
                this.input = input;

				uiSampler = CustomSampler.Create("[TouchScript] Update UI");
            }

            #region Unchanged from PointerInputModule

            private int m_ConsecutiveMoveCount = 0;
            private Vector2 m_LastMoveVector;
            private float m_PrevActionTime;

            private Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>(10);

            public bool IsPointerOverGameObject(int pointerId)
            {
                var lastPointer = GetLastPointerEventData(pointerId);
                if (lastPointer != null)
                    return lastPointer.pointerEnter != null;
                return false;
            }

            protected bool GetPointerData(int id, out PointerEventData data, bool create)
            {
                if (!m_PointerData.TryGetValue(id, out data) && create)
                {
                    data = new PointerEventData(input.eventSystem)
                    {
                        pointerId = id,
                    };
                    m_PointerData.Add(id, data);
                    return true;
                }
                return false;
            }

            protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
            {
                // Selection tracking
                var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
                // if we have clicked something new, deselect the old thing
                // leave 'selection handling' up to the press event though.
                if (selectHandlerGO != input.eventSystem.currentSelectedGameObject)
                    input.eventSystem.SetSelectedGameObject(null, pointerEvent);
            }

            protected PointerEventData GetLastPointerEventData(int id)
            {
                PointerEventData data;
                GetPointerData(id, out data, false);
                return data;
            }

            private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
            {
                if (!useDragThreshold)
                    return true;

                return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
            }

            private bool SendUpdateEventToSelectedObject()
            {
                if (input.eventSystem.currentSelectedGameObject == null)
                    return false;

                var data = input.GetBaseEventData();
                ExecuteEvents.Execute(input.eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
                return data.used;
            }

            private bool SendMoveEventToSelectedObject()
            {
                float time = Time.unscaledTime;

                Vector2 movement = GetRawMoveVector();
                if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
                {
                    m_ConsecutiveMoveCount = 0;
                    return false;
                }

                // If user pressed key again, always allow event
                bool allow = Input.GetButtonDown(input.HorizontalAxis) || Input.GetButtonDown(input.VerticalAxis);
                bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);
                if (!allow)
                {
                    // Otherwise, user held down key or axis.
                    // If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
                    if (similarDir && m_ConsecutiveMoveCount == 1)
                        allow = (time > m_PrevActionTime + input.RepeatDelay);
                    // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
                    else
                        allow = (time > m_PrevActionTime + 1f / input.InputActionsPerSecond);
                }
                if (!allow)
                    return false;

                // Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
                var axisEventData = input.GetAxisEventData(movement.x, movement.y, 0.6f);

                if (axisEventData.moveDir != MoveDirection.None)
                {
                    ExecuteEvents.Execute(input.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
                    if (!similarDir)
                        m_ConsecutiveMoveCount = 0;
                    m_ConsecutiveMoveCount++;
                    m_PrevActionTime = time;
                    m_LastMoveVector = movement;
                }
                else
                {
                    m_ConsecutiveMoveCount = 0;
                }

                return axisEventData.used;
            }

            private bool SendSubmitEventToSelectedObject()
            {
                if (input.eventSystem.currentSelectedGameObject == null)
                    return false;

                var data = input.GetBaseEventData();
                if (Input.GetButtonDown(input.SubmitButton))
                    ExecuteEvents.Execute(input.eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

                if (Input.GetButtonDown(input.CancelButton))
                    ExecuteEvents.Execute(input.eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
                return data.used;
            }

            private Vector2 GetRawMoveVector()
            {
                Vector2 move = Vector2.zero;
                move.x = Input.GetAxisRaw(input.HorizontalAxis);
                move.y = Input.GetAxisRaw(input.VerticalAxis);

                if (Input.GetButtonDown(input.HorizontalAxis))
                {
                    if (move.x < 0)
                        move.x = -1f;
                    if (move.x > 0)
                        move.x = 1f;
                }
                if (Input.GetButtonDown(input.VerticalAxis))
                {
                    if (move.y < 0)
                        move.y = -1f;
                    if (move.y > 0)
                        move.y = 1f;
                }
                return move;
            }

            #endregion

            public void Process()
            {
                bool usedEvent = SendUpdateEventToSelectedObject();

                if (input.eventSystem.sendNavigationEvents)
                {
                    if (!usedEvent)
                        usedEvent |= SendMoveEventToSelectedObject();

                    if (!usedEvent)
                        SendSubmitEventToSelectedObject();
                }

                // touch needs to take precedence because of the mouse emulation layer
                //                if (!ProcessTouchEvents() && Input.mousePresent)
                //                    ProcessMouseEvent();
            }

            #region Changed

            protected void RemovePointerData(int id)
            {
                m_PointerData.Remove(id);
            }

            private void convertRaycast(RaycastHitUI old, ref RaycastResult current)
            {
                current.module = old.Raycaster;
                current.gameObject = old.Target == null ? null : old.Target.gameObject;
                current.depth = old.Depth;
                current.index = old.GraphicIndex;
                current.sortingLayer = old.SortingLayer;
                current.sortingOrder = old.SortingOrder;
            }

            #endregion

            #region Event processors

            public virtual void ProcessUpdated(object sender, PointerEventArgs pointerEventArgs)
            {
				uiSampler.Begin();

                var pointers = pointerEventArgs.Pointers;
                var raycast = new RaycastResult();
                var count = pointers.Count;
                for (var i = 0; i < count; i++)
                {
                    var pointer = pointers[i];
					// Don't update the pointer if it is pressed not over an UI element
					if ((pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed) > 0) 
					{
						var press = pointer.GetPressData();
						if (press.Type != HitData.HitType.UI) continue;
					}

                    var over = pointer.GetOverData();
					// Don't update the pointer if it is not over an UI element
                    if (over.Type != HitData.HitType.UI) continue;

                    PointerEventData data;
                    GetPointerData(pointer.Id, out data, true);
                    data.Reset();
                    var target = over.Target;
                    var currentOverGo = target == null ? null : target.gameObject;

                    data.position = pointer.Position;
                    data.delta = pointer.Position - pointer.PreviousPosition;
                    convertRaycast(over.RaycastHitUI, ref raycast);
                    raycast.screenPosition = data.position;
                    data.pointerCurrentRaycast = raycast;

                    input.HandlePointerExitAndEnter(data, currentOverGo);

                    bool moving = data.IsPointerMoving();

                    if (moving && data.pointerDrag != null
                        && !data.dragging
                        && ShouldStartDrag(data.pressPosition, data.position, input.eventSystem.pixelDragThreshold, data.useDragThreshold))
                    {
                        ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.beginDragHandler);
                        data.dragging = true;
                    }

                    // Drag notification
                    if (data.dragging && moving && data.pointerDrag != null)
                    {
                        // Before doing drag we should cancel any pointer down state
                        // And clear selection!
                        if (data.pointerPress != data.pointerDrag)
                        {
                            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

                            data.eligibleForClick = false;
                            data.pointerPress = null;
                            data.rawPointerPress = null;
                        }
                        ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.dragHandler);
                    }

                    var mousePointer = pointer as MousePointer;
                    if (mousePointer != null && !Mathf.Approximately(mousePointer.ScrollDelta.sqrMagnitude, 0.0f))
                    {
                        data.scrollDelta = mousePointer.ScrollDelta;
                        var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(currentOverGo);
                        ExecuteEvents.ExecuteHierarchy(scrollHandler, data, ExecuteEvents.scrollHandler);
                    }
                }

				uiSampler.End();
            }

            public virtual void ProcessPressed(object sender, PointerEventArgs pointerEventArgs)
            {
				uiSampler.Begin();

                var pointers = pointerEventArgs.Pointers;
                var count = pointers.Count;
                for (var i = 0; i < count; i++)
                {
                    var pointer = pointers[i];

                    var over = pointer.GetOverData();
					// Don't update the pointer if it is not over an UI element
                    if (over.Type != HitData.HitType.UI) continue;

                    PointerEventData data;
                    GetPointerData(pointer.Id, out data, true);
                    var target = over.Target;
                    var currentOverGo = target == null ? null : target.gameObject;

                    data.eligibleForClick = true;
                    data.delta = Vector2.zero;
                    data.dragging = false;
                    data.useDragThreshold = true;
                    data.pressPosition = pointer.Position;
                    data.pointerPressRaycast = data.pointerCurrentRaycast;

                    DeselectIfSelectionChanged(currentOverGo, data);

                    if (data.pointerEnter != currentOverGo)
                    {
                        // send a pointer enter to the touched element if it isn't the one to select...
                        input.HandlePointerExitAndEnter(data, currentOverGo);
                        data.pointerEnter = currentOverGo;
                    }

                    // search for the control that will receive the press
                    // if we can't find a press handler set the press
                    // handler to be what would receive a click.
                    var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, data, ExecuteEvents.pointerDownHandler);

                    // didnt find a press handler... search for a click handler
                    if (newPressed == null)
                        newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                    // Debug.Log("Pressed: " + newPressed);

                    float time = Time.unscaledTime;

                    if (newPressed == data.lastPress) // ?
                    {
                        var diffTime = time - data.clickTime;
                        if (diffTime < 0.3f)
                            ++data.clickCount;
                        else
                            data.clickCount = 1;

                        data.clickTime = time;
                    }
                    else
                    {
                        data.clickCount = 1;
                    }

                    data.pointerPress = newPressed;
                    data.rawPointerPress = currentOverGo;

                    data.clickTime = time;

                    // Save the drag handler as well
                    data.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                    if (data.pointerDrag != null)
                        ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.initializePotentialDrag);
                }

				uiSampler.End();
            }

            public virtual void ProcessReleased(object sender, PointerEventArgs pointerEventArgs)
            {
				uiSampler.Begin();

                var pointers = pointerEventArgs.Pointers;
                var count = pointers.Count;
                for (var i = 0; i < count; i++)
                {
                    var pointer = pointers[i];
					var press = pointer.GetPressData();
					// Don't update the pointer if it is was not pressed over an UI element
					if (press.Type != HitData.HitType.UI) continue;

                    var over = pointer.GetOverData();

                    PointerEventData data;
                    GetPointerData(pointer.Id, out data, true);
                    var target = over.Target;
                    var currentOverGo = target == null ? null : target.gameObject;

                    ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);
                    var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                    if (data.pointerPress == pointerUpHandler && data.eligibleForClick)
                    {
                        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
                    }
                    else if (data.pointerDrag != null && data.dragging)
                    {
                        ExecuteEvents.ExecuteHierarchy(currentOverGo, data, ExecuteEvents.dropHandler);
                    }

                    data.eligibleForClick = false;
                    data.pointerPress = null;
                    data.rawPointerPress = null;

                    if (data.pointerDrag != null && data.dragging)
                        ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);

                    data.dragging = false;
                    data.pointerDrag = null;

                    // send exit events as we need to simulate this on touch up on touch device
                    ExecuteEvents.ExecuteHierarchy(data.pointerEnter, data, ExecuteEvents.pointerExitHandler);
                    data.pointerEnter = null;

                    // redo pointer enter / exit to refresh state
                    // so that if we moused over somethign that ignored it before
                    // due to having pressed on something else
                    // it now gets it.
                    if (currentOverGo != data.pointerEnter)
                    {
                        input.HandlePointerExitAndEnter(data, null);
                        input.HandlePointerExitAndEnter(data, currentOverGo);
                    }
                }

				uiSampler.End();
            }

            public virtual void ProcessCancelled(object sender, PointerEventArgs pointerEventArgs)
            {
				uiSampler.Begin();

                var pointers = pointerEventArgs.Pointers;
                var count = pointers.Count;
                for (var i = 0; i < count; i++)
                {
                    var pointer = pointers[i];

                    var over = pointer.GetOverData();

                    PointerEventData data;
                    GetPointerData(pointer.Id, out data, true);
                    var target = over.Target;
                    var currentOverGo = target == null ? null : target.gameObject;

                    ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

                    if (data.pointerDrag != null && data.dragging)
                    {
                        ExecuteEvents.ExecuteHierarchy(currentOverGo, data, ExecuteEvents.dropHandler);
                    }

                    data.eligibleForClick = false;
                    data.pointerPress = null;
                    data.rawPointerPress = null;

                    if (data.pointerDrag != null && data.dragging)
                        ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);

                    data.dragging = false;
                    data.pointerDrag = null;

                    // send exit events as we need to simulate this on touch up on touch device
                    ExecuteEvents.ExecuteHierarchy(data.pointerEnter, data, ExecuteEvents.pointerExitHandler);
                    data.pointerEnter = null;
                }

				uiSampler.End();
            }

            public virtual void ProcessRemoved(object sender, PointerEventArgs pointerEventArgs)
            {
				uiSampler.Begin();

                var pointers = pointerEventArgs.Pointers;
                var count = pointers.Count;
                for (var i = 0; i < count; i++)
                {
                    var pointer = pointers[i];

					var over = pointer.GetOverData();
					// Don't update the pointer if it is not over an UI element
					if (over.Type != HitData.HitType.UI) continue;

                    PointerEventData data;
                    GetPointerData(pointer.Id, out data, true);

                    if (data.pointerEnter) ExecuteEvents.ExecuteHierarchy(data.pointerEnter, data, ExecuteEvents.pointerExitHandler);
                    RemovePointerData(pointer.Id);
                }

				uiSampler.End();
            }

            #endregion
        }

        #endregion
    }
}