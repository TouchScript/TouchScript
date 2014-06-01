/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouchScript.Devices.Display;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Default implementation of <see cref="ITouchManager"/>.
    /// </summary>
    internal sealed class TouchManagerInstance : MonoBehaviour, ITouchManager
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

        // iOS Events AOT hack
        private EventHandler<TouchEventArgs> touchesBeganInvoker, touchesMovedInvoker,
            touchesEndedInvoker, touchesCancelledInvoker;

        private EventHandler frameStartedInvoker, frameFinishedInvoker;

        #endregion

        #region Public properties

        public static ITouchManager Instance
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
                        go.hideFlags = HideFlags.HideInHierarchy;
                        DontDestroyOnLoad(go);
                        instance = go.AddComponent<TouchManagerInstance>();
                    } else if (objects.Length >= 1)
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
                } else
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
        public IList<TouchLayer> Layers
        {
            get { return layers.AsReadOnly(); }
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

        private IDisplayDevice displayDevice;
        private float dpi = 96;
        private float dotsPerCentimeter = TouchManager.CM_TO_INCH*96;

        private List<TouchLayer> layers = new List<TouchLayer>(5);
        private List<TouchPoint> touches = new List<TouchPoint>(10);
        private Dictionary<int, ITouch> idToTouch = new Dictionary<int, ITouch>(10);

        // Upcoming changes
        private List<ITouch> touchesBegan = new List<ITouch>(10);
        private Dictionary<int, Vector2> touchesMoved = new Dictionary<int, Vector2>(10);
        private List<ITouch> touchesEnded = new List<ITouch>(10);
        private List<ITouch> touchesCancelled = new List<ITouch>(10);

        // Temporary variables for update methods.
        private List<ITouch> reallyMoved = new List<ITouch>(10);

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
            } else
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

            foreach (var touchLayer in layers)
            {
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

        public int BeginTouch(Vector2 position)
        {
            return BeginTouch(position, null, null);
        }

        public int BeginTouch(Vector2 position, Tags tags)
        {
            return BeginTouch(position, tags, null);
        }

        /// <inheritdoc />
        public int BeginTouch(Vector2 position, Tags tags, IDictionary<string, System.Object> properties)
        {
            TouchPoint touch;
            lock (touchesBegan)
            {
                touch = new TouchPoint(nextTouchId++, position, tags, properties);
                touchesBegan.Add(touch);
            }
            return touch.Id;
        }

        /// <inheritdoc />
        public void MoveTouch(int id, Vector2 position)
        {
            lock (touchesMoved)
            {
                Vector2 update;
                if (touchesMoved.TryGetValue(id, out update))
                {
                    touchesMoved[id] = position;
                } else
                {
                    touchesMoved.Add(id, position);
                }
            }
        }

        /// <inheritdoc />
        public void EndTouch(int id)
        {
            lock (touchesEnded)
            {
                ITouch touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    foreach (var addedTouch in touchesBegan)
                    {
                        if (addedTouch.Id == id)
                        {
                            touch = addedTouch;
                            break;
                        }
                    }
                    // No touch with such id
                    if (touch == null) return;
                }
                touchesEnded.Add(touch);
            }
        }

        /// <inheritdoc />
        public void CancelTouch(int id)
        {
            lock (touchesCancelled)
            {
                ITouch touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    // This touch was added this frame
                    foreach (var addedTouch in touchesBegan)
                    {
                        if (addedTouch.Id == id)
                        {
                            touch = addedTouch;
                            break;
                        }
                    }
                    // No touch with such id
                    if (touch == null) return;
                }
                touchesCancelled.Add(touch);
            }
        }

        #endregion

        #region Unity

        private void Awake()
        {
            if (instance == null) instance = this;

            updateDPI();

            StopAllCoroutines();
            StartCoroutine("lateAwake");
        }

        private void OnLevelWasLoaded(int value)
        {
            StopAllCoroutines();
            StartCoroutine("lateAwake");
        }

        private IEnumerator lateAwake()
        {
            yield return new WaitForEndOfFrame();

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
            dotsPerCentimeter = TouchManager.CM_TO_INCH*dpi;
        }

        private void updateLayers()
        {
            layers = layers.FindAll(l => l != null); // filter empty ones
            var unknownLayers = FindObjectsOfType(typeof(TouchLayer));
            foreach (TouchLayer unknownLayer in unknownLayers) AddLayer(unknownLayer);
        }

        private void createCameraLayer()
        {
            if (layers.Count == 0)
            {
                Debug.Log("No camera layers. Adding one for the main camera.");
                if (Camera.main != null) Camera.main.gameObject.AddComponent<CameraLayer>();
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

        private bool updateBegan()
        {
            if (touchesBegan.Count > 0)
            {
                foreach (var touch in touchesBegan)
                {
                    touches.Add(touch as TouchPoint);
                    idToTouch.Add(touch.Id, touch);
                    foreach (var touchLayer in layers)
                    {
                        if (touchLayer == null) continue;
                        if (touchLayer.BeginTouch(touch as TouchPoint)) break;
                    }
                }

                if (touchesBeganInvoker != null) touchesBeganInvoker(this, new TouchEventArgs(touchesBegan));
                touchesBegan.Clear();
                return true;
            }
            return false;
        }

        private bool updateMoved()
        {
            if (touchesMoved.Count > 0)
            {
                reallyMoved.Clear();

                foreach (var touch in touches)
                {
                    if (touchesMoved.ContainsKey(touch.Id))
                    {
                        var position = touchesMoved[touch.Id];
                        if (position != touch.Position)
                        {
                            touch.Position = position;
                            reallyMoved.Add(touch);
                            if (touch.Layer != null) touch.Layer.MoveTouch(touch);
                        } else
                        {
                            touch.ResetPosition();
                        }
                    } else
                    {
                        touch.ResetPosition();
                    }
                }

                if (reallyMoved.Count > 0 && touchesMovedInvoker != null) touchesMovedInvoker(this, new TouchEventArgs(new List<ITouch>(reallyMoved)));
                touchesMoved.Clear();

                return reallyMoved.Count > 0;
            }
            return false;
        }

        private bool updateEnded()
        {
            if (touchesEnded.Count > 0)
            {
                foreach (var touch in touchesEnded)
                {
                    idToTouch.Remove(touch.Id);
                    touches.Remove(touch as TouchPoint);
                    if (touch.Layer != null) touch.Layer.EndTouch(touch);
                }

                if (touchesEndedInvoker != null) touchesEndedInvoker(this, new TouchEventArgs(new List<ITouch>(touchesEnded)));
                touchesEnded.Clear();

                return true;
            }
            return false;
        }

        private bool updateCancelled()
        {
            if (touchesCancelled.Count > 0)
            {
                foreach (var touch in touchesCancelled)
                {
                    idToTouch.Remove(touch.Id);
                    touches.Remove(touch as TouchPoint);
                    if (touch.Layer != null) touch.Layer.CancelTouch(touch);
                }

                if (touchesCancelledInvoker != null) touchesCancelledInvoker(this, new TouchEventArgs(new List<ITouch>(touchesCancelled)));
                touchesCancelled.Clear();

                return true;
            }
            return false;
        }

        private void updateTouches()
        {
            if (frameStartedInvoker != null) frameStartedInvoker(this, EventArgs.Empty);

            bool updated;
            lock (touchesBegan) updated = updateBegan();
            lock (touchesMoved) updated = updateMoved() || updated;
            lock (touchesEnded) updated = updateEnded() || updated;
            lock (touchesCancelled) updated = updateCancelled() || updated;

            if (frameFinishedInvoker != null) frameFinishedInvoker(this, EventArgs.Empty);
        }

        #endregion
    }
}