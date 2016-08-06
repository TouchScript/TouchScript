/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Layers
{
    /// <summary>
    /// Pointer layer which handles Unity UI and interface objects in a Canvas.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/UI Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_UILayer.htm")]
    public class UILayer : TouchLayer
    {
        #region Public properties

        #endregion

        #region Private variables

        private static UILayer instance;

        [NonSerialized]
        private List<RaycastResult> raycastResultCache = new List<RaycastResult>(20);

        private PointerEventData pointerDataCache;
        private EventSystem eventSystem;
        private Dictionary<Canvas, ProjectionParams> projectionParamsCache = new Dictionary<Canvas, ProjectionParams>();

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override HitResult Hit(IPointer pointer, out HitData hit)
        {
            if (base.Hit(pointer, out hit) != HitResult.Hit) return HitResult.Miss;
            if (eventSystem == null) return HitResult.Miss;

            var result = castRay(pointer, out hit);
            if (result != HitResult.Hit) hit = default(HitData);
            return result;
        }

        /// <inheritdoc />
        public override ProjectionParams GetProjectionParams(Pointer pointer)
        {
            var graphic = pointer.GetPressData().Target.GetComponent<Graphic>();
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

        protected HitResult castRay(IPointer pointer, out HitData hit)
        {
            hit = default(HitData);
            if (pointerDataCache == null) pointerDataCache = new PointerEventData(eventSystem);
            pointerDataCache.position = pointer.Position;
            eventSystem.RaycastAll(pointerDataCache, raycastResultCache);

            var count = raycastResultCache.Count;
            if (count == 0) return HitResult.Miss;
            if (count > 1)
            {
                for (var i = 0; i < count; ++i)
                {
                    var raycastHit = raycastResultCache[i];
                    if (!(raycastHit.module is GraphicRaycaster)) continue;
                    var result = doHit(pointer, raycastHit, out hit);
                    if (result != HitResult.Miss) return result;
                }
                return HitResult.Miss;
            }

            if (!(raycastResultCache[0].module is GraphicRaycaster)) return HitResult.Miss;
            return doHit(pointer, raycastResultCache[0], out hit);
        }

        #endregion

        #region Private functions

        private HitResult doHit(IPointer pointer, RaycastResult raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this);
            return checkHitFilters(pointer, hit);
        }

        #endregion
    }
}