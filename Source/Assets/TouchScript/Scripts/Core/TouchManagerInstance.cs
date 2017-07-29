/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Devices.Display;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;
using TouchScript.Core;
#if TOUCHSCRIPT_DEBUG
using TouchScript.Debugging.GL;
using TouchScript.Debugging.Loggers;
#endif
#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace TouchScript.Core
{
    /// <summary>
    /// Default implementation of <see cref="ITouchManager"/>.
    /// </summary>
    public sealed class TouchManagerInstance : DebuggableMonoBehaviour, ITouchManager
    {
        #region Events

        /// <inheritdoc />
        public event EventHandler FrameStarted
        {
            add { frameStartedInvoker += value; }
            remove { frameStartedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler FrameFinished
        {
            add { frameFinishedInvoker += value; }
            remove { frameFinishedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersAdded
        {
            add { pointersAddedInvoker += value; }
            remove { pointersAddedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersUpdated
        {
            add { pointersUpdatedInvoker += value; }
            remove { pointersUpdatedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersPressed
        {
            add { pointersPressedInvoker += value; }
            remove { pointersPressedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersReleased
        {
            add { pointersReleasedInvoker += value; }
            remove { pointersReleasedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersRemoved
        {
            add { pointersRemovedInvoker += value; }
            remove { pointersRemovedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersCancelled
        {
            add { pointersCancelledInvoker += value; }
            remove { pointersCancelledInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<PointerEventArgs> pointersAddedInvoker, pointersUpdatedInvoker, pointersPressedInvoker, pointersReleasedInvoker, pointersRemovedInvoker, pointersCancelledInvoker;

        private EventHandler frameStartedInvoker, frameFinishedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the instance of TouchManager singleton.
        /// </summary>
        public static TouchManagerInstance Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    if (!Application.isPlaying) return null;
                    var objects = FindObjectsOfType<TouchManagerInstance>();
                    if (objects.Length == 0)
                    {
                        var go = new GameObject("TouchManager Instance");
                        instance = go.AddComponent<TouchManagerInstance>();
                    }
                    else if (objects.Length >= 1)
                    {
                        instance = objects[0];
                    }
                }
                return instance;
            }
        }

        /// <inheritdoc />
        public IDisplayDevice DisplayDevice
        {
            get
            {
                if (displayDevice == null)
                {
                    displayDevice = ScriptableObject.CreateInstance<GenericDisplayDevice>();
                }
                return displayDevice;
            }
            set
            {
                if (value == null)
                {
                    displayDevice = ScriptableObject.CreateInstance<GenericDisplayDevice>();
                }
                else
                {
                    displayDevice = value;
                }
                UpdateResolution();
            }
        }

        /// <inheritdoc />
        public float DPI
        {
            get { return dpi; }
        }

        /// <inheritdoc />
        public bool ShouldCreateCameraLayer
        {
            get { return shouldCreateCameraLayer; }
            set { shouldCreateCameraLayer = value; }
        }

        /// <inheritdoc />
        public bool ShouldCreateStandardInput
        {
            get { return shouldCreateStandardInput; }
            set { shouldCreateStandardInput = value; }
        }

        /// <inheritdoc />
        public IList<IInputSource> Inputs
        {
            get { return new List<IInputSource>(inputs); }
        }

        /// <inheritdoc />
        public float DotsPerCentimeter
        {
            get { return dotsPerCentimeter; }
        }

        /// <inheritdoc />
        public int PointersCount
        {
            get { return pointers.Count; }
        }

        /// <inheritdoc />
        public IList<Pointer> Pointers
        {
            get { return new List<Pointer>(pointers); }
        }

        /// <inheritdoc />
        public int PressedPointersCount
        {
            get { return pressedPointers.Count; }
        }

        /// <inheritdoc />
        public IList<Pointer> PressedPointers
        {
            get { return new List<Pointer>(pressedPointers); }
        }

        /// <inheritdoc />
        public bool IsInsidePointerFrame { get; private set; }

        #endregion

        #region Private variables

        private static bool shuttingDown = false;
        private static TouchManagerInstance instance;

        private bool shouldCreateCameraLayer = true;
        private bool shouldCreateStandardInput = true;

        private IDisplayDevice displayDevice;
        private float dpi = 96;
        private float dotsPerCentimeter = TouchManager.CM_TO_INCH * 96;

        private ILayerManager layerManager;

        private List<IInputSource> inputs = new List<IInputSource>(3);
        private int inputCount = 0;

        private List<Pointer> pointers = new List<Pointer>(30);
        private HashSet<Pointer> pressedPointers = new HashSet<Pointer>();
        private Dictionary<int, Pointer> idToPointer = new Dictionary<int, Pointer>(30);

        // Upcoming changes
        private List<Pointer> pointersAdded = new List<Pointer>(10);
        private HashSet<int> pointersUpdated = new HashSet<int>();
        private HashSet<int> pointersPressed = new HashSet<int>();
        private HashSet<int> pointersReleased = new HashSet<int>();
        private HashSet<int> pointersRemoved = new HashSet<int>();
        private HashSet<int> pointersCancelled = new HashSet<int>();

        private static ObjectPool<List<Pointer>> pointerListPool = new ObjectPool<List<Pointer>>(2,
            () => new List<Pointer>(10), null, (l) => l.Clear());

        private static ObjectPool<List<int>> intListPool = new ObjectPool<List<int>>(3, () => new List<int>(10), null,
            (l) => l.Clear());

        private int nextPointerId = 0;
        private object pointerLock = new object();

		// Cache delegates
		private Func<TouchLayer, bool> _layerAddPointer, _layerUpdatePointer, _layerRemovePointer, _layerCancelPointer;

        #endregion

        #region Temporary variables

        // Used in layer dispatch fucntions
        private Pointer tmpPointer;

        #endregion

        #region Debug

#if TOUCHSCRIPT_DEBUG
        private IPointerLogger pLogger;
#endif

		private CustomSampler samplerUpdateInputs, samplerUpdateAdded, samplerUpdatePressed, samplerUpdateUpdated, samplerUpdateReleased, samplerUpdateRemoved, samplerUpdateCancelled;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public bool AddInput(IInputSource input)
        {
            if (input == null) return false;
            if (inputs.Contains(input)) return true;
            inputs.Add(input);
            inputCount++;
            return true;
        }

        /// <inheritdoc />
        public bool RemoveInput(IInputSource input)
        {
            if (input == null) return false;
            var result = inputs.Remove(input);
            if (result) inputCount--;
            return result;
        }

        /// <inheritdoc />
        public void CancelPointer(int id, bool shouldReturn)
        {
            Pointer pointer;
            if (idToPointer.TryGetValue(id, out pointer))
            {
                pointer.InputSource.CancelPointer(pointer, shouldReturn);
            }
        }

        /// <inheritdoc />
        public void CancelPointer(int id)
        {
            CancelPointer(id, false);
        }

        /// <inheritdoc />
        public void UpdateResolution()
        {
            if (DisplayDevice != null)
            {
                DisplayDevice.UpdateDPI();
                dpi = DisplayDevice.DPI;
            }
            else
            {
                dpi = 96;
            }
            dotsPerCentimeter = TouchManager.CM_TO_INCH * dpi;
#if TOUCHSCRIPT_DEBUG
            debugPointerSize = Vector2.one * dotsPerCentimeter;
#endif
            
            foreach (var input in inputs) input.UpdateResolution();
        }

        #endregion

        #region Internal methods

        internal void INTERNAL_AddPointer(Pointer pointer)
        {
            lock (pointerLock)
            {
                pointer.INTERNAL_Init(nextPointerId);
                pointersAdded.Add(pointer);

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.IdAllocated);
#endif

                nextPointerId++;
            }
        }

        internal void INTERNAL_UpdatePointer(int id)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
                    // This pointer was added this frame
					if (!wasPointerAddedThisFrame(id, out pointer))
                    {
						// No pointer with such id
#if TOUCHSCRIPT_DEBUG
                        if (DebugMode) Debug.LogWarning("TouchScript > Pointer with id [" + id + "] is requested to MOVE to but no pointer with such id found.");
#endif
                        return;
                    }
                }

                pointersUpdated.Add(id);
            }
        }

        internal void INTERNAL_PressPointer(int id)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
                    // This pointer was added this frame
					if (!wasPointerAddedThisFrame(id, out pointer))
					{
						// No pointer with such id
#if TOUCHSCRIPT_DEBUG
                        if (DebugMode)
                            Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                             "] is requested to PRESS but no pointer with such id found.");
#endif
                        return;
                    }
                }
#if TOUCHSCRIPT_DEBUG
                if (!pointersPressed.Add(id))
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to PRESS more than once this frame.");
#else
                pointersPressed.Add(id);
#endif

            }
        }

        /// <inheritdoc />
        internal void INTERNAL_ReleasePointer(int id)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
					// This pointer was added this frame
					if (!wasPointerAddedThisFrame(id, out pointer))
					{
						// No pointer with such id
#if TOUCHSCRIPT_DEBUG
                        if (DebugMode)
                            Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                             "] is requested to END but no pointer with such id found.");
#endif
                        return;
                    }
                }
#if TOUCHSCRIPT_DEBUG
                if (!pointersReleased.Add(id))
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to END more than once this frame.");
#else
                pointersReleased.Add(id);
#endif

            }
        }

        /// <inheritdoc />
        internal void INTERNAL_RemovePointer(int id)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
					// This pointer was added this frame
					if (!wasPointerAddedThisFrame(id, out pointer))
					{
						// No pointer with such id
#if TOUCHSCRIPT_DEBUG
                        if (DebugMode)
                            Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                             "] is requested to REMOVE but no pointer with such id found.");
#endif
                        return;
                    }
                }
#if TOUCHSCRIPT_DEBUG
                if (!pointersRemoved.Add(pointer.Id))
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to REMOVE more than once this frame.");
#else
                pointersRemoved.Add(pointer.Id);
#endif

            }
        }

        /// <inheritdoc />
        internal void INTERNAL_CancelPointer(int id)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
					// This pointer was added this frame
					if (!wasPointerAddedThisFrame(id, out pointer))
					{
						// No pointer with such id
#if TOUCHSCRIPT_DEBUG
                        if (DebugMode)
                            Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                             "] is requested to CANCEL but no pointer with such id found.");
