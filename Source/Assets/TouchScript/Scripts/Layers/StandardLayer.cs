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
using System.Collections;
using TouchScript.Utils.Attributes;

namespace TouchScript.Layers
{
    /// <summary>
    /// A layer which combines all types of hit recognition into one: UI (Screen Space and World), 3D and 2D.
    /// </summary>
    /// <seealso cref="TouchScript.Layers.TouchLayer" />
    [AddComponentMenu("TouchScript/Layers/Standard Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_StandardLayer.htm")]
    public class StandardLayer : TouchLayer
    {
        #region Public properties

        /// <summary>
        /// Indicates that the layer should look for 3D objects in the scene. Set this to <c>false</c> to optimize hit processing.
        /// </summary>
        public bool Hit3DObjects
        {
            get { return hit3DObjects; }
            set
            {
                hit3DObjects = value;
                updateVariants();
            }
        }

        /// <summary>
        /// Indicates that the layer should look for 2D objects in the scene. Set this to <c>false</c> to optimize hit processing.
        /// </summary>
        public bool Hit2DObjects
        {
            get { return hit2DObjects; }
            set
            {
                hit2DObjects = value;
                updateVariants();
            }
        }

        /// <summary>
        /// Indicates that the layer should look for World UI objects in the scene. Set this to <c>false</c> to optimize hit processing.
        /// </summary>
        public bool HitWorldSpaceUI
        {
            get { return hitWorldSpaceUI; }
            set
            {
                hitWorldSpaceUI = value;
                setupInputModule();
                updateVariants();
            }
        }

        /// <summary>
        /// Indicates that the layer should look for Screen Space UI objects in the scene. Set this to <c>false</c> to optimize hit processing.
        /// </summary>
        public bool HitScreenSpaceUI
        {
            get { return hitScreenSpaceUI; }
            set
            {
                hitScreenSpaceUI = value;
                setupInputModule();
            }
        }

        /// <summary>
        /// Indicates that the layer should query for <see cref="HitTest"/> components on target objects. Set this to <c>false</c> to optimize hit processing.
        /// </summary>
        public bool UseHitFilters
        {
            get { return useHitFilters; }
            set { useHitFilters = value; }
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

#pragma warning disable CS0414

		[SerializeField]
		[HideInInspector]
		private bool basicEditor = true;

		[SerializeField]
        [HideInInspector]
        private bool advancedProps; // is used to save if advanced properties are opened or closed

#pragma warning restore CS0414

		[SerializeField]
        [HideInInspector]
        private bool hitProps;

        [SerializeField]
        [ToggleLeft]
        private bool hit3DObjects = true;

        [SerializeField]
        [ToggleLeft]
        private bool hit2DObjects = true;

        [SerializeField]
        [ToggleLeft]
        private bool hitWorldSpaceUI = true;

        [SerializeField]
        [ToggleLeft]
        private bool hitScreenSpaceUI = true;

        [SerializeField]
        private LayerMask layerMask = -1;

        [SerializeField]
        [ToggleLeft]
        private bool useHitFilters = false;

        private bool lookForCameraObjects = false;
        private TouchScriptInputModule inputModule;

        /// <summary>
        /// Camera.
        /// </summary>
        protected Camera _camera;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override HitResult Hit(IPointer pointer, out HitData hit)
        {
            if (base.Hit(pointer, out hit) != HitResult.Hit) return HitResult.Miss;

            var result = HitResult.Miss;

            if (hitScreenSpaceUI)
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

            if (lookForCameraObjects)
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

            return HitResult.Miss;
        }

        /// <inheritdoc />
        public override ProjectionParams GetProjectionParams(Pointer pointer)
        {
            var press = pointer.GetPressData();
            if ((press.Type == HitData.HitType.World2D) ||
                (press.Type == HitData.HitType.World3D))
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
            updateVariants();
            base.Awake();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            TouchManager.Instance.FrameStarted += frameStartedHandler;
            StartCoroutine(lateEnable());
        }

        private IEnumerator lateEnable()
        {
            // Need to wait while EventSystem initializes
            yield return new WaitForEndOfFrame();
            setupInputModule();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (inputModule != null) inputModule.INTERNAL_Release();
            if (TouchManager.Instance != null) TouchManager.Instance.FrameStarted -= frameStartedHandler;
        }

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
			basicEditor = true;
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
                if (_camera != null) Name = _camera.name;
                else Name = "Layer";
        }

