/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Unity UI compatible Input Module which sends all TouchScript data to UI EventSystem.
    /// It works without any layers or gestures but can be used with for examle a CameraLayer and BoxColliders on UI elements to attach gestures to them.
    /// </summary>
    [AddComponentMenu("TouchScript/TouchScript Input Module")]
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Behaviors_TouchScriptInputModule.htm")]
    public class TouchScriptInputModule : BaseInputModule
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the name of Unity horizontal axis. Required to be compatible with other input.
        /// </summary>
        /// <value> The name of Unity horizontal axis. </value>
        public string HorizontalAxis
        {
            get { return horizontalAxis; }
            set { horizontalAxis = value; }
        }

        /// <summary>
        /// Gets or sets the name of Unity vertical axis.  Required to be compatible with other input.
        /// </summary>
        /// <value> The name of Unity vertical axis. </value>
        public string VerticalAxis
        {
            get { return verticalAxis; }
            set { verticalAxis = value; }
        }

        /// <summary>
        /// Gets or sets the name of Unity submit button. Required to be compatible with other input.
        /// </summary>
        /// <value> The name of Unity submit button. </value>
        public string SubmitButton
        {
            get { return submitButton; }
            set { submitButton = value; }
        }

        /// <summary>
        /// Gets or sets the name of Unity cancel button. Required to be compatible with other input.
        /// </summary>
        /// <value> The name of Unity cancel button. </value>
        public string CancelButton
        {
            get { return cancelButton; }
            set { cancelButton = value; }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// id to event data.
        /// </summary>
        protected Dictionary<int, PointerEventData> pointerEvents = new Dictionary<int, PointerEventData>();

        [SerializeField]
        private string horizontalAxis = "Horizontal";

        [SerializeField]
        private string verticalAxis = "Vertical";

        [SerializeField]
        private string submitButton = "Submit";

        [SerializeField]
        private string cancelButton = "Cancel";

        [SerializeField]
        private float inputActionsPerSecond = 10f;

        [SerializeField]
        private float repeatDelay = 0.5f;

        private float nextActionTime;
        private MoveDirection lastMoveDirection;
        private float lastMoveStartTime;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool IsModuleSupported()
        {
            return true;
        }

        /// <inheritdoc />
        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            //var shouldActivate = Input.GetButtonDown(submitButton);
            //shouldActivate |= Input.GetButtonDown(cancelButton);
            //shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(horizontalAxis), 0.0f);
            //shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(verticalAxis), 0.0f);
            //return shouldActivate;

            return true;
        }

        /// <inheritdoc />
        public override bool IsPointerOverGameObject(int pointerId)
        {
            var lastPointer = getLastPointerEventData(pointerId);
            if (lastPointer != null)
                return lastPointer.pointerEnter != null;
            return false;
        }

        /// <inheritdoc />
        public override void ActivateModule()
        {
            base.ActivateModule();

            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.TouchBegan += touchBeganHandler;
                touchManager.TouchMoved += touchMovedHandler;
                touchManager.TouchEnded += touchEndedHandler;
                touchManager.TouchCancelled += touchCancelledHandler;
            }

            var toSelect = eventSystem.currentSelectedGameObject;
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        /// <inheritdoc />
        public override void DeactivateModule()
        {
            base.DeactivateModule();

            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.TouchBegan -= touchBeganHandler;
                touchManager.TouchMoved -= touchMovedHandler;
                touchManager.TouchEnded -= touchEndedHandler;
                touchManager.TouchCancelled -= touchCancelledHandler;
            }

            clearSelection();
        }

        /// <inheritdoc />
        public override void Process()
        {
            bool usedEvent = sendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= sendMoveEventToSelectedObject();

                if (!usedEvent)
                    sendSubmitEventToSelectedObject();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("<b>Pointer Input Module of type: </b>" + GetType());
            sb.AppendLine();
            foreach (var pointer in pointerEvents)
            {
                if (pointer.Value == null)
                    continue;
                sb.AppendLine("<B>Pointer:</b> " + pointer.Key);
                sb.AppendLine(pointer.Value.ToString());
            }
            return sb.ToString();
        }

        #endregion

        #region Protected functions

        /// <summary>
        /// Does a raycast with pointer data.
        /// </summary>
        /// <param name="pointerEvent"> Pointer data. </param>
        protected void raycastPointer(PointerEventData pointerEvent)
        {
            eventSystem.RaycastAll(pointerEvent, m_RaycastResultCache);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            pointerEvent.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();
        }

        /// <summary>
        /// Initializes pointer data for a touch.
        /// </summary>
        /// <param name="touch"> The touch to initialize pointer data from. </param>
        /// <returns> Pointer data for the touch. </returns>
        protected PointerEventData initPointerData(TouchPoint touch)
        {
            PointerEventData pointerEvent;
            getPointerData(touch.Id, out pointerEvent, true);

            pointerEvent.position = touch.Position;
            pointerEvent.button = PointerEventData.InputButton.Left;
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;

            return pointerEvent;
        }

        /// <summary>
        /// Injects the pointer into UI.
        /// </summary>
        /// <param name="pointerEvent"> The pointer data to inject. </param>
        protected void injectPointer(PointerEventData pointerEvent)
        {
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            deselectIfSelectionChanged(currentOverGo, pointerEvent);

            if (pointerEvent.pointerEnter != currentOverGo)
            {
                // send a pointer enter to the touched element if it isn't the one to select...
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                pointerEvent.pointerEnter = currentOverGo;
            }

            // search for the control that will receive the press
            // if we can't find a press handler set the press
            // handler to be what would receive a click.
            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent,
                ExecuteEvents.pointerDownHandler);

            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // TODO: double-tap
            pointerEvent.clickCount = 1;
            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;
            pointerEvent.clickTime = Time.unscaledTime;

            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }

        /// <summary>
        /// Updates pointer data for touch.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        /// <returns> Updated pointer data. </returns>
        protected PointerEventData updatePointerData(TouchPoint touch)
        {
            PointerEventData pointerEvent;
            getPointerData(touch.Id, out pointerEvent, true);

            pointerEvent.position = touch.Position;
            pointerEvent.delta = touch.Position - touch.PreviousPosition;

            return pointerEvent;
        }

        /// <summary>
        /// Moves injected pointer in UI.
        /// </summary>
        /// <param name="pointerEvent"> The pointer data. </param>
        protected void movePointer(PointerEventData pointerEvent)
        {
            var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerEvent, targetGO);

            bool moving = pointerEvent.IsPointerMoving();

            if (moving && pointerEvent.pointerDrag != null
                && !pointerEvent.dragging
                &&
                shouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold,
                    pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        /// <summary>
        /// Sends ended event for injected pointer.
        /// </summary>
        /// <param name="pointerEvent"> The pointer data. </param>
        protected void endPointer(PointerEventData pointerEvent)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            // see if we mouse up on the same element that we clicked on...
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            // send exit events as we need to simulate this on touch up on touch device
            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
            pointerEvent.pointerEnter = null;

            removePointerData(pointerEvent);
        }

        /// <summary>
        /// Gets pointer data for a touch.
        /// </summary>
        /// <param name="id"> Touch id. </param>
        /// <param name="data"> Pointer data. </param>
        /// <param name="create"> If set to <c>true</c> not found pointer data is created. </param>
        /// <returns><c>true</c> if pointer data is found or created; <c>false</c> otherwise.</returns>
        protected bool getPointerData(int id, out PointerEventData data, bool create)
        {
            if (!pointerEvents.TryGetValue(id, out data) && create)
            {
                data = new PointerEventData(eventSystem)
                {
                    pointerId = id,
                };
                pointerEvents.Add(id, data);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes pointer data.
        /// </summary>
        /// <param name="data"> The data. </param>
        protected void removePointerData(PointerEventData data)
        {
            pointerEvents.Remove(data.pointerId);
        }

        /// <summary>
        /// Gets the last pointer event data.
        /// </summary>
        /// <param name="id"> Touch id. </param>
        /// <returns> Pointer data. </returns>
        protected PointerEventData getLastPointerEventData(int id)
        {
            PointerEventData data;
            getPointerData(id, out data, false);
            return data;
        }

        /// <summary>
        /// Clears UI selection.
        /// </summary>
        protected void clearSelection()
        {
            var baseEventData = GetBaseEventData();

            foreach (var pointer in pointerEvents.Values)
            {
                // clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }

            pointerEvents.Clear();
            eventSystem.SetSelectedGameObject(null, baseEventData);
        }

        /// <summary>
        /// Deselects if selection changed.
        /// </summary>
        /// <param name="currentOverGo"> GameObject which has the touch over it. </param>
        /// <param name="pointerEvent"> Pointer data for the touch. </param>
        protected void deselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
        {
            // Selection tracking
            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
            // if we have clicked something new, deselect the old thing
            // leave 'selection handling' up to the press event though.
            if (selectHandlerGO != eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null, pointerEvent);
        }

        #endregion

        #region Private functions

        private void processBegan(TouchPoint touch)
        {
            PointerEventData pointerEvent = initPointerData(touch);
            raycastPointer(pointerEvent);
            injectPointer(pointerEvent);
        }

        private void processMove(TouchPoint touch)
        {
            PointerEventData pointerEvent = updatePointerData(touch);
            raycastPointer(pointerEvent);
            movePointer(pointerEvent);
        }

        private void processEnded(TouchPoint touch)
        {
            PointerEventData pointerEvent = updatePointerData(touch);
            raycastPointer(pointerEvent);
            endPointer(pointerEvent);
        }

        private bool allowMoveEventProcessing(float time)
        {
            bool allow = Input.GetButtonDown(horizontalAxis);
            allow |= Input.GetButtonDown(verticalAxis);
            allow |= (time > nextActionTime);
            return allow;
        }

        private static bool shouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        private bool sendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            if (Input.GetButtonDown(submitButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

            if (Input.GetButtonDown(cancelButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            return data.used;
        }

        private Vector2 getRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = Input.GetAxisRaw(horizontalAxis);
            move.y = Input.GetAxisRaw(verticalAxis);

            if (Input.GetButtonDown(horizontalAxis))
            {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            if (Input.GetButtonDown(verticalAxis))
            {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            return move;
        }

        private bool sendMoveEventToSelectedObject()
        {
            float time = Time.unscaledTime;

            if (!allowMoveEventProcessing(time))
                return false;

            Vector2 movement = getRawMoveVector();
            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
            MoveDirection moveDir = axisEventData.moveDir;

            // Repeat delay
            if (moveDir != lastMoveDirection)
            {
                lastMoveDirection = moveDir;
                lastMoveStartTime = time;
            }
            else
            {
                if (time < lastMoveStartTime + repeatDelay)
                    return false;
            }

            if (moveDir != MoveDirection.None)
            {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            }
            nextActionTime = time + 1f / inputActionsPerSecond;
            return axisEventData.used;
        }

        private bool sendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        #endregion

        #region Touch event callbacks

        private void touchBeganHandler(object sender, TouchEventArgs touchEventArgs)
        {
            processBegan(touchEventArgs.Touch);
        }

        private void touchMovedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            processMove(touchEventArgs.Touch);
        }

        private void touchEndedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            processEnded(touchEventArgs.Touch);
        }

        private void touchCancelledHandler(object sender, TouchEventArgs touchEventArgs)
        {
            processEnded(touchEventArgs.Touch);
        }

        #endregion
    }
}