#endif
                        return;
                    }
                }
#if TOUCHSCRIPT_DEBUG
                if (!pointersCancelled.Add(pointer.Id))
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to CANCEL more than once this frame.");
#else
                pointersCancelled.Add(pointer.Id);
#endif

            }
        }

        #endregion

        #region Unity

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
                return;
            }

#if TOUCHSCRIPT_DEBUG
            pLogger = Debugging.TouchScriptDebugger.Instance.PointerLogger;
#endif

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += sceneLoadedHandler;
#endif

            gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(gameObject);

            layerManager = LayerManager.Instance;

            UpdateResolution();

            StopAllCoroutines();
            StartCoroutine(lateAwake());

            pointerListPool.WarmUp(2);
            intListPool.WarmUp(3);

			_layerAddPointer = layerAddPointer;
			_layerUpdatePointer = layerUpdatePointer;
			_layerRemovePointer = layerRemovePointer;
			_layerCancelPointer = layerCancelPointer;

            samplerUpdateInputs = CustomSampler.Create("[TouchScript] Update Inputs");
			samplerUpdateAdded = CustomSampler.Create("[TouchScript] Added Pointers");
			samplerUpdatePressed = CustomSampler.Create("[TouchScript] Press Pointers");
			samplerUpdateUpdated = CustomSampler.Create("[TouchScript] Update Pointers");
			samplerUpdateReleased = CustomSampler.Create("[TouchScript] Release Pointers");
			samplerUpdateRemoved = CustomSampler.Create("[TouchScript] Remove Pointers");
			samplerUpdateCancelled = CustomSampler.Create("[TouchScript] Cancel Pointers");
        }

