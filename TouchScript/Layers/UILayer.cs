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

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            hit = default(TouchHit);
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return LayerHitResult.Error;

            if (pointerDataCache == null) pointerDataCache = new PointerEventData(eventSystem);
            pointerDataCache.position = position;
            eventSystem.RaycastAll(pointerDataCache, raycastResultCache);

            var raycast = findFirstRaycast(raycastResultCache);
            raycastResultCache.Clear();
            if (raycast.gameObject == null) return LayerHitResult.Miss;

            if (!(raycast.module is GraphicRaycaster))
            {
                if (Application.isEditor)
                    Debug.LogWarning("UILayer in Layer mode doesn't support raycasters other than GraphicRaycaster. Please use CameraLayer or CameraLayer2D to hit 3d objects.");
                return LayerHitResult.Miss;
            }

            hit = new TouchHit(raycast);
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