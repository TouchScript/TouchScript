/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers.UI;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Layers.Base
{
    public class UILayerBase : TouchLayer
    {
        #region Private variables

		private List<RaycastHitUI> graphicList = new List<RaycastHitUI>(20);
		private Comparison<RaycastHitUI> _raycastComparerFunc;

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
                if (filterCanvas(canvas)) continue;

                var eventCamera = canvas.worldCamera;
                var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
                var count2 = foundGraphics.Count;
                for (int j = 0; j < count2; j++)
                {
                    Graphic graphic = foundGraphics[j];

                    // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                    if (graphic.depth == -1 || !graphic.raycastTarget)
                        continue;

                    if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, position, eventCamera))
                        continue;

                    if (graphic.Raycast(position, null))
                    {
                        var appendGraphic = true;
                        if (raycaster.ignoreReversedGraphics)
                        {
                            if (eventCamera == null)
                            {
                                // If we dont have a camera we know that we should always be facing forward
                                var dir = graphic.transform.rotation * Vector3.forward;
                                appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                            }
                            else
                            {
                                // If we have a camera compare the direction against the cameras forward.
                                var cameraFoward = eventCamera.transform.rotation * Vector3.forward;
                                var dir = graphic.transform.rotation * Vector3.forward;
                                appendGraphic = Vector3.Dot(cameraFoward, dir) > 0;
                            }
                        }
                        if (appendGraphic)
                            graphicList.Add(
								new RaycastHitUI()
                                {
                                    GameObject = graphic.gameObject,
                                    Raycaster = raycaster,
									Graphic = graphic,
//                                    distance = 0,
//                                    screenPosition = position,
                                    GraphicIndex = graphicList.Count,
                                    Depth = graphic.depth,
                                    SortingLayer = canvas.sortingLayerID,
                                    SortingOrder = canvas.sortingOrder
                                });
                    }
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

        protected virtual bool filterCanvas(Canvas canvas)
        {
            return true;
        }

        #endregion

        #region Private functions

		private HitResult doHit(IPointer pointer, RaycastHitUI raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this, true);
            return checkHitFilters(pointer, hit);
        }

		private static int raycastComparerFunc(RaycastHitUI lhs, RaycastHitUI rhs)
        {
            if (lhs.Raycaster != rhs.Raycaster)
            {
                if (lhs.Raycaster.sortOrderPriority != rhs.Raycaster.sortOrderPriority)
                    return rhs.Raycaster.sortOrderPriority.CompareTo(lhs.Raycaster.sortOrderPriority);

                if (lhs.Raycaster.renderOrderPriority != rhs.Raycaster.renderOrderPriority)
                    return rhs.Raycaster.renderOrderPriority.CompareTo(lhs.Raycaster.renderOrderPriority);
            }

            if (lhs.SortingLayer != rhs.SortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.SortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.SortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.SortingOrder != rhs.SortingOrder)
                return rhs.SortingOrder.CompareTo(lhs.SortingOrder);

            if (lhs.Depth != rhs.Depth)
                return rhs.Depth.CompareTo(lhs.Depth);

            return lhs.GraphicIndex.CompareTo(rhs.GraphicIndex);
        }

        #endregion
    }
}