/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Hit;
using UnityEngine;
using System.Collections.Generic;
using TouchScript.Layers.UI;
using TouchScript.Pointers;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Layers
{

    [AddComponentMenu("TouchScript/Layers/Standard Layer")]
    public class StandardLayer : TouchLayer
    {
        #region Public properties

        public bool LookFor3DObjects
        {
            get { return lookFor3DObjects; }
            set { lookFor3DObjects = value; }
        }

        public bool LookFor2DObjects
        {
            get { return lookFor2DObjects; }
            set { lookFor2DObjects = value; }
        }

        public bool LookForWorldSpaceUI
        {
            get { return lookForWorldSpaceUI; }
            set { lookForWorldSpaceUI = value; }
        }

        public bool LookForScreenSpaceUI
        {
            get { return lookForScreenSpaceUI; }
            set { lookForScreenSpaceUI = value; }
        }

        /// <summary>
        /// Gets or sets the layer mask which is used to select layers which should be touchable from this layer.
        /// </summary>
        /// <value>A mask to exclude objects from possibly touchable list.</value>
        public LayerMask LayerMask
        {
            get { return layerMask; }
            set { layerMask = value; }
        }

        /// <inheritdoc />
        public override Vector3 WorldProjectionNormal
        {
            get
            {
                if (_camera == null) return Vector3.forward;
                return _camera.transform.forward;
            }
        }

        #endregion

        #region Private variables

        private static Comparison<RaycastHitUI> _raycastHitUIComparerFunc = raycastHitUIComparerFunc;
        private static Comparison<RaycastHit> _raycastHitComparerFunc = raycastHitComparerFunc;
        private static Comparison<HitData> _hitDataComparerFunc = hitDataComparerFunc;

        private static Dictionary<int, ProjectionParams> projectionParamsCache = new Dictionary<int, ProjectionParams>();
        private static List<BaseRaycaster> raycasters; 

        private static List<RaycastHitUI> raycastHitUIList = new List<RaycastHitUI>(20);
        private static List<RaycastHit> raycastHitList = new List<RaycastHit>(20);
        private static List<HitData> hitList = new List<HitData>(20);
#if UNITY_5_3_OR_NEWER
        private static RaycastHit[] raycastHits = new RaycastHit[20];
#endif
        private static RaycastHit2D[] raycastHits2D = new RaycastHit2D[20];

        [SerializeField]
        private bool lookFor3DObjects = true;

        [SerializeField]
        private bool lookFor2DObjects = false;

        [SerializeField]
        private bool lookForWorldSpaceUI = true;

        [SerializeField]
        private bool lookForScreenSpaceUI = true;

        [SerializeField]
        private LayerMask layerMask = -1;

        /// <summary>
        /// Camera.
        /// </summary>
        protected Camera _camera;

        #endregion

        #region Public methods

        public override HitResult Hit(IPointer pointer, out HitData hit)
        {
            if (base.Hit(pointer, out hit) != HitResult.Hit) return HitResult.Miss;

            HitResult result = HitResult.Miss;

            if (lookForScreenSpaceUI)
            {
                result = performSSUISearch(pointer, out hit);
                switch (result)
                {
                    case HitResult.Hit:
                        return result;
                    case HitResult.Discard:
                        hit = default(HitData);
                        return result;
                }
            }

            if (_camera != null && (lookFor3DObjects || lookFor2DObjects || lookForWorldSpaceUI))
            {
                result = performWorldSearch(pointer, out hit);
                switch (result)
                {
                    case HitResult.Hit:
                        return result;
                    case HitResult.Discard:
                        hit = default(HitData);
                        return result;
                }
            }

            hit = default(HitData);
            return HitResult.Miss;
        }

        /// <inheritdoc />
        public override ProjectionParams GetProjectionParams(Pointer pointer)
        {
            var press = pointer.GetPressData();
            if (press.Type == HitData.HitType.World2D ||
                press.Type == HitData.HitType.World3D)
                return layerProjectionParams;

            var graphic = press.RaycastHitUI.Graphic;
            if (graphic == null) return layerProjectionParams;
            var canvas = graphic.canvas;
            if (canvas == null) return layerProjectionParams;

            ProjectionParams pp;
            if (!projectionParamsCache.TryGetValue(canvas.GetInstanceID(), out pp))
            {
                // TODO: memory leak
                pp = new WorldSpaceCanvasProjectionParams(canvas);
                projectionParamsCache.Add(canvas.GetInstanceID(), pp);
            }
            return pp;
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            updateCamera();
            base.Awake();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            if (TouchScriptInputModule.Instance != null) TouchScriptInputModule.Instance.INTERNAL_Retain();
            TouchManager.Instance.FrameStarted += frameStartedHandler;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (TouchScriptInputModule.Instance != null) TouchScriptInputModule.Instance.INTERNAL_Release();
            if (TouchManager.Instance != null) TouchManager.Instance.FrameStarted -= frameStartedHandler;
        }

        #endregion

        #region Protected functions

        /// <summary>
        /// Finds a camera.
        /// </summary>
        protected virtual void updateCamera()
        {
            _camera = GetComponent<Camera>();
        }

        /// <inheritdoc />
        protected override ProjectionParams createProjectionParams()
        {
            return new CameraProjectionParams(_camera);
        }

        /// <inheritdoc />
        protected override void setName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                if (_camera != null) Name = _camera.name;
                else Name = "Layer";
            }
        }

        #endregion

        #region Private functions

        private HitResult performSSUISearch(IPointer pointer, out HitData hit)
        {
            hit = default(HitData);

            raycastHitUIList.Clear();

            if (raycasters == null) raycasters = TouchScriptInputModule.Instance.GetRaycasters();
            var count = raycasters.Count;

            for (var i = 0; i < count; i++)
            {
                var raycaster = raycasters[i] as GraphicRaycaster;
                if (raycaster == null) continue;
                var canvas = TouchScriptInputModule.Instance.GetCanvasForRaycaster(raycaster); // TODO: cache
                if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                performUISearchForCanvas(pointer, canvas, raycaster);
            }

            count = raycastHitUIList.Count;
            if (count == 0) return HitResult.Miss;
            if (count > 1)
            {
                raycastHitUIList.Sort(_raycastHitUIComparerFunc);
                for (var i = 0; i < count; ++i)
                {
                    var result = doHit(pointer, raycastHitUIList[i], out hit);
                    if (result != HitResult.Miss) return result;
                }
                return HitResult.Miss;
            }
            return doHit(pointer, raycastHitUIList[0], out hit);
        }

        private HitResult performWorldSearch(IPointer pointer, out HitData hit)
        {
            hit = default(HitData);

            if (_camera == null) return HitResult.Miss;
            if (_camera.enabled == false || _camera.gameObject.activeInHierarchy == false) return HitResult.Miss;
            var position = pointer.Position;
            if (!_camera.pixelRect.Contains(position)) return HitResult.Miss;

            hitList.Clear();
            var ray = _camera.ScreenPointToRay(position);

            var searchDistance = float.MaxValue;
            var result3D = HitResult.Miss;
            if (lookFor3DObjects)
            {
                result3D = perform3DSearch(pointer, ray, out hit);
                if (result3D != HitResult.Miss) searchDistance = hit.Distance;
            }
            if (lookFor2DObjects) perform2DSearch(ray, searchDistance);
            if (lookForWorldSpaceUI) performWSUISearch(pointer, ray, searchDistance);

            var count = hitList.Count;
            if (hitList.Count == 0) return result3D;

            if (count > 1)
            {
                hitList.Sort(_hitDataComparerFunc);
                for (var i = 0; i < count; ++i)
                {
                    hit = hitList[i];
                    var result = checkHitFilters(pointer, hit);
                    if (result != HitResult.Miss) return result;
                }
                return HitResult.Miss;
            }
            hit = hitList[0];
            return checkHitFilters(pointer, hit);
        }

        private HitResult perform3DSearch(IPointer pointer, Ray ray, out HitData hit)
        {
            hit = default(HitData);
#if UNITY_5_3_OR_NEWER
            var count = Physics.RaycastNonAlloc(ray, raycastHits, float.PositiveInfinity, layerMask);
#else
            var raycastHits = Physics.RaycastAll(ray, float.PositiveInfinity, layerMask);
            var count = raycastHits.Length;
#endif
            if (count == 0) return HitResult.Miss;
            if (count > 1)
            {
                raycastHitList.Clear();
                for (var i = 0; i < count; i++) raycastHitList.Add(raycastHits[i]);
                raycastHitList.Sort(_raycastHitComparerFunc);

                RaycastHit raycastHit = default(RaycastHit);
                for (var i = 0; i < count; i++)
                {
                    raycastHit = raycastHitList[i];
                    var result = doHit(pointer, raycastHit, out hit);
                    if (result != HitResult.Miss) return result;
                }
                return HitResult.Miss;
            }
            return doHit(pointer, raycastHits[0], out hit);
        }

        private void perform2DSearch(Ray ray, float maxDistance)
        {
            var count = Physics2D.GetRayIntersectionNonAlloc(ray, raycastHits2D, maxDistance, layerMask);
            for (var i = 0; i < count; i++)
            {
                hitList.Add(new HitData(raycastHits2D[i], this));
            }
        }

        private void performWSUISearch(IPointer pointer, Ray ray, float maxDistance)
        {
            raycastHitUIList.Clear();

            if (raycasters == null) raycasters = TouchScriptInputModule.Instance.GetRaycasters();
            var count = raycasters.Count;

            for (var i = 0; i < count; i++)
            {
                var raycaster = raycasters[i] as GraphicRaycaster;
                if (raycaster == null) continue;
                var canvas = TouchScriptInputModule.Instance.GetCanvasForRaycaster(raycaster); // TODO: cache
                if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.worldCamera != _camera) continue;
                performUISearchForCanvas(pointer, canvas, raycaster, maxDistance, ray);
            }

            count = raycastHitUIList.Count;
            for (var i = 0; i < count; i++)
            {
                hitList.Add(new HitData(raycastHitUIList[i], this));
            }
        }

        private void performUISearchForCanvas(IPointer pointer, Canvas canvas, GraphicRaycaster raycaster, float maxDistance = float.MaxValue, Ray ray = default(Ray))
        {
            var position = pointer.Position;
            var eventCamera = canvas.worldCamera;
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            var count2 = foundGraphics.Count;
            for (int j = 0; j < count2; j++)
            {
                Graphic graphic = foundGraphics[j];
                
                if (layerMask.value != -1 && (layerMask.value & 1 << graphic.gameObject.layer) == 0) continue;

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, position, eventCamera))
                    continue;

                if (graphic.Raycast(position, null))
                {
                    var t = graphic.transform;
                    if (raycaster.ignoreReversedGraphics)
                    {
                        if (eventCamera == null)
                        {
                            // If we dont have a camera we know that we should always be facing forward
                            var dir = t.rotation * Vector3.forward;
                            if (Vector3.Dot(Vector3.forward, dir) <= 0) continue;
                        }
                        else
                        {
                            // If we have a camera compare the direction against the cameras forward.
                            var cameraFoward = eventCamera.transform.rotation * Vector3.forward;
                            var dir = t.rotation * Vector3.forward;
                            if (Vector3.Dot(cameraFoward, dir) <= 0) continue;
                        }
                    }

                    float distance = 0;

                    if (eventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) {}
                    else
                    {
                        Vector3 transForward = t.forward;
                        // http://geomalgorithms.com/a06-_intersect-2.html
                        distance = (Vector3.Dot(transForward, t.position - ray.origin) / Vector3.Dot(transForward, ray.direction));

                        // Check to see if the go is behind the camera.
                        if (distance < 0) continue;
                        if (distance >= maxDistance) continue;
                    }

                    raycastHitUIList.Add(
                            new RaycastHitUI()
                            {
                                GameObject = graphic.gameObject,
                                Raycaster = raycaster,
                                Graphic = graphic,
                                GraphicIndex = raycastHitUIList.Count,
                                Depth = graphic.depth,
                                SortingLayer = canvas.sortingLayerID,
                                SortingOrder = canvas.sortingOrder,
                                Distance = distance
                            });
                }
            }
        }

        private HitResult doHit(IPointer pointer, RaycastHitUI raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this, true);
            return checkHitFilters(pointer, hit);
        }

        private HitResult doHit(IPointer pointer, RaycastHit raycastHit, out HitData hit)
        {
            hit = new HitData(raycastHit, this);
            return checkHitFilters(pointer, hit);
        }

        #endregion

        #region Compare functions

        private static int raycastHitUIComparerFunc(RaycastHitUI lhs, RaycastHitUI rhs)
        {
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

            if (!Mathf.Approximately(lhs.Distance, rhs.Distance))
                return lhs.Distance.CompareTo(rhs.Distance);

            return lhs.GraphicIndex.CompareTo(rhs.GraphicIndex);
        }

        private static int raycastHitComparerFunc(RaycastHit lhs, RaycastHit rhs)
        {
            if (lhs.collider.transform == rhs.collider.transform) return 0;
            return lhs.distance < rhs.distance ? -1 : 1;
        }

        private static int hitDataComparerFunc(HitData lhs, HitData rhs)
        {
            if (lhs.SortingLayer != rhs.SortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.SortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.SortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.SortingOrder != rhs.SortingOrder)
                return rhs.SortingOrder.CompareTo(lhs.SortingOrder);

            if (lhs.Type == HitData.HitType.UI && rhs.Type == HitData.HitType.UI && 
                lhs.RaycastHitUI.Depth != rhs.RaycastHitUI.Depth)
                return rhs.RaycastHitUI.Depth.CompareTo(lhs.RaycastHitUI.Depth);

            if (!Mathf.Approximately(lhs.Distance, rhs.Distance))
                return lhs.Distance.CompareTo(rhs.Distance);

            if (lhs.Type == HitData.HitType.UI && rhs.Type == HitData.HitType.UI &&
                lhs.RaycastHitUI.GraphicIndex != rhs.RaycastHitUI.GraphicIndex)
                return rhs.RaycastHitUI.GraphicIndex.CompareTo(lhs.RaycastHitUI.GraphicIndex);

            return 0;
        }

        #endregion

        #region Event handlers

        private void frameStartedHandler(object sender, EventArgs eventArgs)
        {
            raycasters = null;
        }

        #endregion

    }
}