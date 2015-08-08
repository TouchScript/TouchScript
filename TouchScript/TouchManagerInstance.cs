/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TouchScript.Devices.Display;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using TouchScript.Utils;
#if DEBUG
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
        private EventHandler<TouchEventArgs> touchesBeganInvoker, touchesMovedInvoker,
            touchesEndedInvoker, touchesCancelledInvoker;

        private EventHandler frameStartedInvoker, frameFinishedInvoker;

        #endregion

        #region Public properties

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
        public IList<TouchLayer> Layers
        {
            get { return new ReadOnlyCollection<TouchLayer>(layers); }
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
        public IList<ITouch> ActiveTouches
        {
            get { return touches.Cast<ITouch>().ToList(); }
        }

        #endregion

        #region Private variables

        private static bool shuttingDown = false;
        private static TouchManagerInstance instance;
        private Boolean shouldCreateCameraLayer = true;

        private IDisplayDevice displayDevice;
        private float dpi = 96;
        private float dotsPerCentimeter = TouchManager.CM_TO_INCH * 96;

        private List<TouchLayer> layers = new List<TouchLayer>(10);
        private List<TouchPoint> touches = new List<TouchPoint>(30);
        private Dictionary<int, TouchPoint> idToTouch = new Dictionary<int, TouchPoint>(30);

        // Upcoming changes
        private List<TouchPoint> touchesBegan = new List<TouchPoint>(30);
        private List<int> touchesUpdated = new List<int>(30);
        private List<int> touchesEnded = new List<int>(30);
        private List<int> touchesCancelled = new List<int>(30);
        private List<ITouch> tmpList_ITouch = new List<ITouch>(30);

        private int nextTouchId = 0;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public bool AddLayer(TouchLayer layer)
        {
            if (layer == null) return false;
            if (layers.Contains(layer)) return true;
            layers.Add(layer);
            return true;
        }

        /// <inheritdoc />
        public bool AddLayer(TouchLayer layer, int index)
        {
            if (layer == null) return false;
            if (index >= layers.Count) return AddLayer(layer);
            var i = layers.IndexOf(layer);
            if (i == -1)
            {
                layers.Insert(index, layer);
            }
            else
            {
                if (index == i || i == index - 1) return true;
                layers.RemoveAt(i);
                if (index < i) layers.Insert(index, layer);
                else layers.Insert(index - 1, layer);
            }
            return true;
        }

        /// <inheritdoc />
        public bool RemoveLayer(TouchLayer layer)
        {
            if (layer == null) return false;
            var result = layers.Remove(layer);
            return result;
        }

        /// <inheritdoc />
        public void ChangeLayerIndex(int at, int to)
        {
            if (at < 0 || at >= layers.Count) return;
            if (to < 0 || to >= layers.Count) return;
            var data = layers[at];
            layers.RemoveAt(at);
            layers.Insert(to, data);
        }

        /// <inheritdoc />
        public Transform GetHitTarget(Vector2 position)
        {
            ITouchHit hit;
            TouchLayer layer;
            if (GetHitTarget(position, out hit, out layer)) return hit.Transform;
            return null;
        }

        /// <inheritdoc />
        public bool GetHitTarget(Vector2 position, out ITouchHit hit)
        {
            TouchLayer layer;
            return GetHitTarget(position, out hit, out layer);
        }

        /// <inheritdoc />
        public bool GetHitTarget(Vector2 position, out ITouchHit hit, out TouchLayer layer)
        {
            hit = null;
            layer = null;

            var count = layers.Count;
            for (var i = 0; i < count; i++)
            {
                var touchLayer = layers[i];
                if (touchLayer == null) continue;
                ITouchHit _hit;
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
        public void CancelTouch(int id, bool redispatch = false)
        {
            INTERNAL_CancelTouch(id);
        }

        #endregion

        #region Internal methods

        internal ITouch INTERNAL_BeginTouch(Vector2 position)
        {
            return INTERNAL_BeginTouch(position, null);
        }

        internal ITouch INTERNAL_BeginTouch(Vector2 position, Tags tags)
        {
            TouchPoint touch;
            lock (touchesBegan)
            {
                touch = new TouchPoint(nextTouchId++, position, tags);
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
            lock (touchesUpdated)
            {
                if (idToTouch.ContainsKey(id)) touchesUpdated.Add(id);
            }
        }

        internal void INTERNAL_MoveTouch(int id, Vector2 position)
        {
            lock (touchesUpdated)
            {
                TouchPoint touch;
                if (idToTouch.TryGetValue(id, out touch))
                {
                    touch.INTERNAL_SetPosition(position);
                    touchesUpdated.Add(id);
                }
            }
        }

        /// <inheritdoc />
        internal void INTERNAL_EndTouch(int id)
        {
            lock (touchesEnded)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    touch = touchesBegan.Find((t) => t.Id == id);
                    // No touch with such id
                    if (touch == null) return;
                }
                touchesEnded.Add(touch.Id);
            }
        }

        /// <inheritdoc />
        internal void INTERNAL_CancelTouch(int id)
        {
            lock (touchesCancelled)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    touch = touchesBegan.Find((t) => t.Id == id);
                    // No touch with such id
                    if (touch == null) return;
                }
                touchesCancelled.Add(touch.Id);
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
            StartCoroutine("lateAwake");

#if DEBUG
            DebugMode = true;
#endif
        }

        private void OnLevelWasLoaded(int value)
        {
            StopAllCoroutines();
            StartCoroutine("lateAwake");
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
#if DEBUG
            debugTouchSize = Vector2.one * dotsPerCentimeter;
#endif
        }

        private void updateLayers()
        {
            // filter empty layers
            layers = layers.FindAll(l => l != null);
        }

        private void createCameraLayer()
        {
            if (layers.Count == 0 && shouldCreateCameraLayer)
            {
                Debug.LogWarning("No camera layers, adding CameraLayer for the main camera. (this message is harmless)");
                if (Camera.main != null)
                {
                    var layer = Camera.main.GetComponent<TouchLayer>();
                    if (layer == null) layer = Camera.main.gameObject.AddComponent<CameraLayer>();
                    AddLayer(layer);
                }
            }
        }

        private void createTouchInput()
        {
            var inputs = FindObjectsOfType(typeof(InputSource));
            if (inputs.Length == 0)
            {
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

                switch (Application.platform)
                {
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.Android:
                    case RuntimePlatform.BlackBerryPlayer:
                    case RuntimePlatform.MetroPlayerARM:
                    case RuntimePlatform.MetroPlayerX64:
                    case RuntimePlatform.MetroPlayerX86:
                    case RuntimePlatform.WP8Player:
                        obj.AddComponent<MobileInput>();
                        break;
                    default:
                        obj.AddComponent<MouseInput>();
                        break;
                }
            }
        }

        private void updateBegan()
        {
            var count = touchesBegan.Count;
            if (count > 0)
            {
                tmpList_ITouch.Clear();
                for (var i = 0; i < count; i++)
                {
                    var touch = touchesBegan[i];
                    tmpList_ITouch.Add(touch);
                    touches.Add(touch);
                    idToTouch.Add(touch.Id, touch);

                    var layerCount = layers.Count;
                    for (var j = 0; j< layerCount; j++)
                    {
                        var touchLayer = Layers[j];
                        if (touchLayer == null) continue;
                        if (touchLayer.INTERNAL_BeginTouch(touch)) break;
                    }

#if DEBUG
                    addDebugFigureForTouch(touch);
#endif

                }

                if (touchesBeganInvoker != null) touchesBeganInvoker.InvokeHandleExceptions(this, new TouchEventArgs(tmpList_ITouch));
                touchesBegan.Clear();
            }
        }

        private void updateUpdated()
        {
            var updatedCount = touchesUpdated.Count;
            if (updatedCount > 0)
            {
                tmpList_ITouch.Clear();
                // Need to loop through all touches to reset those which did not move
                var count = touches.Count;
                for (var i = 0; i < count; i++)
                {
                    touches[i].INTERNAL_ResetPosition();
                }
                for (var i = 0; i < updatedCount; i++)
                {
                    var id = touchesUpdated[i];
                    var touch = idToTouch[id];
                    tmpList_ITouch.Add(touch);
                    if (touch.Layer != null) touch.Layer.INTERNAL_UpdateTouch(touch);

#if DEBUG
                    addDebugFigureForTouch(touch);
#endif

                }

                if (touchesMovedInvoker != null) touchesMovedInvoker.InvokeHandleExceptions(this, new TouchEventArgs(tmpList_ITouch));
                touchesUpdated.Clear();
            }
        }

        private void updateEnded()
        {
            var endedCount = touchesEnded.Count;
            if (endedCount > 0)
            {
                tmpList_ITouch.Clear();
                for (var i = 0; i < endedCount; i++)
                {
                    var id = touchesEnded[i];
                    var touch = idToTouch[id];
                    idToTouch.Remove(id);
                    touches.Remove(touch);
                    tmpList_ITouch.Add(touch);
                    if (touch.Layer != null) touch.Layer.INTERNAL_EndTouch(touch);

#if DEBUG
                    removeDebugFigureForTouch(touch);
#endif
                }

                if (touchesEndedInvoker != null) touchesEndedInvoker.InvokeHandleExceptions(this, new TouchEventArgs(tmpList_ITouch));
                touchesEnded.Clear();
            }
        }

        private void updateCancelled()
        {
            var cancelledCount = touchesCancelled.Count;
            if (touchesCancelled.Count > 0)
            {
                tmpList_ITouch.Clear();
                for (var i = 0; i < cancelledCount; i++)
                {
                    var id = touchesCancelled[i];
                    var touch = idToTouch[id];
                    idToTouch.Remove(id);
                    touches.Remove(touch);
                    tmpList_ITouch.Add(touch);
                    if (touch.Layer != null) touch.Layer.INTERNAL_CancelTouch(touch);

#if DEBUG
                    removeDebugFigureForTouch(touch);
#endif
                }

                if (touchesCancelledInvoker != null) touchesCancelledInvoker.InvokeHandleExceptions(this, new TouchEventArgs(tmpList_ITouch));
                touchesCancelled.Clear();
            }
        }

        private void updateTouches()
        {
            if (frameStartedInvoker != null) frameStartedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);

            lock (touchesBegan) updateBegan();
            lock (touchesUpdated) updateUpdated();
            lock (touchesEnded) updateEnded();
            lock (touchesCancelled) updateCancelled();

            if (frameFinishedInvoker != null) frameFinishedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
        }

#if DEBUG
        private Vector2 debugTouchSize;

        private void removeDebugFigureForTouch(ITouch touch)
        {
            GLDebug.RemoveFigure(TouchManager.DEBUG_GL_TOUCH + touch.Id);
        }

        private void addDebugFigureForTouch(ITouch touch)
        {
            GLDebug.DrawSquareScreenSpace(TouchManager.DEBUG_GL_TOUCH + touch.Id, touch.Position, 0, debugTouchSize, GLDebug.MULTIPLY, float.PositiveInfinity);
        }
#endif

        #endregion
    }
}
