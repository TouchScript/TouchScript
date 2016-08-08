/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers.UI;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/UI Overlay Layer")]
    public class UIOverlayLayer : TouchLayer
    {

        #region Private variables

        private List<RaycastResult> graphicList = new List<RaycastResult>(20);
        private Comparison<RaycastResult> _raycastComparerFunc;

        #endregion

        #region Public methods

        public override HitResult Hit(IPointer pointer, out HitData hit)
        {
            if (base.Hit(pointer, out hit) != HitResult.Hit) return HitResult.Miss;

            graphicList.Clear();

            hit = default(HitData);
            var position = pointer.Position;
            var raycasters = TouchScriptInputModule.Instance.GetRaycasters();
            var count = raycasters.Count;

            for (var i = 0; i < count; i++)
            {
                var raycaster = raycasters[i] as GraphicRaycaster;
                if (raycaster == null) continue;
                var canvas = TouchScriptInputModule.Instance.GetCanvasForRaycaster(raycaster);
                if (canvas == null) continue;
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            
                var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
                var count2 = foundGraphics.Count;
                for (int j = 0; j < count2; j++)
                {
                    Graphic graphic = foundGraphics[j];
            
                    // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                    if (graphic.depth == -1 || !graphic.raycastTarget)
                        continue;
            
                    if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, position))
                        continue;
            
                    if (graphic.Raycast(position, null)) graphicList.Add(
                        new RaycastResult()
                        {
                            gameObject = graphic.gameObject,
                            module = raycaster,
                            distance = 0,
                            screenPosition = position,
                            index = graphicList.Count,
                            depth = graphic.depth,
                            sortingLayer = canvas.sortingLayerID,
                            sortingOrder = canvas.sortingOrder
                        });
                }
            }

            count = graphicList.Count;
            if (count == 0) return HitResult.Miss;
            if (count > 1)
            {
                graphicList.Sort(_raycastComparerFunc);
                for (var i = 0; i < count; ++i)
                {
                    var result = doHit(pointer, graphicList[i], out hit);
                    if (result != HitResult.Miss) return result;
                }
                return HitResult.Miss;
            }
            return doHit(pointer, graphicList[0], out hit);
        }

        #endregion

        #region Unity methods

        protected override void Awake()
        {
            base.Awake();
            _raycastComparerFunc = raycastComparerFunc;
        }

        protected void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (TouchScriptInputModule.Instance != null) TouchScriptInputModule.Instance.INTERNAL_Retain();
        }

        protected void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (TouchScriptInputModule.Instance != null) TouchScriptInputModule.Instance.INTERNAL_Release();
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            Name = "UI Overlay Layer";
        }

        #endregion

        #region Private functions

        private HitResult doHit(IPointer pointer, RaycastResult raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this, true);
            return checkHitFilters(pointer, hit);
        }

        private static int raycastComparerFunc(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            if (lhs.depth != rhs.depth)
                return rhs.depth.CompareTo(lhs.depth);

            return lhs.index.CompareTo(rhs.index);
        }

        #endregion

    }
}
