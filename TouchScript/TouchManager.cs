/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Events;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Singleton which handles all touch points management.
    /// </summary>
    [AddComponentMenu("TouchScript/Touch Manager")]
    public class TouchManager : MonoBehaviour
    {
        /// <summary>
        /// Ratio of cm to inch
        /// </summary>
        public const float CM_TO_INCH = 0.393700787f;

        /// <summary>
        /// Ratio of inch to cm
        /// </summary>
        public const float INCH_TO_CM = 1/CM_TO_INCH;

        #region Events

        /// <summary>
        /// Occurs when a new frame is started before all other events.
        /// </summary>
        public event EventHandler FrameStarted
        {
            add { frameStartedInvoker += value; }
            remove { frameStartedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a frame is finished. After all other events.
        /// </summary>
        public event EventHandler FrameFinished
        {
            add { frameFinishedInvoker += value; }
            remove { frameFinishedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when new touch points are added.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchesBegan
        {
            add { touchesBeganInvoker += value; }
            remove { touchesBeganInvoker -= value; }
        }

        /// <summary>
        /// Occurs when touch points are updated.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchesMoved
        {
            add { touchesMovedInvoker += value; }
            remove { touchesMovedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when touch points are removed.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchesEnded
        {
            add { touchesEndedInvoker += value; }
            remove { touchesEndedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when touch points are cancelled.
        /// </summary>
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

        /// <summary>
        /// TouchManager singleton instance.
        /// </summary>
        public static TouchManager Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(TouchManager)) as TouchManager;
                    if (instance == null && Application.isPlaying)
                    {
                        var go = GameObject.Find("TouchScript");
                        if (go == null) go = new GameObject("TouchScript");
                        instance = go.AddComponent<TouchManager>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Current DPI.
        /// </summary>
        public float DPI
        {
            get { return dpi; }
            set
            {
                if (Application.isEditor) EditorDPI = value;
                else LiveDPI = value;
            }
        }

        /// <summary>
        /// DPI while testing in editor.
        /// </summary>
        public float EditorDPI
        {
            get { return editorDpi; }
            set
            {
                editorDpi = value;
                updateDPI();
            }
        }

        /// <summary>
        /// DPI of target touch device.
        /// </summary>
        public float LiveDPI
        {
            get { return liveDpi; }
            set
            {
                liveDpi = value;
                updateDPI();
            }
        }

        /// <summary>
        /// List of touch layers.
        /// </summary>
        public List<TouchLayer> Layers
        {
            get { return new List<TouchLayer>(layers); }
        }

        /// <summary>
        /// Pixels in a cm with current DPI.
        /// </summary>
        public float DotsPerCentimeter
        {
            get { return CM_TO_INCH*dpi; }
        }

        /// <summary>
        /// Number of active touches.
        /// </summary>
        public int TouchPointsCount
        {
            get { return touchPoints.Count; }
        }

        /// <summary>
        /// List of active touches.
        /// </summary>
        public List<TouchPoint> TouchPoints
        {
            get { return new List<TouchPoint>(touchPoints); }
        }

        #endregion

        #region Private variables

        private static TouchManager instance;
        // Flag to indicate that we are going out of Play Mode in the editor. Otherwise there might be a loop when while deinitializing other objects access TouchScript.Instance which recreates an instance of TouchManager and everything breaks.
        private static bool shuttingDown = false;

        private float dpi = 72;

        [SerializeField]
        private float liveDpi = 72;

        [SerializeField]
        private float editorDpi = 72;

        [SerializeField]
        private List<TouchLayer> layers = new List<TouchLayer>();

        private List<TouchPoint> touchPoints = new List<TouchPoint>();
        private Dictionary<int, TouchPoint> idToTouch = new Dictionary<int, TouchPoint>();

        // Upcoming changes
        private List<TouchPoint> touchesBegan = new List<TouchPoint>();
        private Dictionary<int, Vector2> touchesMoved = new Dictionary<int, Vector2>();
        private List<TouchPoint> touchesEnded = new List<TouchPoint>();
        private List<TouchPoint> touchesCancelled = new List<TouchPoint>();

        // Temporary variables for update methods.
        private List<TouchPoint> reallyMoved = new List<TouchPoint>();

        private int nextTouchPointId = 0;

        #endregion

        #region Unity

        private void Awake()
        {
            shuttingDown = false;
            if (instance == null) instance = this;
            updateDPI();

            StartCoroutine(lateAwake());
        }

        private IEnumerator lateAwake()
        {
            yield return new WaitForEndOfFrame();

            layers = layers.FindAll(l => l != null); // filter empty ones
            var unknownLayers = FindObjectsOfType(typeof(TouchLayer));
            foreach (TouchLayer unknownLayer in unknownLayers) AddLayer(unknownLayer);

            createCameraLayer();
            createTouchInput();
        }

        private void Update()
        {
            updateTouchPoints();
        }

        private void OnDestroy()
        {
            if (!Application.isLoadingLevel) shuttingDown = true;
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Adds a layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>True if layer was added.</returns>
        public static bool AddLayer(TouchLayer layer)
        {
            if (shuttingDown) return false;
            if (layer == null) return false;
            if (Instance == null) return false;
            if (Instance.layers.Contains(layer)) return false;
            Instance.layers.Add(layer);
            return true;
        }

        /// <summary>
        /// Removes a layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>True if layer was removed.</returns>
        public static bool RemoveLayer(TouchLayer layer)
        {
            if (shuttingDown) return false;
            if (layer == null) return false;
            if (instance == null) return false;
            var result = instance.layers.Remove(layer);
            return result;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Swaps layers in sorted array.
        /// </summary>
        /// <param name="at">Layer index 1.</param>
        /// <param name="to">Layer index 2</param>
        public void ChangeLayerIndex(int at, int to)
        {
            if (at < 0 || at >= layers.Count) return;
            if (to < 0 || to >= layers.Count) return;
            var data = layers[at];
            layers.RemoveAt(at);
            layers.Insert(to, data);
        }

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="position">Touch screen position.</param>
        /// <returns>Object's transform which has been hit or null otherwise.</returns>
        public Transform GetHitTarget(Vector2 position)
        {
            TouchHit hit;
            TouchLayer layer;
            if (GetHitTarget(position, out hit, out layer)) return hit.Transform;
            return null;
        }

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="position">Touch point screen position.</param>
        /// <param name="hit">Output RaycastHit.</param>
        /// <param name="layer">Output touch layer which was hit.</param>
        /// <returns>True if something was hit.</returns>
        public bool GetHitTarget(Vector2 position, out TouchHit hit, out TouchLayer layer)
        {
            hit = new TouchHit();
            layer = null;

            foreach (var touchLayer in layers)
            {
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

        /// <summary>
        /// Registers a touch.
        /// </summary>
        /// <param name="position">Touch position.</param>
        /// <returns>Internal id of the new touch.</returns>
        public int BeginTouch(Vector2 position)
        {
            TouchPoint touch;
            lock (touchesBegan)
            {
                touch = new TouchPoint(nextTouchPointId++, position);
                touchesBegan.Add(touch);
            }
            return touch.Id;
        }

        /// <summary>
        /// Moves a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        /// <param name="position">New position.</param>
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

        /// <summary>
        /// Ends a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        public void EndTouch(int id)
        {
            lock (touchesEnded)
            {
                TouchPoint touch;
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

        /// <summary>
        /// Cancels a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        public void CancelTouch(int id)
        {
            lock (touchesCancelled)
            {
                TouchPoint touch;
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

        #region Internal methods

        #endregion

        #region Private functions

        private void updateDPI()
        {
            if (Application.isEditor) dpi = EditorDPI;
            else dpi = LiveDPI;
        }

        private void createCameraLayer()
        {
            if (layers.Count == 0)
            {
                Debug.Log("No camera layers. Adding one for the main camera.");
                if (Camera.main != null)
                {
                    Camera.main.gameObject.AddComponent<CameraLayer>();
                } else
                {
                    Debug.LogError("No main camera found!");
                }
            }
        }

        private void createTouchInput()
        {
            var inputs = FindObjectsOfType(typeof(InputSource));
            if (inputs.Length == 0)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
                {
                    gameObject.AddComponent<MobileInput>();
                } else
                {
                    gameObject.AddComponent<MouseInput>();
                }
            }
        }

        private bool updateBegan()
        {
            if (touchesBegan.Count > 0)
            {
                foreach (var touch in touchesBegan)
                {
                    touchPoints.Add(touch);
                    idToTouch.Add(touch.Id, touch);
                    foreach (var touchLayer in layers)
                    {
                        if (touchLayer == null) continue;
                        if (touchLayer.BeginTouch(touch)) break;
                    }
                }

                if (touchesBeganInvoker != null) touchesBeganInvoker(this, new TouchEventArgs(new List<TouchPoint>(touchesBegan)));
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

                foreach (var touch in touchPoints)
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

                if (reallyMoved.Count > 0 && touchesMovedInvoker != null) touchesMovedInvoker(this, new TouchEventArgs(new List<TouchPoint>(reallyMoved)));
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
                    touchPoints.Remove(touch);
                    if (touch.Layer != null) touch.Layer.EndTouch(touch);
                }

                if (touchesEndedInvoker != null) touchesEndedInvoker(this, new TouchEventArgs(new List<TouchPoint>(touchesEnded)));
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
                    touchPoints.Remove(touch);
                    if (touch.Layer != null) touch.Layer.CancelTouch(touch);
                }

                if (touchesCancelledInvoker != null) touchesCancelledInvoker(this, new TouchEventArgs(new List<TouchPoint>(touchesCancelled)));
                touchesCancelled.Clear();

                return true;
            }
            return false;
        }

        private void updateTouchPoints()
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