/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
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
        #region Public properties

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
        private float screenSpaceZOffset = 1000;

        [NonSerialized]
        private List<RaycastResult> raycastResultCache = new List<RaycastResult>();

        private PointerEventData pointerDataCache;
        private EventSystem eventSystem;
        private Dictionary<Canvas, ProjectionParams> projectionParamsCache = new Dictionary<Canvas, ProjectionParams>(); 

        #endregion

        #region Public methods

        public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            // have to duplicate some code since Gesture depends on Layer.Hit but in beginTouch we need full touch info
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;
            if (eventSystem == null) return LayerHitResult.Error;
            var raycast = this.raycast(position);
            if (raycast.gameObject == null) return LayerHitResult.Miss;

            if (!(raycast.module is GraphicRaycaster))
            {
                if (Application.isEditor)
                    Debug.LogWarning("UILayer in doesn't support raycasters other than GraphicRaycaster. Please use CameraLayer to hit 3d or CameraLayer2D to hit 2d objects.");
                return LayerHitResult.Miss;
            }

            hit = new TouchHit(raycast);

            return LayerHitResult.Hit;
        }

        public override ProjectionParams GetProjectionParams(ITouch touch)
        {
            var graphic = touch.Target.GetComponent<Graphic>();
            if (graphic == null) return layerProjectionParams;
            var canvas = graphic.canvas;
            if (canvas == null) return layerProjectionParams;

            ProjectionParams pp;
            if (!projectionParamsCache.TryGetValue(canvas, out pp))
            {
                // TODO: memory leak
                pp = new CanvasProjectionParams(canvas);
                projectionParamsCache.Add(canvas, pp);
            }
            return pp;
        }

        #endregion

        #region Unity methods

        protected override void Awake()
        {
            if (Application.isPlaying)
            {
                if (instance == null) instance = this;
                if (instance != this)
                {
                    Debug.LogError("Only one instance ot UILayer should exist in a scene.");
                    Destroy(this);
                    return;
                }
            }

            base.Awake();
            if (!Application.isPlaying) return;

            StartCoroutine(lateAwake());
        }

        protected IEnumerator lateAwake()
        {
            yield return new WaitForEndOfFrame();
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = gameObject.AddComponent<EventSystem>();
                eventSystem.hideFlags = HideFlags.DontSave;
            }
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

            if (!(raycast.module is GraphicRaycaster))
            {
                if (Application.isEditor)
                    Debug.LogWarning("UILayer in Layer mode doesn't support raycasters other than GraphicRaycaster. Please use CameraLayer or CameraLayer2D to hit 3d objects.");
                return LayerHitResult.Miss;
            }

            hit = new TouchHit(raycast);

            return LayerHitResult.Hit;
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

        #endregion

    }
}