#if UNITY_5_4_OR_NEWER
        private void sceneLoadedHandler(Scene scene, LoadSceneMode mode)
        {
            StopAllCoroutines();
            StartCoroutine(lateAwake());
        }
#else
        private void OnLevelWasLoaded(int value)
        {
            StopAllCoroutines();
            StartCoroutine(lateAwake());
        }
#endif

        private IEnumerator lateAwake()
        {
            // Wait 2 frames:
            // Frame 0: TouchManager adds layers in order
            // Frame 1: Layers add themselves
            // Frame 2: We add a layer if there are none
            yield return null;
            yield return null;

            createCameraLayer();
            createInput();
        }

        private void Update()
        {
            sendFrameStartedToPointers();
            updateInputs();
            updatePointers();
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Private functions

        private void createCameraLayer()
        {
            if (layerManager.LayerCount == 0 && shouldCreateCameraLayer)
            {
                if (Camera.main != null)
                {
                    if (Application.isEditor)
                        Debug.Log(
                            "[TouchScript] No touch layers found, adding StandardLayer for the main camera. (this message is harmless)");
                    var layer = Camera.main.gameObject.AddComponent<StandardLayer>();
                    layerManager.AddLayer(layer);
                }
            }
        }

        private void createInput()
        {
            if (inputCount == 0 && shouldCreateStandardInput)
            {
                if (Application.isEditor)
                    Debug.Log("[TouchScript] No input source found, adding StandardInput. (this message is harmless)");
                GameObject obj = null;
                var objects = FindObjectsOfType<TouchManager>();
                if (objects.Length == 0)
                {
                    obj = GameObject.Find("TouchScript");
                    if (obj == null) obj = new GameObject("TouchScript");
                }
                else
                {
                    obj = objects[0].gameObject;
                }
                obj.AddComponent<StandardInput>();
            }
        }

        private void updateInputs()
        {
            samplerUpdateInputs.Begin();
            for (var i = 0; i < inputCount; i++) inputs[i].UpdateInput();
            samplerUpdateInputs.End();
        }

        private void updateAdded(List<Pointer> pointers)
        {
			samplerUpdateAdded.Begin();

            var addedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < addedCount; i++)
            {
                var pointer = pointers[i];
                list.Add(pointer);
                this.pointers.Add(pointer);
                idToPointer.Add(pointer.Id, pointer);

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.Added);
#endif

                tmpPointer = pointer;
                layerManager.ForEach(_layerAddPointer);
                tmpPointer = null;

#if TOUCHSCRIPT_DEBUG
                if (DebugMode) addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersAddedInvoker != null)
                pointersAddedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);

			samplerUpdateAdded.End();
        }

        private bool layerAddPointer(TouchLayer layer)
        {
            layer.INTERNAL_AddPointer(tmpPointer);
            return true;
        }

        private void updateUpdated(List<int> pointers)
        {
			samplerUpdateUpdated.Begin();

            var updatedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < updatedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Id [" + id +
                                         "] was in UPDATED list but no pointer with such id found.");
#endif
                    continue;
                }
                list.Add(pointer);

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.Updated);
#endif

                var layer = pointer.GetPressData().Layer;
                if (layer != null) layer.INTERNAL_UpdatePointer(pointer);
                else
                {
                    tmpPointer = pointer;
					layerManager.ForEach(_layerUpdatePointer);
                    tmpPointer = null;
                }

