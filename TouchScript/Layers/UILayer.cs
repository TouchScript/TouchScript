/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Layers
{
    /// <summary>
    /// 
    /// </summary>
    public class UILayer : TouchLayer
    {
        #region Constants

        public enum UILayerMode
        {
            InputModule,
            Layer
        }

        #endregion

        #region Public properties

        public UILayerMode Mode
        {
            get { return mode; }
            set
            {
                if (value == mode) return;
                deactivateMode();
                mode = value;
                activateMode();
            }
        }

        public float ScreenSpaceZOffset
        {
            get { return screenSpaceZOffset; }
            set { screenSpaceZOffset = value; }
        }

        #endregion

        #region Private variables

        private static UILayer instance;

        private UILayerMode mode = UILayerMode.Layer;
        private float screenSpaceZOffset = 1000;

        [NonSerialized]
        private List<RaycastResult> raycastResultCache = new List<RaycastResult>();
        private PointerEventData pointerDataCache;
        private EventSystem eventSystem;

        protected Dictionary<int, PointerEventData> pointerEvents = new Dictionary<int, PointerEventData>();

        #endregion

        #region Public methods

        public override ProjectionParams GetProjectionParams(ITouch touch)
        {
            var graphic = touch.Target.GetComponent<Graphic>();
            if (graphic == null) return INVALID_PROJECTION_PARAMS;
            var canvas = graphic.canvas;
            if (canvas == null) return INVALID_PROJECTION_PARAMS;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                var canvasRect = canvas.GetComponent<RectTransform>();
                return new ProjectionParams((screenPosition) =>
                    new Ray(new Vector3(screenPosition.x, screenPosition.y, canvasRect.position.z - ScreenSpaceZOffset), Vector3.forward));
            }
            var c = canvas.worldCamera ?? Camera.main;
            return new ProjectionParams((screenPosition) => c.ScreenPointToRay(screenPosition));
        }

        #endregion

        #region Unity methods

        protected override void Awake()
        {
            if (instance == null) instance = this;
            if (instance != this)
            {
                Debug.LogError("Only one instance ot UILayer should exist in a scene.");
                Destroy(this);
                return;
            }

            base.Awake();
            if (Application.isPlaying) activateMode();
        }

        protected override void OnDestroy()
        {
            if (Application.isPlaying) deactivateMode();
            base.OnDestroy();
        }

        #endregion

        #region Protected functions

        protected override void setName()
        {
            Name = "UI Layer";
        }

        protected override LayerHitResult beginTouch(ITouch touch, out TouchHit hit)
        {
            hit = default(TouchHit);
            if (enabled == false || gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            if (eventSystem == null) return LayerHitResult.Error;

            if (pointerDataCache == null) pointerDataCache = new PointerEventData(eventSystem);
            pointerDataCache.position = touch.Position;
            eventSystem.RaycastAll(pointerDataCache, raycastResultCache);

            var raycast = findFirstRaycast(raycastResultCache);
            raycastResultCache.Clear();
            if (raycast.gameObject == null) return LayerHitResult.Miss;

            if (mode == UILayerMode.Layer)
            {
                if (!(raycast.module is GraphicRaycaster))
                {
                    if (Application.isEditor)
                        Debug.LogWarning("UILayer in Layer mode doesn't support raycasters other than GraphicRaycaster. Please use CameraLayer or CameraLayer2D to hit 3d objects.");
                    return LayerHitResult.Miss;
                }

                hit = new TouchHit(raycast);
            }
            else
            {
                // don't init hit, no target --> layer consumes touch

                PointerEventData pointerEvent;
                getPointerData(touch.Id, out pointerEvent, true);

                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast = raycast;
                pointerEvent.position = touch.Position;
                pointerEvent.button = PointerEventData.InputButton.Left;
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;

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
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

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

            return LayerHitResult.Hit;
        }

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

        private static RaycastResult findFirstRaycast(List<RaycastResult> candidates)
        {
            for (var i = 0; i < candidates.Count; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }
            return new RaycastResult();
        }

        private void activateMode()
        {
            eventSystem = EventSystem.current;
            switch (mode)
            {
                case UILayerMode.InputModule:
                    throw new NotImplementedException();
                    break;
                case UILayerMode.Layer:
                    break;
            }
        }

        private void deactivateMode()
        {
            switch (mode)
            {
                case UILayerMode.InputModule:
                    throw new NotImplementedException();
                    break;
                case UILayerMode.Layer:
                    break;
            }
        }

        #endregion
    }
}