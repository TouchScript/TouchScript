/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Behaviors;
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

        /// <summary>
        /// Determines layer behavior.
        /// </summary>
        public enum UILayerMode
        {
            /// <summary>
            /// Works as a touch layer using UI EventSystem to check if touch points hit any UI elements.
            /// </summary>
            Layer,

            /// <summary>
            /// Works as a UI input module redirecting touch points to UI EventSystem.
            /// </summary>
            Proxy
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

        /// <summary>
        /// Z offset used to cast a ray from a screen space canvas.
        /// </summary>
        public float ScreenSpaceZOffset
        {
            get { return screenSpaceZOffset; }
            set { screenSpaceZOffset = value; }
        }

        #endregion

        #region Private variables

        private static UILayer instance;

        [SerializeField]
        private UILayerMode mode = UILayerMode.Layer;

        [SerializeField]
        private float screenSpaceZOffset = 1000;

        [NonSerialized]
        private List<RaycastResult> raycastResultCache = new List<RaycastResult>();

        private PointerEventData pointerDataCache;
        private EventSystem eventSystem;
        private InputModuleStub inputModule;

        #endregion

        #region Public methods

        public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            // have to duplicate some code since Gesture depends on Layer.Hit but in beginTouch we need full touch info
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;
            if (eventSystem == null) return LayerHitResult.Error;
            var raycast = this.raycast(position);
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
            }

            return LayerHitResult.Hit;
        }

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
        }

        protected override IEnumerator lateAwake()
        {
            yield return base.lateAwake();
            activateMode();
        }

        protected override void OnDestroy()
        {
            deactivateMode();
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
            var raycast = this.raycast(touch.Position);
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
                var pointerEvent = inputModule.InitPointerData(touch);
                pointerEvent.pointerCurrentRaycast = raycast;
                inputModule.InjectPointer(pointerEvent);
            }

            return LayerHitResult.Hit;
        }

        protected override void updateTouch(ITouch touch)
        {
            if (mode != UILayerMode.Proxy) return;

            PointerEventData pointerEvent = inputModule.UpdatePointerData(touch);
            inputModule.RaycastPointer(pointerEvent);
            inputModule.MovePointer(pointerEvent);
        }

        protected override void endTouch(ITouch touch)
        {
            if (mode != UILayerMode.Proxy) return;

            PointerEventData pointerEvent = inputModule.UpdatePointerData(touch);
            inputModule.RaycastPointer(pointerEvent);
            inputModule.EndPointer(pointerEvent);
        }

        protected override void cancelTouch(ITouch touch)
        {
            if (mode != UILayerMode.Proxy) return;

            endTouch(touch);
        }

        #endregion

        #region Private functions

        private RaycastResult raycast(Vector2 position)
        {
            if (pointerDataCache == null) pointerDataCache = new PointerEventData(eventSystem);
            pointerDataCache.position = position;
            eventSystem.RaycastAll(pointerDataCache, raycastResultCache);
            var raycast = findFirstRaycast(raycastResultCache);
            raycastResultCache.Clear();

            return raycast;
        }

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
            if (!Application.isPlaying) return;

            eventSystem = EventSystem.current;
            if (eventSystem == null) eventSystem = gameObject.AddComponent<EventSystem>();

            switch (mode)
            {
                case UILayerMode.Proxy:
                    inputModule = eventSystem.gameObject.AddComponent<InputModuleStub>();
                    inputModule.hideFlags = HideFlags.HideInInspector;
                    break;
                case UILayerMode.Layer:
                    break;
            }
        }

        private void deactivateMode()
        {
            if (!Application.isPlaying) return;

            switch (mode)
            {
                case UILayerMode.Proxy:
                    if (inputModule)
                    {
                        Destroy(inputModule);
                        inputModule = null;
                    }
                    break;
                case UILayerMode.Layer:
                    break;
            }
        }

        #endregion

        private class InputModuleStub : TouchScriptInputModule
        {
            public new static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
            {
                return BaseInputModule.FindFirstRaycast(candidates);
            }

            public void RaycastPointer(PointerEventData pointerEvent)
            {
                raycastPointer(pointerEvent);
            }

            public PointerEventData InitPointerData(ITouch touch)
            {
                return initPointerData(touch);
            }

            public void InjectPointer(PointerEventData pointerEvent)
            {
                injectPointer(pointerEvent);
            }

            public PointerEventData UpdatePointerData(ITouch touch)
            {
                return updatePointerData(touch);
            }

            public void MovePointer(PointerEventData pointerEvent)
            {
                movePointer(pointerEvent);
            }

            public void EndPointer(PointerEventData pointerEvent)
            {
                endPointer(pointerEvent);
            }

            public override void Process() {}
            public override void ActivateModule() {}
            public override void DeactivateModule() {}
        }
    }
}