#if TOUCHSCRIPT_DEBUG
                if (DebugMode) addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersUpdatedInvoker != null)
                pointersUpdatedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);

			samplerUpdateUpdated.End();
        }

        private bool layerUpdatePointer(TouchLayer layer)
        {
            layer.INTERNAL_UpdatePointer(tmpPointer);
            return true;
        }

        private void updatePressed(List<int> pointers)
        {
			samplerUpdatePressed.Begin();

            var pressedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < pressedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Id [" + id +
                                         "] was in PRESSED list but no pointer with such id found.");
#endif
                    continue;
                }
                list.Add(pointer);
                pressedPointers.Add(pointer);

                HitData hit = pointer.GetOverData();
                if (hit.Layer != null)
                {
                    pointer.INTERNAL_SetPressData(hit);
                    hit.Layer.INTERNAL_PressPointer(pointer);
                }

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.Pressed);
#endif

#if TOUCHSCRIPT_DEBUG
                if (DebugMode) addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersPressedInvoker != null)
                pointersPressedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);

			samplerUpdatePressed.End();
        }

        private void updateReleased(List<int> pointers)
        {
			samplerUpdateReleased.Begin();

            var releasedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < releasedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    if (DebugMode) Debug.LogWarning("TouchScript > Id [" + id + "] was in RELEASED list but no pointer with such id found.");
#endif
                    continue;
                }
                list.Add(pointer);
                pressedPointers.Remove(pointer);

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.Released);
#endif

                var layer = pointer.GetPressData().Layer;
                if (layer != null) layer.INTERNAL_ReleasePointer(pointer);

