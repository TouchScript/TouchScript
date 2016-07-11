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
#if TOUCHSCRIPT_DEBUG
using TouchScript.Utils.DebugUtils;
#endif
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Default implementation of <see cref="ITouchManager"/>.
    /// </summary>
    internal sealed class TouchManagerInstance : DebuggableMonoBehaviour, ITouchManager
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

        /// <inheritdoc />
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
                updateDPI();
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
        public IList<TouchLayer> Layers
        {
            get { return new List<TouchLayer>(layers); }
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

        #endregion

        #region Private variables

        private static bool shuttingDown = false;
        private static TouchManagerInstance instance;
        private bool shouldCreateCameraLayer = true;
        private bool shouldCreateStandardInput = true;

        private IDisplayDevice displayDevice;
        private float dpi = 96;
        private float dotsPerCentimeter = TouchManager.CM_TO_INCH*96;

        private List<TouchLayer> layers = new List<TouchLayer>(10);
        private int layerCount = 0;
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

        #endregion

        #region Public methods

        /// <inheritdoc />
        public bool AddLayer(TouchLayer layer, int index = -1, bool addIfExists = true)
        {
            if (layer == null) return false;

            var i = layers.IndexOf(layer);
            if (i != -1)
            {
                if (!addIfExists) return false;
                layers.RemoveAt(i);
                layerCount--;
            }
            if (index == 0)
            {
                layers.Insert(0, layer);
                layerCount++;
                return i == -1;
            }
            if (index == -1 || index >= layerCount)
            {
                layers.Add(layer);
                layerCount++;
                return i == -1;
            }
            if (i != -1)
            {
                if (index < i) layers.Insert(index, layer);
                else layers.Insert(index - 1, layer);
                layerCount++;
                return false;
            }
            layers.Insert(index, layer);
            layerCount++;
            return true;
        }

        /// <inheritdoc />
        public bool RemoveLayer(TouchLayer layer)
        {
            if (layer == null) return false;
            var result = layers.Remove(layer);
            if (result) layerCount--;
            return result;
        }

        /// <inheritdoc />
        public void ChangeLayerIndex(int at, int to)
        {
            if (at < 0 || at >= layerCount) return;
            if (to < 0 || to >= layerCount) return;
            var data = layers[at];
            layers.RemoveAt(at);
            layers.Insert(to, data);
        }

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
        public Transform GetHitTarget(Vector2 position)
        {
            HitData hit;
            if (GetHitTarget(position, out hit)) return hit.Target;
            return null;
        }

        /// <inheritdoc />
        public bool GetHitTarget(Vector2 position, out HitData hit)
        {
            hit = default(HitData);

            for (var i = 0; i < layerCount; i++)
            {
                var touchLayer = layers[i];
                if (touchLayer == null) continue;
                HitData _hit;
                if (touchLayer.Hit(position, out _hit) == TouchLayer.LayerHitResult.Hit)
                {
                    hit = _hit;
                    return true;
                }
            }

            return false;
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

        #endregion

        #region Internal methods

        internal void INTERNAL_AddPointer(Pointer pointer)
        {
            lock (pointerLock)
            {
                pointer.INTERNAL_Init(nextPointerId++);
                pointersAdded.Add(pointer);
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
                    pointer = pointersAdded.Find((t) => t.Id == id);
                    // No pointer with such id
                    if (pointer == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Pointer with id [" + id + "] is requested to MOVE to but no pointer with such id found.");
#endif
                        return;
                    }
                }

                if (!pointersUpdated.Contains(id)) pointersUpdated.Add(id);
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
                    pointer = pointersAdded.Find((t) => t.Id == id);
                    // No pointer with such id
                    if (pointer == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to PRESS but no pointer with such id found.");
#endif
                        return;
                    }
                }
                if (!pointersPressed.Contains(id)) pointersPressed.Add(id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                     "] is requested to PRESS more than once this frame.");
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
                    pointer = pointersAdded.Find((t) => t.Id == id);
                    // No pointer with such id
                    if (pointer == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to END but no pointer with such id found.");
#endif
                        return;
                    }
                }
                if (!pointersReleased.Contains(id)) pointersReleased.Add(id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                     "] is requested to END more than once this frame.");
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
                    pointer = pointersAdded.Find((t) => t.Id == id);
                    // No pointer with such id
                    if (pointer == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to REMOVE but no pointer with such id found.");
#endif
                        return;
                    }
                }
                if (!pointersRemoved.Contains(id)) pointersRemoved.Add(pointer.Id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                     "] is requested to REMOVE more than once this frame.");
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
                    pointer = pointersAdded.Find((t) => t.Id == id);
                    // No pointer with such id
                    if (pointer == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                         "] is requested to CANCEL but no pointer with such id found.");
