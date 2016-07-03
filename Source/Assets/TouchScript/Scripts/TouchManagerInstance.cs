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
        public event EventHandler<PointerEventArgs> PointersBegan
        {
            add { pointersBeganInvoker += value; }
            remove { pointersBeganInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersMoved
        {
            add { pointersMovedInvoker += value; }
            remove { pointersMovedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersEnded
        {
            add { pointersEndedInvoker += value; }
            remove { pointersEndedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<PointerEventArgs> PointersCancelled
        {
            add { pointersCancelledInvoker += value; }
            remove { pointersCancelledInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<PointerEventArgs> pointersBeganInvoker,
                                             pointersMovedInvoker,
                                             pointersEndedInvoker,
                                             pointersCancelledInvoker;

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
        public int NumberOfPointers
        {
            get { return pointers.Count; }
        }

        /// <inheritdoc />
        public IList<Pointer> ActivePointers
        {
            get { return new List<Pointer>(pointers); }
        }

        #endregion

        #region Private variables

        private static bool shuttingDown = false;
        private static TouchManagerInstance instance;
        private bool shouldCreateCameraLayer = true;
        private bool shouldCreateStandardInput = true;

        private IDisplayDevice displayDevice;
        private float dpi = 96;
        private float dotsPerCentimeter = TouchManager.CM_TO_INCH * 96;

        private List<TouchLayer> layers = new List<TouchLayer>(10);
        private int layerCount = 0;
        private List<IInputSource> inputs = new List<IInputSource>(3);
        private int inputCount = 0;

        private List<Pointer> pointers = new List<Pointer>(30);
        private Dictionary<int, Pointer> idToPointer = new Dictionary<int, Pointer>(30);

        // Upcoming changes
        private List<Pointer> pointersBegan = new List<Pointer>(10);
        private HashSet<int> pointersUpdated = new HashSet<int>();
        private HashSet<int> pointersEnded = new HashSet<int>();
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
            TouchHit hit;
            TouchLayer layer;
            if (GetHitTarget(position, out hit, out layer)) return hit.Transform;
            return null;
        }

        /// <inheritdoc />
        public bool GetHitTarget(Vector2 position, out TouchHit hit)
        {
            TouchLayer layer;
            return GetHitTarget(position, out hit, out layer);
        }

        /// <inheritdoc />
        public bool GetHitTarget(Vector2 position, out TouchHit hit, out TouchLayer layer)
        {
            hit = default(TouchHit);
            layer = null;

            for (var i = 0; i < layerCount; i++)
            {
                var touchLayer = layers[i];
                if (touchLayer == null) continue;
                TouchHit _hit;
                if (touchLayer.Hit(position, out _hit) == TouchLayer.LayerHitResult.Hit)
                {
                    hit = _hit;
                    layer = touchLayer;
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void CancelPointer(int id, bool @return)
        {
            Pointer pointer;
            if (idToPointer.TryGetValue(id, out pointer))
            {
                pointer.InputSource.CancelPointer(pointer, @return);
            }
        }

        /// <inheritdoc />
        public void CancelPointer(int id)
        {
            CancelPointer(id, false);
        }

        #endregion

        #region Internal methods

        internal void INTERNAL_BeginPointer(Pointer pointer, Vector2 position)
        {
            lock (pointerLock)
            {
                pointer.INTERNAL_Init(nextPointerId++, position);
                pointersBegan.Add(pointer);
            }
        }

        /// <summary>
        /// Update pointer without moving it
        /// </summary>
        /// <param name="id">Pointer id</param>
        internal void INTERNAL_UpdatePointer(int id)
        {
            lock (pointerLock)
            {
                if (idToPointer.ContainsKey(id))
                {
                    if (!pointersUpdated.Contains(id)) pointersUpdated.Add(id);
                }
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                     "] is requested to UPDATE but no pointer with such id found.");
#endif
            }
        }

        internal void INTERNAL_MovePointer(int id, Vector2 position)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
                    // This pointer was added this frame
                    pointer = pointersBegan.Find((t) => t.Id == id);
                    // No pointer with such id
                    if (pointer == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Pointer with id [" + id + "] is requested to MOVE to " + position +
                                         " but no pointer with such id found.");
#endif
                        return;
                    }
                }

                pointer.INTERNAL_SetPosition(position);
                if (!pointersUpdated.Contains(id)) pointersUpdated.Add(id);
            }
        }

        /// <inheritdoc />
        internal void INTERNAL_EndPointer(int id)
        {
            lock (pointerLock)
            {
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
                    // This pointer was added this frame
                    pointer = pointersBegan.Find((t) => t.Id == id);
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
                if (!pointersEnded.Contains(id)) pointersEnded.Add(id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Pointer with id [" + id +
                                     "] is requested to END more than once this frame.");
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
                    pointer = pointersBegan.Find((t) => t.Id == id);
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
            dotsPerCentimeter = TouchManager.CM_TO_INCH * dpi;
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

        private void updateBegan(List<Pointer> pointers)
        {
            var count = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < count; i++)
            {
                var pointer = pointers[i];
                list.Add(pointer);
                this.pointers.Add(pointer);
                idToPointer.Add(pointer.Id, pointer);

                for (var j = 0; j < layerCount; j++)
                {
                    var touchLayer = layers[j];
                    if (touchLayer == null || !touchLayer.enabled) continue;
                    if (touchLayer.INTERNAL_BeginPointer(pointer)) break;
                }

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersBeganInvoker != null)
                pointersBeganInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);
        }

        private void updateUpdated(List<int> pointers)
        {
            var updatedCount = pointers.Count;
            var list = pointerListPool.Get();
            // Need to loop through all pointers to reset those which did not move
            var count = this.pointers.Count;
            for (var i = 0; i < count; i++)
            {
                this.pointers[i].INTERNAL_ResetPosition();
            }
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
                if (pointer.Layer != null) pointer.Layer.INTERNAL_UpdatePointer(pointer);

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForPointer(pointer);
#endif
            }

            if (pointersMovedInvoker != null)
                pointersMovedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));
            pointerListPool.Release(list);
        }

        private void updateEnded(List<int> pointers)
        {
            var endedCount = pointers.Count;
            var list = pointerListPool.Get();
            for (var i = 0; i < endedCount; i++)
            {
                var id = pointers[i];
                Pointer pointer;
                if (!idToPointer.TryGetValue(id, out pointer))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id + "] was in ENDED list but no pointer with such id found.");
#endif
                    continue;
                }
                idToPointer.Remove(id);
                this.pointers.Remove(pointer);
                list.Add(pointer);
                if (pointer.Layer != null) pointer.Layer.INTERNAL_EndPointer(pointer);

#if TOUCHSCRIPT_DEBUG
                removeDebugFigureForPointer(pointer);
#endif
            }

            if (pointersEndedInvoker != null)
                pointersEndedInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));

            for (var i = 0; i < endedCount; i++)
            {
                var pointer = list[i];
                pointer.InputSource.INTERNAL_ReleasePointer(pointer);
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
                list.Add(pointer);
                if (pointer.Layer != null) pointer.Layer.INTERNAL_CancelPointer(pointer);

#if TOUCHSCRIPT_DEBUG
                removeDebugFigureForPointer(pointer);
#endif
            }

            if (pointersCancelledInvoker != null)
                pointersCancelledInvoker.InvokeHandleExceptions(this, PointerEventArgs.GetCachedEventArgs(list));

            for (var i = 0; i < cancelledCount; i++)
            {
                var pointer = list[i];
                pointer.InputSource.INTERNAL_ReleasePointer(pointer);
            }
            pointerListPool.Release(list);
        }

        private void updatePointers()
        {
            if (frameStartedInvoker != null) frameStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);

            // need to copy buffers since they might get updated during execution
            List<Pointer> beganList = null;
            List<int> updatedList = null;
            List<int> endedList = null;
            List<int> cancelledList = null;
            lock (pointerLock)
            {
                if (pointersBegan.Count > 0)
                {
                    beganList = pointerListPool.Get();
                    beganList.AddRange(pointersBegan);
                    pointersBegan.Clear();
                }
                if (pointersUpdated.Count > 0)
                {
                    updatedList = intListPool.Get();
                    updatedList.AddRange(pointersUpdated);
                    pointersUpdated.Clear();
                }
                if (pointersEnded.Count > 0)
                {
                    endedList = intListPool.Get();
                    endedList.AddRange(pointersEnded);
                    pointersEnded.Clear();
                }
                if (pointersCancelled.Count > 0)
                {
                    cancelledList = intListPool.Get();
                    cancelledList.AddRange(pointersCancelled);
                    pointersCancelled.Clear();
                }
            }

            if (beganList != null)
            {
                updateBegan(beganList);
                pointerListPool.Release(beganList);
            }
            if (updatedList != null)
            {
                updateUpdated(updatedList);
                intListPool.Release(updatedList);
            }
            if (endedList != null)
            {
                updateEnded(endedList);
                intListPool.Release(endedList);
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