#if TOUCHSCRIPT_DEBUG
                if (DebugMode) addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersReleasedInvoker != null)
                pointersReleasedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));

            releasedCount = list.Count;
            for (var i = 0; i < releasedCount; i++)
            {
                var pointer = list[i];
                pointer.INTERNAL_ClearPressData();
            }
            pointerListPool.Release(list);

			samplerUpdateReleased.End();
        }

        private void updateRemoved(List<int> pointers)
        {
			samplerUpdateRemoved.Begin();

            var removedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < removedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    if (DebugMode) Debug.LogWarning("TouchScript > Id [" + id + "] was in REMOVED list but no pointer with such id found.");
#endif
                    continue;
                }
                idToPointer.Remove(id);
                this.pointers.Remove(pointer);
                pressedPointers.Remove(pointer);
                list.Add(pointer);

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.Removed);
#endif

                tmpPointer = pointer;
                layerManager.ForEach(_layerRemovePointer);
                tmpPointer = null;

#if TOUCHSCRIPT_DEBUG
                if (DebugMode) removeDebugFigureForPointer(pointer);
#endif
            }

            if (pointersRemovedInvoker != null)
                pointersRemovedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));

            removedCount = list.Count;
            for (var i = 0; i < removedCount; i++)
            {
                var pointer = list[i];
                pointer.InputSource.INTERNAL_DiscardPointer(pointer);
            }
            pointerListPool.Release(list);

			samplerUpdateRemoved.End();
        }

        private bool layerRemovePointer(TouchLayer layer)
        {
            layer.INTERNAL_RemovePointer(tmpPointer);
            return true;
        }

        private void updateCancelled(List<int> pointers)
        {
			samplerUpdateCancelled.Begin();

            var cancelledCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < cancelledCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    if (DebugMode)
                        Debug.LogWarning("TouchScript > Id [" + id +
                                         "] was in CANCELLED list but no pointer with such id found.");
#endif
                    continue;
                }
                idToPointer.Remove(id);
                this.pointers.Remove(pointer);
                pressedPointers.Remove(pointer);
                list.Add(pointer);

#if TOUCHSCRIPT_DEBUG
                pLogger.Log(pointer, PointerEvent.Cancelled);
#endif

                tmpPointer = pointer;
                layerManager.ForEach(_layerCancelPointer);
                tmpPointer = null;

#if TOUCHSCRIPT_DEBUG
                if (DebugMode) removeDebugFigureForPointer(pointer);
