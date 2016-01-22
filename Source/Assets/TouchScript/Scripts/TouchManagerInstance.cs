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
#if TOUCHSCRIPT_DEBUG
using TouchScript.Utils.Debug;
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
        public event EventHandler<TouchEventArgs> TouchesBegan
        {
            add { touchesBeganInvoker += value; }
            remove { touchesBeganInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<TouchEventArgs> TouchesMoved
        {
            add { touchesMovedInvoker += value; }
            remove { touchesMovedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<TouchEventArgs> TouchesEnded
        {
            add { touchesEndedInvoker += value; }
            remove { touchesEndedInvoker -= value; }
        }

        /// <inheritdoc />
        public event EventHandler<TouchEventArgs> TouchesCancelled
        {
            add { touchesCancelledInvoker += value; }
            remove { touchesCancelledInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<TouchEventArgs> touchesBeganInvoker,
                                             touchesMovedInvoker,
                                             touchesEndedInvoker,
                                             touchesCancelledInvoker;

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
        public int NumberOfTouches
        {
            get { return touches.Count; }
        }

        /// <inheritdoc />
        public IList<TouchPoint> ActiveTouches
        {
            get { return new List<TouchPoint>(touches); }
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

        private List<TouchPoint> touches = new List<TouchPoint>(30);
        private Dictionary<int, TouchPoint> idToTouch = new Dictionary<int, TouchPoint>(30);

        // Upcoming changes
        private List<TouchPoint> touchesBegan = new List<TouchPoint>(10);
        private HashSet<int> touchesUpdated = new HashSet<int>();
        private HashSet<int> touchesEnded = new HashSet<int>();
        private HashSet<int> touchesCancelled = new HashSet<int>();

        private static ObjectPool<TouchPoint> touchPointPool = new ObjectPool<TouchPoint>(10, null, null,
            (t) => t.INTERNAL_Reset());

        private static ObjectPool<List<TouchPoint>> touchPointListPool = new ObjectPool<List<TouchPoint>>(2,
            () => new List<TouchPoint>(10), null, (l) => l.Clear());

        private static ObjectPool<List<int>> intListPool = new ObjectPool<List<int>>(3, () => new List<int>(10), null,
            (l) => l.Clear());

        private int nextTouchId = 0;
        private object touchLock = new object();

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
        public void CancelTouch(int id, bool @return)
        {
            TouchPoint touch;
            if (idToTouch.TryGetValue(id, out touch))
            {
                touch.InputSource.CancelTouch(touch, @return);
            }
        }

        /// <inheritdoc />
        public void CancelTouch(int id)
        {
            CancelTouch(id, false);
        }

        #endregion

        #region Internal methods

        internal TouchPoint INTERNAL_BeginTouch(Vector2 position, IInputSource input)
        {
            return INTERNAL_BeginTouch(position, input, null);
        }

        internal TouchPoint INTERNAL_BeginTouch(Vector2 position, IInputSource input, Tags tags)
        {
            TouchPoint touch;
            lock (touchLock)
            {
                touch = touchPointPool.Get();
                touch.INTERNAL_Init(nextTouchId++, position, input, tags);
                touchesBegan.Add(touch);
            }
            return touch;
        }

        /// <summary>
        /// Update touch without moving it
        /// </summary>
        /// <param name="id">Touch id</param>
        internal void INTERNAL_UpdateTouch(int id)
        {
            lock (touchLock)
            {
                if (idToTouch.ContainsKey(id))
                {
                    if (!touchesUpdated.Contains(id)) touchesUpdated.Add(id);
                }
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Touch with id [" + id +
                                     "] is requested to UPDATE but no touch with such id found.");
#endif
            }
        }

        internal void INTERNAL_MoveTouch(int id, Vector2 position)
        {
            lock (touchLock)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    touch = touchesBegan.Find((t) => t.Id == id);
                    // No touch with such id
                    if (touch == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Touch with id [" + id + "] is requested to MOVE to " + position +
                                         " but no touch with such id found.");
#endif
                        return;
                    }
                }

                touch.INTERNAL_SetPosition(position);
                if (!touchesUpdated.Contains(id)) touchesUpdated.Add(id);
            }
        }

        /// <inheritdoc />
        internal void INTERNAL_EndTouch(int id)
        {
            lock (touchLock)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    touch = touchesBegan.Find((t) => t.Id == id);
                    // No touch with such id
                    if (touch == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Touch with id [" + id +
                                         "] is requested to END but no touch with such id found.");
#endif
                        return;
                    }
                }
                if (!touchesEnded.Contains(id)) touchesEnded.Add(id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Touch with id [" + id +
                                     "] is requested to END more than once this frame.");
#endif
            }
        }

        /// <inheritdoc />
        internal void INTERNAL_CancelTouch(int id)
        {
            lock (touchLock)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    touch = touchesBegan.Find((t) => t.Id == id);
                    // No touch with such id
                    if (touch == null)
                    {
#if TOUCHSCRIPT_DEBUG
                        Debug.LogWarning("TouchScript > Touch with id [" + id +
                                         "] is requested to CANCEL but no touch with such id found.");
#endif
                        return;
                    }
                }
                if (!touchesCancelled.Contains(id)) touchesCancelled.Add(touch.Id);
#if TOUCHSCRIPT_DEBUG
                else
                    Debug.LogWarning("TouchScript > Touch with id [" + id +
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

            touchPointListPool.WarmUp(2);
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
            createTouchInput();
        }

        private void Update()
        {
            updateInputs();
            updateTouches();
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
            debugTouchSize = Vector2.one*dotsPerCentimeter;
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

        private void createTouchInput()
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

        private void updateBegan(List<TouchPoint> points)
        {
            var count = points.Count;
            var list = touchPointListPool.Get();
            for (var i = 0; i < count; i++)
            {
                var touch = points[i];
                list.Add(touch);
                touches.Add(touch);
                idToTouch.Add(touch.Id, touch);

                for (var j = 0; j < layerCount; j++)
                {
                    var touchLayer = layers[j];
                    if (touchLayer == null || !touchLayer.enabled) continue;
                    if (touchLayer.INTERNAL_BeginTouch(touch)) break;
                }

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForTouch(touch);
#endif
            }

            if (touchesBeganInvoker != null)
                touchesBeganInvoker.InvokeHandleExceptions(this, TouchEventArgs.GetCachedEventArgs(list));
            touchPointListPool.Release(list);
        }

        private void updateUpdated(List<int> points)
        {
            var updatedCount = points.Count;
            var list = touchPointListPool.Get();
            // Need to loop through all touches to reset those which did not move
            var count = touches.Count;
            for (var i = 0; i < count; i++)
            {
                touches[i].INTERNAL_ResetPosition();
            }
            for (var i = 0; i < updatedCount; i++)
            {
                var id = points[i];
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id +
                                     "] was in UPDATED list but no touch with such id found.");
#endif
                    continue;
                }
                list.Add(touch);
                if (touch.Layer != null) touch.Layer.INTERNAL_UpdateTouch(touch);

#if TOUCHSCRIPT_DEBUG
                addDebugFigureForTouch(touch);
#endif
            }

            if (touchesMovedInvoker != null)
                touchesMovedInvoker.InvokeHandleExceptions(this, TouchEventArgs.GetCachedEventArgs(list));
            touchPointListPool.Release(list);
        }

        private void updateEnded(List<int> points)
        {
            var endedCount = points.Count;
            var list = touchPointListPool.Get();
            for (var i = 0; i < endedCount; i++)
            {
                var id = points[i];
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id + "] was in ENDED list but no touch with such id found.");
#endif
                    continue;
                }
                idToTouch.Remove(id);
                touches.Remove(touch);
                list.Add(touch);
                if (touch.Layer != null) touch.Layer.INTERNAL_EndTouch(touch);

#if TOUCHSCRIPT_DEBUG
                removeDebugFigureForTouch(touch);
#endif
            }

            if (touchesEndedInvoker != null)
                touchesEndedInvoker.InvokeHandleExceptions(this, TouchEventArgs.GetCachedEventArgs(list));

            for (var i = 0; i < endedCount; i++) touchPointPool.Release(list[i]);
            touchPointListPool.Release(list);
        }

        private void updateCancelled(List<int> points)
        {
            var cancelledCount = points.Count;
            var list = touchPointListPool.Get();
            for (var i = 0; i < cancelledCount; i++)
            {
                var id = points[i];
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
#if TOUCHSCRIPT_DEBUG
                    Debug.LogWarning("TouchScript > Id [" + id +
                                     "] was in CANCELLED list but no touch with such id found.");
#endif
                    continue;
                }
                idToTouch.Remove(id);
                touches.Remove(touch);
                list.Add(touch);
                if (touch.Layer != null) touch.Layer.INTERNAL_CancelTouch(touch);

#if TOUCHSCRIPT_DEBUG
                removeDebugFigureForTouch(touch);
#endif
            }

            if (touchesCancelledInvoker != null)
                touchesCancelledInvoker.InvokeHandleExceptions(this, TouchEventArgs.GetCachedEventArgs(list));

            for (var i = 0; i < cancelledCount; i++) touchPointPool.Release(list[i]);
            touchPointListPool.Release(list);
        }

        private void updateTouches()
        {
            if (frameStartedInvoker != null) frameStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);

            // need to copy buffers since they might get updated during execution
            List<TouchPoint> beganList = null;
            List<int> updatedList = null;
            List<int> endedList = null;
            List<int> cancelledList = null;
            lock (touchLock)
            {
                if (touchesBegan.Count > 0)
                {
                    beganList = touchPointListPool.Get();
                    beganList.AddRange(touchesBegan);
                    touchesBegan.Clear();
                }
                if (touchesUpdated.Count > 0)
                {
                    updatedList = intListPool.Get();
                    updatedList.AddRange(touchesUpdated);
                    touchesUpdated.Clear();
                }
                if (touchesEnded.Count > 0)
                {
                    endedList = intListPool.Get();
                    endedList.AddRange(touchesEnded);
                    touchesEnded.Clear();
                }
                if (touchesCancelled.Count > 0)
                {
                    cancelledList = intListPool.Get();
                    cancelledList.AddRange(touchesCancelled);
                    touchesCancelled.Clear();
                }
            }

            if (beganList != null)
            {
                updateBegan(beganList);
                touchPointListPool.Release(beganList);
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
        private Vector2 debugTouchSize;

        private void removeDebugFigureForTouch(TouchPoint touch)
        {
            GLDebug.RemoveFigure(TouchManager.DEBUG_GL_TOUCH + touch.Id);
        }

        private void addDebugFigureForTouch(TouchPoint touch)
        {
            GLDebug.DrawSquareScreenSpace(TouchManager.DEBUG_GL_TOUCH + touch.Id, touch.Position, 0, debugTouchSize,
                GLDebug.MULTIPLY, float.PositiveInfinity);
        }
#endif

        #endregion
    }
}