        #endregion

        #region Private functions

        private void setupInputModule()
        {
            if (inputModule == null)
            {
                if (!hitWorldSpaceUI && !hitScreenSpaceUI) return;
                inputModule = TouchScriptInputModule.Instance;
                if (inputModule != null) TouchScriptInputModule.Instance.INTERNAL_Retain();
            }
            else
            {
                if (hitWorldSpaceUI || hitScreenSpaceUI) return;
                inputModule.INTERNAL_Release();
                inputModule = null;
            }
        }

        private HitResult performWorldSearch(IPointer pointer, out HitData hit)
        {
            hit = default(HitData);

            if (_camera == null) return HitResult.Miss;
            if ((_camera.enabled == false) || (_camera.gameObject.activeInHierarchy == false)) return HitResult.Miss;
            var position = pointer.Position;
            if (!_camera.pixelRect.Contains(position)) return HitResult.Miss;

            hitList.Clear();
            var ray = _camera.ScreenPointToRay(position);

            int count;
            bool exclusiveSet = manager.HasExclusive;

            if (hit3DObjects)
            {
#if UNITY_5_3_OR_NEWER
                count = Physics.RaycastNonAlloc(ray, raycastHits, float.PositiveInfinity, layerMask);
#else
                var raycastHits = Physics.RaycastAll(ray, float.PositiveInfinity, layerMask);
                var count = raycastHits.Length;
#endif

                // Try to do some optimizations if 2D and WS UI are not required
                if (!hit2DObjects && !hitWorldSpaceUI)
                {
                    RaycastHit raycast;

                    if (count == 0) return HitResult.Miss;
                    if (count > 1)
                    {
                        raycastHitList.Clear();
                        for (var i = 0; i < count; i++)
                        {
                            raycast = raycastHits[i];
                            if (exclusiveSet && !manager.IsExclusive(raycast.transform)) continue;
                            raycastHitList.Add(raycast);
                        }
                        if (raycastHitList.Count == 0) return HitResult.Miss;

                        raycastHitList.Sort(_raycastHitComparerFunc);
                        if (useHitFilters)
                        {
                            for (var i = 0; i < count; i++)
                            {
                                var result = doHit(pointer, raycastHitList[i], out hit);
                                if (result != HitResult.Miss) return result;
                            }
                            return HitResult.Miss;
                        }
                        hit = new HitData(raycastHitList[0], this);
                        return HitResult.Hit;
                    }

                    raycast = raycastHits[0];
                    if (exclusiveSet && !manager.IsExclusive(raycast.transform)) return HitResult.Miss;
                    if (useHitFilters) return doHit(pointer, raycast, out hit);
                    hit = new HitData(raycast, this);
                    return HitResult.Hit;
                }
                for (var i = 0; i < count; i++)
                {
                    var raycast = raycastHits[i];
                    if (exclusiveSet && !manager.IsExclusive(raycast.transform)) continue;
                    hitList.Add(new HitData(raycastHits[i], this));
                }
            }

            if (hit2DObjects)
            {
                count = Physics2D.GetRayIntersectionNonAlloc(ray, raycastHits2D, float.MaxValue, layerMask);
                for (var i = 0; i < count; i++)
                {
                    var raycast = raycastHits2D[i];
                    if (exclusiveSet && !manager.IsExclusive(raycast.transform)) continue;
                    hitList.Add(new HitData(raycast, this));
                }
            }

            if (hitWorldSpaceUI)
            {
                raycastHitUIList.Clear();
                if (raycasters == null) raycasters = TouchScriptInputModule.Instance.GetRaycasters();
                count = raycasters.Count;

                for (var i = 0; i < count; i++)
                {
                    var raycaster = raycasters[i] as GraphicRaycaster;
                    if (raycaster == null) continue;
                    var canvas = TouchScriptInputModule.Instance.GetCanvasForRaycaster(raycaster);
                    if ((canvas == null) || (canvas.renderMode == RenderMode.ScreenSpaceOverlay) || (canvas.worldCamera != _camera)) continue;
                    performUISearchForCanvas(pointer, canvas, raycaster, _camera, float.MaxValue, ray);
                }

                count = raycastHitUIList.Count;
                for (var i = 0; i < count; i++) hitList.Add(new HitData(raycastHitUIList[i], this));
            }

            count = hitList.Count;
            if (hitList.Count == 0) return HitResult.Miss;
            if (count > 1)
            {
                hitList.Sort(_hitDataComparerFunc);
                if (useHitFilters)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        hit = hitList[i];
                        var result = checkHitFilters(pointer, hit);
                        if (result != HitResult.Miss) return result;
                    }
                    return HitResult.Miss;
                }
                else
                {
                    hit = hitList[0];
                    return HitResult.Hit;
                }
            }
            hit = hitList[0];
            if (useHitFilters) return checkHitFilters(pointer, hit);
            return HitResult.Hit;
        }

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
                var canvas = TouchScriptInputModule.Instance.GetCanvasForRaycaster(raycaster);
                if ((canvas == null) || (canvas.renderMode != RenderMode.ScreenSpaceOverlay)) continue;
                performUISearchForCanvas(pointer, canvas, raycaster);
            }

            count = raycastHitUIList.Count;
            if (count == 0) return HitResult.Miss;
            if (count > 1)
            {
                raycastHitUIList.Sort(_raycastHitUIComparerFunc);
                if (useHitFilters)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var result = doHit(pointer, raycastHitUIList[i], out hit);
                        if (result != HitResult.Miss) return result;
                    }
                    return HitResult.Miss;
                }

                hit = new HitData(raycastHitUIList[0], this, true);
                return HitResult.Hit;
            }

            if (useHitFilters) return doHit(pointer, raycastHitUIList[0], out hit);
            hit = new HitData(raycastHitUIList[0], this, true);
            return HitResult.Hit;
        }

        private void performUISearchForCanvas(IPointer pointer, Canvas canvas, GraphicRaycaster raycaster, Camera eventCamera = null, float maxDistance = float.MaxValue, Ray ray = default(Ray))
        {
            var position = pointer.Position;
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            var count = foundGraphics.Count;
            var exclusiveSet = manager.HasExclusive;

            for (var i = 0; i < count; i++)
            {
                var graphic = foundGraphics[i];
                var t = graphic.transform;

                if (exclusiveSet && !manager.IsExclusive(t)) continue;

                if ((layerMask.value != -1) && ((layerMask.value & (1 << graphic.gameObject.layer)) == 0)) continue;

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if ((graphic.depth == -1) || !graphic.raycastTarget)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, position, eventCamera))
                    continue;

                if (graphic.Raycast(position, eventCamera))
                {
                    if (raycaster.ignoreReversedGraphics)
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

                    float distance = 0;

                    if ((eventCamera == null) || (canvas.renderMode == RenderMode.ScreenSpaceOverlay)) {}
                    else
                    {
                        var transForward = t.forward;
                        // http://geomalgorithms.com/a06-_intersect-2.html
                        distance = Vector3.Dot(transForward, t.position - ray.origin) / Vector3.Dot(transForward, ray.direction);

                        // Check to see if the go is behind the camera.
                        if (distance < 0) continue;
                        if (distance >= maxDistance) continue;
                    }

                    raycastHitUIList.Add(
                        new RaycastHitUI()
                        {
                            Target = graphic.transform,
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

        private void updateVariants()
        {
            lookForCameraObjects = _camera != null && (hit3DObjects || hit2DObjects || hitWorldSpaceUI);
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

            if ((lhs.Type == HitData.HitType.UI) && (rhs.Type == HitData.HitType.UI))
            {
                if (lhs.RaycastHitUI.Depth != rhs.RaycastHitUI.Depth)
                    return rhs.RaycastHitUI.Depth.CompareTo(lhs.RaycastHitUI.Depth);

                if (!Mathf.Approximately(lhs.Distance, rhs.Distance))
                    return lhs.Distance.CompareTo(rhs.Distance);

                if (lhs.RaycastHitUI.GraphicIndex != rhs.RaycastHitUI.GraphicIndex)
                    return rhs.RaycastHitUI.GraphicIndex.CompareTo(lhs.RaycastHitUI.GraphicIndex);
            }

            return lhs.Distance < rhs.Distance ? -1 : 1;
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