#endif
            }

            if (pointersCancelledInvoker != null)
                pointersCancelledInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));

            for (var i = 0; i < cancelledCount; i++)
            {
                var pointer = list[i];
                pointer.InputSource.INTERNAL_DiscardPointer(pointer);
            }
            pointerListPool.Release(list);

			samplerUpdateCancelled.End();
        }

        private bool layerCancelPointer(TouchLayer layer)
        {
            layer.INTERNAL_CancelPointer(tmpPointer);
            return true;
        }

        private void sendFrameStartedToPointers()
        {
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                pointers[i].INTERNAL_FrameStarted();
            }
        }

        private void updatePointers()
        {
            IsInsidePointerFrame = true;
            if (frameStartedInvoker != null) frameStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);

            // need to copy buffers since they might get updated during execution
            List<Pointer> addedList = null;
            List<int> updatedList = null;
            List<int> pressedList = null;
            List<int> releasedList = null;
            List<int> removedList = null;
            List<int> cancelledList = null;
            lock (pointerLock)
            {
                if (pointersAdded.Count > 0)
                {
                    addedList = pointerListPool.Get();
                    addedList.AddRange(pointersAdded);
                    pointersAdded.Clear();
                }
                if (pointersUpdated.Count > 0)
                {
                    updatedList = intListPool.Get();
                    updatedList.AddRange(pointersUpdated);
                    pointersUpdated.Clear();
                }
                if (pointersPressed.Count > 0)
                {
                    pressedList = intListPool.Get();
                    pressedList.AddRange(pointersPressed);
                    pointersPressed.Clear();
                }
                if (pointersReleased.Count > 0)
                {
                    releasedList = intListPool.Get();
                    releasedList.AddRange(pointersReleased);
                    pointersReleased.Clear();
                }
                if (pointersRemoved.Count > 0)
                {
                    removedList = intListPool.Get();
                    removedList.AddRange(pointersRemoved);
                    pointersRemoved.Clear();
                }
                if (pointersCancelled.Count > 0)
                {
                    cancelledList = intListPool.Get();
                    cancelledList.AddRange(pointersCancelled);
                    pointersCancelled.Clear();
                }
            }

            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                pointers[i].INTERNAL_UpdatePosition();
            }

            if (addedList != null)
            {
                updateAdded(addedList);
                pointerListPool.Release(addedList);
            }

            if (updatedList != null)
            {
                updateUpdated(updatedList);
                intListPool.Release(updatedList);
            }
            if (pressedList != null)
            {
                updatePressed(pressedList);
                intListPool.Release(pressedList);
            }
            if (releasedList != null)
            {
                updateReleased(releasedList);
                intListPool.Release(releasedList);
            }
            if (removedList != null)
            {
                updateRemoved(removedList);
                intListPool.Release(removedList);
            }
            if (cancelledList != null)
            {
                updateCancelled(cancelledList);
                intListPool.Release(cancelledList);
            }

            if (frameFinishedInvoker != null) frameFinishedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            IsInsidePointerFrame = false;
        }

		private bool wasPointerAddedThisFrame(int id, out Pointer pointer)
		{
			pointer = null;
			foreach (var p in pointersAdded)
			{
				if (p.Id == id)
				{
					pointer = p;
					return true;
				}
			}
			return false;
		}

#if TOUCHSCRIPT_DEBUG
        private Vector2 debugPointerSize;

        private void removeDebugFigureForPointer(Pointer pointer)
        {
            GLDebug.RemoveFigure(TouchManager.DEBUG_GL_TOUCH + pointer.Id);
        }

        private void addDebugFigureForPointer(Pointer pointer)
        {
            GLDebug.DrawSquareScreenSpace(TouchManager.DEBUG_GL_TOUCH + pointer.Id, pointer.Position, 0, debugPointerSize,
                GLDebug.MULTIPLY, float.PositiveInfinity);
        }
#endif

        #endregion
    }
}