#endif
                        return;
                    }
                }
                if (!pointersCancelled.Contains(id)) pointersCancelled.Add(pointer.Id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                     "] is requested to CANCEL more than once this frame.");
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

            gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(gameObject);

            updateDPI();

            StopAllCoroutines();
            StartCoroutine(lateAwake());

            pointerListPool.WarmUp(2);
            intListPool.WarmUp(3);

#if TOUCHSCRIPT_DEBUG
            DebugMode = true;
#endif
        }

        private void OnLevelWasLoaded(int value)
        {
            StopAllCoroutines();
            StartCoroutine(lateAwake());
        }

        private IEnumerator lateAwake()
        {
            yield return null;

            updateLayers();
            createCameraLayer();
            createInput();
        }

        private void Update()
        {
            updateInputs();
            updatePointers();
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Private functions

        private void updateDPI()
        {
            dpi = DisplayDevice == null ? 96 : DisplayDevice.DPI;
            dotsPerCentimeter = TouchManager.CM_TO_INCH*dpi;
#if TOUCHSCRIPT_DEBUG
            debugPointerSize = Vector2.one*dotsPerCentimeter;
#endif
        }

        private void updateLayers()
        {
            // filter empty layers
            layers = layers.FindAll(l => l != null);
            layerCount = layers.Count;
        }

        private void createCameraLayer()
        {
            if (layerCount == 0 && shouldCreateCameraLayer)
            {
                if (Camera.main != null)
                {
                    if (Application.isEditor)
                        Debug.Log(
                            "[TouchScript] No camera layer found, adding CameraLayer for the main camera. (this message is harmless)");
                    var layer = Camera.main.gameObject.AddComponent<CameraLayer>();
                    AddLayer(layer);
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
            for (var i = 0; i < inputCount; i++) inputs[i].UpdateInput();
        }

        private void updateAdded(List<Pointer> pointers)
        {
            var addedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < addedCount; i++)
            {
                var pointer = pointers[i];
                list.Add(pointer);
                this.pointers.Add(pointer);
                idToPointer.Add(pointer.Id, pointer);

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersAddedInvoker != null)
                pointersAddedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);
        }

        private void updateUpdated(List<int> pointers)
        {
            var updatedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < updatedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id +
                                     "] was in UPDATED list but no pointer with such id found.");
#endif
                    continue;
                }
                list.Add(pointer);
                var layer = pointer.GetPressData().Layer;
                if (layer != null) layer.INTERNAL_UpdatePointer(pointer);

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersUpdatedInvoker != null)
                pointersUpdatedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);
        }

        private void updatePressed(List<int> pointers)
        {
            var pressedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < pressedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id +
                                     "] was in PRESSED list but no pointer with such id found.");
#endif
                    continue;
                }
                list.Add(pointer);
                pressedPointers.Add(pointer);
                for (var j = 0; j < layerCount; j++)
                {
                    var touchLayer = layers[j];
                    if (touchLayer == null || !touchLayer.enabled) continue;
                    if (touchLayer.INTERNAL_PressPointer(pointer)) break;
                }

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersPressedInvoker != null)
                pointersPressedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);
        }

        private void updateReleased(List<int> pointers)
        {
            var releasedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < releasedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id + "] was in RELEASED list but no pointer with such id found.");
#endif
                    continue;
                }
                list.Add(pointer);
                pressedPointers.Remove(pointer);
                var layer = pointer.GetPressData().Layer;
                if (layer != null) layer.INTERNAL_ReleasePointer(pointer);

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForPointer(pointer);
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
        }

        private void updateRemoved(List<int> pointers)
        {
            var removedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < removedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id + "] was in REMOVED list but no pointer with such id found.");
#endif
                    continue;
                }
                idToPointer.Remove(id);
                this.pointers.Remove(pointer);
                pressedPointers.Remove(pointer);
                list.Add(pointer);

#if TOUCHSCRIPT_DEBUG
                removeDebugFigureForPointer(pointer);
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
        }

        private void updateCancelled(List<int> pointers)
        {
            var cancelledCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < cancelledCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id +
                                     "] was in CANCELLED list but no pointer with such id found.");
#endif
                    continue;
                }
                idToPointer.Remove(id);
                this.pointers.Remove(pointer);
                pressedPointers.Remove(pointer);
                list.Add(pointer);
                var layer = pointer.GetPressData().Layer;
                if (layer != null) layer.INTERNAL_CancelPointer(pointer);

#if TOUCHSCRIPT_DEBUG
                removeDebugFigureForPointer(pointer);
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
        }

        private void updatePointers()
        {
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

            if (addedList != null)
            {
                updateAdded(addedList);
                pointerListPool.Release(addedList);
            }

            // Need to loop through all pointers to update position/previousPosition.
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                pointers[i].INTERNAL_FrameStarted();
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