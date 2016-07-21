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
    /// Touch layer which handles Unity UI and interface objects in a Canvas.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/UI Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_UILayer.htm")]
    public class UILayer : TouchLayer
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the layer mask which is used to select layers which should be touchable from this layer.
        /// </summary>
        /// <value>A mask to include/exclude objects from possibly touchable list.</value>
        public LayerMask LayerMask
        {
          get { return layerMask; }
          set { layerMask = value; }
        }

        #endregion

        #region Private variables

        private static UILayer instance;

        [SerializeField]
        private LayerMask layerMask = -1;

        [NonSerialized]
        private List<RaycastResult> raycastResultCache = new List<RaycastResult>(20);

        private List<HitTest> tmpHitTestList = new List<HitTest>(10);

        private PointerEventData pointerDataCache;
        private EventSystem eventSystem;
        private Dictionary<Canvas, ProjectionParams> projectionParamsCache = new Dictionary<Canvas, ProjectionParams>();

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;
            if (eventSystem == null) return LayerHitResult.Error;

            if (pointerDataCache == null) pointerDataCache = new PointerEventData(eventSystem);
            pointerDataCache.position = position;
            eventSystem.RaycastAll(pointerDataCache, raycastResultCache);

            var count = raycastResultCache.Count;
            if (count == 0) return LayerHitResult.Miss;
            if (count > 1)
            {
                for (var i = 0; i < count; ++i)
                {
                    var raycastHit = raycastResultCache[i];
                    switch (doHit(raycastHit, out hit))
                    {
                        case HitTest.ObjectHitResult.Hit:
                            return LayerHitResult.Hit;
                        case HitTest.ObjectHitResult.Discard:
                            return LayerHitResult.Miss;
                    }
                }
            }
            else
            {
                switch (doHit(raycastResultCache[0], out hit))
                {
                    case HitTest.ObjectHitResult.Hit:
                        return LayerHitResult.Hit;
                    case HitTest.ObjectHitResult.Error:
                        return LayerHitResult.Error;
                    default:
                        return LayerHitResult.Miss;
                }
            }

            return LayerHitResult.Miss;
        }

        /// <inheritdoc />
        public override ProjectionParams GetProjectionParams(TouchPoint touch)
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

        /// <inheritdoc />
        protected override void Awake()
        {
            if (Application.isPlaying)
            {
                if (instance == null) instance = this;
                if (instance != this)
                {
                    Debug.LogWarning("[TouchScript] Only one instance of UILayer should exist in a scene. Destroying.");
                    Destroy(this);
                    return;
                }
            }

            base.Awake();
            if (!Application.isPlaying) return;

            StartCoroutine(lateAwake());
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void setName()
        {
            Name = "UI Layer";
        }

        #endregion

        #region Private functions

        private HitTest.ObjectHitResult doHit(RaycastResult raycastHit, out TouchHit hit)
        {
            hit = new TouchHit(raycastHit);

            if (!(raycastHit.module is GraphicRaycaster)) return HitTest.ObjectHitResult.Miss;
            var go = raycastHit.gameObject;
            if (go == null) return HitTest.ObjectHitResult.Miss;

            if (((1 << go.layer) & LayerMask) == 0) return HitTest.ObjectHitResult.Miss;

            go.GetComponents(tmpHitTestList);
            var count = tmpHitTestList.Count;
            if (count == 0) return HitTest.ObjectHitResult.Hit;

            var hitResult = HitTest.ObjectHitResult.Hit;
            for (var i = 0; i < count; i++)
            {
                var test = tmpHitTestList[i];
                if (!test.enabled) continue;
                hitResult = test.IsHit(hit);
                if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) break;
            }
            return hitResult;
        }

        #endregion
    }
}
