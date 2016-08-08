/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pointer = TouchScript.Pointers.Pointer;

namespace TouchScript.Layers.UI
{
    internal sealed class TouchScriptInputModule : BaseInputModule
    {

        #region Public properties

        public static TouchScriptInputModule Instance
        {
            get
            {
                if (instance == null)
                {
                    if (EventSystem.current != null)
                    {
                        instance = EventSystem.current.GetComponent<TouchScriptInputModule>();
                        if (instance == null) instance = EventSystem.current.gameObject.AddComponent<TouchScriptInputModule>();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Private variables

        private static TouchScriptInputModule instance;
        private static FieldInfo raycastersProp;
        private static PropertyInfo canvasProp;
        private static Dictionary<int, Canvas> raycasterCanvasCache = new Dictionary<int, Canvas>();
        private static int refCount = 0;

        private UIPointerInputModule ui;

        #endregion

        #region Constructor

        private TouchScriptInputModule()
        {
            if (raycastersProp == null)
            {
                raycastersProp = Type.GetType(Assembly.CreateQualifiedName("UnityEngine.UI", "UnityEngine.EventSystems.RaycasterManager")).
                                      GetField("s_Raycasters", BindingFlags.NonPublic | BindingFlags.Static);
                canvasProp = typeof (GraphicRaycaster).GetProperty("canvas", BindingFlags.NonPublic | BindingFlags.Instance);
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
                if (eventSystem != EventSystem.current)
                {
                    Destroy(this);
                    instance = null;
                    return;
                }
            }

            ui = new UIStandardInputModule(this, eventSystem);

            TouchManager.Instance.PointersUpdated += pointerUpdatedHandler;
        }

        protected override void OnDisable()
        {
            if (TouchManager.Instance != null) TouchManager.Instance.PointersUpdated -= pointerUpdatedHandler;
            instance = null;

            base.OnDisable();
        }

        #endregion

        #region Public methods

        public List<BaseRaycaster> GetRaycasters()
        {
            return raycastersProp.GetValue(null) as List<BaseRaycaster>;
        }

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
            ui.Process();
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return base.IsPointerOverGameObject(pointerId);
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

        internal void INTERNAL_Retain()
        {
            refCount++;
        }

        internal int INTERNAL_Release()
        {
            if (--refCount <= 0) Destroy(this);
            return refCount;
        }

        #endregion

        #region Private functions

        #endregion

        #region Event handlers

        private void pointerUpdatedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            ui.ProcessUpdated(pointerEventArgs.Pointers);
        }

        #endregion

        #region Copypasted code from UI

        // last update: df1947cd (5.4f3)
        private abstract class UIPointerInputModule
        {

            protected TouchScriptInputModule input;
            protected EventSystem eventSystem;

            protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();

            public UIPointerInputModule(TouchScriptInputModule input, EventSystem eventSystem)
            {
                this.eventSystem = eventSystem;
                this.input = input;
            }

            #region Unchanged

            protected bool GetPointerData(int id, out PointerEventData data, bool create)
            {
                if (!m_PointerData.TryGetValue(id, out data) && create)
                {
                    data = new PointerEventData(eventSystem)
                    {
                        pointerId = id,
                    };
                    m_PointerData.Add(id, data);
                    return true;
                }
                return false;
            }

            #endregion

            #region Changed

            #endregion

            public abstract void Process();

            public virtual void ProcessUpdated(IList<Pointer> pointers)
            {
                var count = pointers.Count;
                for (var i = 0; i < count; i++)
                {
                    var pointer = pointers[i];
                    PointerEventData data;
                    GetPointerData(pointer.Id, out data, true);

                    data.position = pointer.Position;
                    data.delta = pointer.Position - pointer.PreviousPosition;

                    var target = pointer.GetOverData().Target;
                    input.HandlePointerExitAndEnter(data, target == null ? null : target.gameObject);
                }
            }

        }

        private abstract class UITouchInputModule : UIPointerInputModule
        {

            public UITouchInputModule(TouchScriptInputModule input, EventSystem eventSystem) : base(input, eventSystem)
            { }

            public override void Process()
            {}
        }

        private sealed class UIStandardInputModule : UIPointerInputModule
        {

            public UIStandardInputModule(TouchScriptInputModule input, EventSystem eventSystem) : base(input, eventSystem)
            { }

            #region Unchanged 

            private string m_HorizontalAxis = "Horizontal";
            private string m_VerticalAxis = "Vertical";
            private string m_SubmitButton = "Submit";
            private string m_CancelButton = "Cancel";
            private float m_InputActionsPerSecond = 10;
            private float m_RepeatDelay = 0.5f;

            private int m_ConsecutiveMoveCount = 0;
            private Vector2 m_LastMoveVector;
            private float m_PrevActionTime;

            public override void Process()
            {
                bool usedEvent = SendUpdateEventToSelectedObject();

                if (eventSystem.sendNavigationEvents)
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

            private bool SendUpdateEventToSelectedObject()
            {
                if (eventSystem.currentSelectedGameObject == null)
                    return false;

                var data = input.GetBaseEventData();
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
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
                bool allow = Input.GetButtonDown(m_HorizontalAxis) || Input.GetButtonDown(m_VerticalAxis);
                bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);
                if (!allow)
                {
                    // Otherwise, user held down key or axis.
                    // If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
                    if (similarDir && m_ConsecutiveMoveCount == 1)
                        allow = (time > m_PrevActionTime + m_RepeatDelay);
                    // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
                    else
                        allow = (time > m_PrevActionTime + 1f / m_InputActionsPerSecond);
                }
                if (!allow)
                    return false;

                // Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
                var axisEventData = input.GetAxisEventData(movement.x, movement.y, 0.6f);

                if (axisEventData.moveDir != MoveDirection.None)
                {
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
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
                if (eventSystem.currentSelectedGameObject == null)
                    return false;

                var data = input.GetBaseEventData();
                if (Input.GetButtonDown(m_SubmitButton))
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

                if (Input.GetButtonDown(m_CancelButton))
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
                return data.used;
            }

            private Vector2 GetRawMoveVector()
            {
                Vector2 move = Vector2.zero;
                move.x = Input.GetAxisRaw(m_HorizontalAxis);
                move.y = Input.GetAxisRaw(m_VerticalAxis);

                if (Input.GetButtonDown(m_HorizontalAxis))
                {
                    if (move.x < 0)
                        move.x = -1f;
                    if (move.x > 0)
                        move.x = 1f;
                }
                if (Input.GetButtonDown(m_VerticalAxis))
                {
                    if (move.y < 0)
                        move.y = -1f;
                    if (move.y > 0)
                        move.y = 1f;
                }
                return move;
            }

            #endregion

        }

        #endregion

    }
}