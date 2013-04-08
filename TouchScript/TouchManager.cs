/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Events;
using TouchScript.Gestures;
using TouchScript.InputSources;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Singleton which handles all touch and gesture management.
    /// Shouldn't be instantiated manually.
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
        /// Occurs when new touch points are added.
        /// </summary>
        /// 
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
        /// Active cameras to look for touch targets in specific order.
        /// </summary>
        public List<Camera> HitCameras { get; set; }

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

        public List<TouchLayer> Layers
        {
            get { return new List<TouchLayer>(layers); }
        }

        /// <summary>
        /// Radius of single touch point on device in cm.
        /// </summary>
        public float TouchRadius
        {
            get { return touchRadius; }
            set { touchRadius = value; }
        }

        /// <summary>
        /// Touch point radius in pixels.
        /// </summary>
        public float PixelTouchRadius
        {
            get { return touchRadius*DotsPerCentimeter; }
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
        public int TouchesCount
        {
            get { return touches.Count; }
        }

        /// <summary>
        /// List of active touches.
        /// </summary>
        public List<TouchPoint> Touches
        {
            get { return new List<TouchPoint>(touches); }
        }

        #endregion

        #region private Variables

        private static TouchManager instance;
        private static bool shuttingDown = false;

        private float dpi = 72;

        [SerializeField]
        private float liveDpi = 72;

        [SerializeField]
        private float editorDpi = 72;

        [SerializeField]
        private float touchRadius = .75f;

        [SerializeField]
        private List<TouchLayer> layers = new List<TouchLayer>();

        private List<TouchPoint> touches = new List<TouchPoint>();
        private Dictionary<int, TouchPoint> idToTouch = new Dictionary<int, TouchPoint>();

        // Upcoming changes
        private List<TouchPoint> touchesBegan = new List<TouchPoint>();
        private List<TouchPoint> touchesEnded = new List<TouchPoint>();
        private List<TouchPoint> touchesCancelled = new List<TouchPoint>();
        private Dictionary<int, Vector2> touchesMoved = new Dictionary<int, Vector2>();
        private List<Gesture> gesturesToReset = new List<Gesture>();

        // Locks
        private readonly object sync = new object();

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
            updateTouches();
        }

        private void OnDestroy()
        {
            shuttingDown = true;
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Public static methods

        public static bool AddLayer(TouchLayer layer)
        {
            if (shuttingDown) return false;
            if (layer == null) return false;
            if (Instance == null) return false;
            if (Instance.layers.Contains(layer)) return false;
            Instance.layers.Add(layer);
            return true;
        }

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
        /// <param name="touch">The touch.</param>
        /// <returns>Object's transform which has been hit or null otherwise.</returns>
        public Transform GetHitTarget(Vector2 position)
        {
            RaycastHit hit;
            TouchLayer layer;
            if (GetHitTarget(position, out hit, out layer)) return hit.transform;
            return null;
        }

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <param name="hit">Output RaycastHit.</param>
        /// <param name="hitCamera">Output camera which was used to hit an object.</param>
        /// <returns>Object's transform which has been hit or null otherwise.</returns>
        public bool GetHitTarget(Vector2 position, out RaycastHit hit, out TouchLayer layer)
        {
            hit = new RaycastHit();
            layer = null;

            foreach (var touchLayer in layers)
            {
                if (touchLayer == null) continue;
                RaycastHit _hit;
                if (touchLayer.Hit(position, out _hit) == HitResult.Hit)
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
            lock (sync)
            {
                touch = new TouchPoint(nextTouchPointId++, position);
                touchesBegan.Add(touch);
            }
            return touch.Id;
        }

        /// <summary>
        /// Ends a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        public void EndTouch(int id)
        {
            lock (sync)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    foreach (var addedTouch in touchesBegan)
                    {
                        if (addedTouch.Id == id)
                        {
                            touch = addedTouch;
                            break;
                        }
                    }
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
            lock (sync)
            {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch))
                {
                    foreach (var addedTouch in touchesBegan)
                    {
                        if (addedTouch.Id == id)
                        {
                            touch = addedTouch;
                            break;
                        }
                    }
                    if (touch == null) return;
                }
                touchesCancelled.Add(touch);
            }
        }

        /// <summary>
        /// Moves a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        /// <param name="position">New position.</param>
        public void MoveTouch(int id, Vector2 position)
        {
            lock (sync)
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

        #endregion

        #region Internal methods

        internal Gesture.GestureState GestureChangeState(Gesture gesture, Gesture.GestureState state)
        {
            switch (state)
            {
                case Gesture.GestureState.Possible:
                    break;
                case Gesture.GestureState.Began:
                    switch (gesture.State)
                    {
                        case Gesture.GestureState.Possible:
                            break;
                        default:
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", gesture, state, gesture.State));
                            break;
                    }
                    if (gestureCanRecognize(gesture))
                    {
                        recognizeGesture(gesture);
                    } else
                    {
                        if (!gesturesToReset.Contains(gesture)) gesturesToReset.Add(gesture);
                        return Gesture.GestureState.Failed;
                    }
                    break;
                case Gesture.GestureState.Changed:
                    switch (gesture.State)
                    {
                        case Gesture.GestureState.Began:
                        case Gesture.GestureState.Changed:
                            break;
                        default:
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", gesture, state, gesture.State));
                            break;
                    }
                    break;
                case Gesture.GestureState.Failed:
                    if (!gesturesToReset.Contains(gesture)) gesturesToReset.Add(gesture);
                    break;
                case Gesture.GestureState.Recognized: // Ended
                    if (!gesturesToReset.Contains(gesture)) gesturesToReset.Add(gesture);
                    switch (gesture.State)
                    {
                        case Gesture.GestureState.Possible:
                            if (gestureCanRecognize(gesture))
                            {
                                recognizeGesture(gesture);
                            } else
                            {
                                return Gesture.GestureState.Failed;
                            }
                            break;
                        case Gesture.GestureState.Began:
                        case Gesture.GestureState.Changed:
                            break;
                        default:
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", gesture, state, gesture.State));
                            break;
                    }
                    break;
                case Gesture.GestureState.Cancelled:
                    if (!gesturesToReset.Contains(gesture)) gesturesToReset.Add(gesture);
                    break;
            }

            return state;
        }

        internal void IgnoreTouch(TouchPoint touch)
        {}

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
                // get touches per target
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                foreach (var touch in touchesBegan)
                {
                    touches.Add(touch);
                    idToTouch.Add(touch.Id, touch);
                    if (checkLayers(touch))
                    {
                        if (touch.Target != null)
                        {
                            List<TouchPoint> list;
                            if (!targetTouches.TryGetValue(touch.Target, out list))
                            {
                                list = new List<TouchPoint>();
                                targetTouches.Add(touch.Target, list);
                            }
                            list.Add(touch);
                        }
                    }
                }

                // get touches per gesture
                // touches can come to a gesture from multiple targets in hierarchy
                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                var activeGestures = new List<Gesture>(); // no order in dictionary
                foreach (var target in targetTouches.Keys)
                {
                    var mightBeActiveGestures = getHierarchyContaining(target);
                    var possibleGestures = getHierarchyEndingWith(target);
                    foreach (var gesture in possibleGestures)
                    {
                        if (!gestureIsActive(gesture)) continue;

                        var canReceiveTouches = true;
                        foreach (var activeGesture in mightBeActiveGestures)
                        {
                            if (gesture == activeGesture) continue;
                            if ((activeGesture.State == Gesture.GestureState.Began || activeGesture.State == Gesture.GestureState.Changed) && (activeGesture.CanPreventGesture(gesture)))
                            {
                                canReceiveTouches = false;
                                break;
                            }
                        }
                        if (canReceiveTouches)
                        {
                            var touchesToReceive =
                                targetTouches[target].FindAll((TouchPoint touch) => gesture.ShouldReceiveTouch(touch));
                            if (touchesToReceive.Count > 0)
                            {
                                if (gestureTouches.ContainsKey(gesture))
                                {
                                    gestureTouches[gesture].AddRange(touchesToReceive);
                                } else
                                {
                                    activeGestures.Add(gesture);
                                    gestureTouches.Add(gesture, touchesToReceive);
                                }
                            }
                        }
                    }
                }

                foreach (var gesture in activeGestures)
                {
                    if (gestureIsActive(gesture)) gesture.TouchesBegan(gestureTouches[gesture]);
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
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                var reallyMoved = new List<TouchPoint>();

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

                            if (touch.Target != null)
                            {
                                List<TouchPoint> list;
                                if (!targetTouches.TryGetValue(touch.Target, out list))
                                {
                                    list = new List<TouchPoint>();
                                    targetTouches.Add(touch.Target, list);
                                }
                                list.Add(touch);
                            }
                        } else
                        {
                            touch.Position = touch.Position;
                        }
                    }
                }

                if (reallyMoved.Count > 0)
                {
                    var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                    var activeGestures = new List<Gesture>(); // no order in dictionary
                    foreach (var target in targetTouches.Keys)
                    {
                        var possibleGestures = getHierarchyEndingWith(target);

                        foreach (var gesture in possibleGestures)
                        {
                            if (!gestureIsActive(gesture)) continue;

                            var touchesToReceive =
                                targetTouches[target].FindAll(gesture.HasTouchPoint);
                            if (touchesToReceive.Count > 0)
                            {
                                if (gestureTouches.ContainsKey(gesture))
                                {
                                    gestureTouches[gesture].AddRange(touchesToReceive);
                                } else
                                {
                                    activeGestures.Add(gesture);
                                    gestureTouches.Add(gesture, touchesToReceive);
                                }
                            }
                        }
                    }

                    foreach (var gesture in activeGestures)
                    {
                        if (gestureIsActive(gesture)) gesture.TouchesMoved(gestureTouches[gesture]);
                    }

                    if (touchesMovedInvoker != null) touchesMovedInvoker(this, new TouchEventArgs(new List<TouchPoint>(reallyMoved)));
                }
                touchesMoved.Clear();

                return reallyMoved.Count > 0;
            }
            return false;
        }

        private bool updateEnded()
        {
            if (touchesEnded.Count > 0)
            {
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                foreach (var touch in touchesEnded)
                {
                    idToTouch.Remove(touch.Id);
                    touches.Remove(touch);
                    if (touch.Layer != null) touch.Layer.EndTouch(touch);

                    if (touch.Target != null)
                    {
                        List<TouchPoint> list;
                        if (!targetTouches.TryGetValue(touch.Target, out list))
                        {
                            list = new List<TouchPoint>();
                            targetTouches.Add(touch.Target, list);
                        }
                        list.Add(touch);
                    }
                }

                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                var activeGestures = new List<Gesture>(); // no order in dictionary
                foreach (var target in targetTouches.Keys)
                {
                    var possibleGestures = getHierarchyEndingWith(target);
                    foreach (var gesture in possibleGestures)
                    {
                        if (!gestureIsActive(gesture)) continue;

                        var touchesToReceive =
                            targetTouches[target].FindAll(gesture.HasTouchPoint);
                        if (touchesToReceive.Count > 0)
                        {
                            if (gestureTouches.ContainsKey(gesture))
                            {
                                gestureTouches[gesture].AddRange(touchesToReceive);
                            } else
                            {
                                activeGestures.Add(gesture);
                                gestureTouches.Add(gesture, touchesToReceive);
                            }
                        }
                    }
                }

                foreach (var gesture in activeGestures)
                {
                    if (gestureIsActive(gesture)) gesture.TouchesEnded(gestureTouches[gesture]);
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
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                foreach (var touch in touchesCancelled)
                {
                    idToTouch.Remove(touch.Id);
                    touches.Remove(touch);
                    if (touch.Layer != null) touch.Layer.CancelTouch(touch);

                    if (touch.Target != null)
                    {
                        List<TouchPoint> list;
                        if (!targetTouches.TryGetValue(touch.Target, out list))
                        {
                            list = new List<TouchPoint>();
                            targetTouches.Add(touch.Target, list);
                        }
                        list.Add(touch);
                    }
                }

                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                var activeGestures = new List<Gesture>(); // no order in dictionary
                foreach (var target in targetTouches.Keys)
                {
                    var possibleGestures = getHierarchyEndingWith(target);
                    foreach (var gesture in possibleGestures)
                    {
                        if (!gestureIsActive(gesture)) continue;

                        var touchesToReceive =
                            targetTouches[target].FindAll(gesture.HasTouchPoint);
                        if (touchesToReceive.Count > 0)
                        {
                            if (gestureTouches.ContainsKey(gesture))
                            {
                                gestureTouches[gesture].AddRange(touchesToReceive);
                            } else
                            {
                                activeGestures.Add(gesture);
                                gestureTouches.Add(gesture, touchesToReceive);
                            }
                        }
                    }
                }

                foreach (var gesture in activeGestures)
                {
                    if (gestureIsActive(gesture)) gesture.TouchesCancelled(gestureTouches[gesture]);
                }

                if (touchesCancelledInvoker != null) touchesCancelledInvoker(this, new TouchEventArgs(new List<TouchPoint>(touchesCancelled)));
                touchesCancelled.Clear();

                return true;
            }
            return false;
        }

        private void updateTouches()
        {
            // reset gestures changed between update loops
            resetGestures();
            bool updated;
            lock (sync)
            {
                updated = updateBegan();
                updated = updateMoved() || updated;
                updated = updateEnded() || updated;
                updated = updateCancelled() || updated;
            }

            if (updated) resetGestures();
        }

        private void resetGestures()
        {
            foreach (var gesture in gesturesToReset)
            {
                gesture.Reset();
                gesture.SetState(Gesture.GestureState.Possible);
            }
            gesturesToReset.Clear();
        }

        private List<Gesture> getHierarchyEndingWith(Transform target)
        {
            var hierarchy = new List<Gesture>();
            while (target != null)
            {
                hierarchy.AddRange(getEnabledGesturesOnTarget(target));
                target = target.parent;
            }
            return hierarchy;
        }

        private List<Gesture> getHierarchyBeginningWith(Transform target, bool includeSelf = true)
        {
            var hierarchy = new List<Gesture>();
            if (includeSelf)
            {
                hierarchy.AddRange(getEnabledGesturesOnTarget(target));
            }
            foreach (Transform child in target)
            {
                hierarchy.AddRange(getHierarchyBeginningWith(child));
            }
            return hierarchy;
        }

        private List<Gesture> getHierarchyContaining(Transform target)
        {
            var hierarchy = getHierarchyEndingWith(target);
            hierarchy.AddRange(getHierarchyBeginningWith(target, false));
            return hierarchy;
        }

        private List<Gesture> getEnabledGesturesOnTarget(Transform target)
        {
            var result = new List<Gesture>();
            if (target.gameObject.active)
            {
                result.AddRange(target.GetComponents<Gesture>());
            }
            return result;
        }

        private bool gestureIsActive(Gesture gesture)
        {
            if (gesture.gameObject.active == false) return false;
            if (gesture.enabled == false) return false;
            switch (gesture.State)
            {
                case Gesture.GestureState.Failed:
                case Gesture.GestureState.Recognized:
                case Gesture.GestureState.Cancelled:
                    return false;
                default:
                    return true;
            }
        }

        private bool gestureCanRecognize(Gesture gesture)
        {
            if (!gesture.ShouldBegin()) return false;

            var gestures = getHierarchyContaining(gesture.transform);
            foreach (var otherGesture in gestures)
            {
                if (gesture == otherGesture) continue;
                if (!gestureIsActive(otherGesture)) continue;
                if ((otherGesture.State == Gesture.GestureState.Began || otherGesture.State == Gesture.GestureState.Changed) &&
                    otherGesture.CanPreventGesture(gesture))
                {
                    return false;
                }
            }

            return true;
        }

        private void recognizeGesture(Gesture gesture)
        {
            var gestures = getHierarchyContaining(gesture.transform);
            foreach (var otherGesture in gestures)
            {
                if (gesture == otherGesture) continue;
                if (!gestureIsActive(otherGesture)) continue;
                if (!(otherGesture.State == Gesture.GestureState.Began || otherGesture.State == Gesture.GestureState.Changed) &&
                    gesture.CanPreventGesture(otherGesture))
                {
                    failGesture(otherGesture);
                }
            }
        }

        private void failGesture(Gesture gesture)
        {
            gesture.SetState(Gesture.GestureState.Failed);
        }

        private bool checkLayers(TouchPoint touch)
        {
            foreach (var touchLayer in layers)
            {
                if (touchLayer == null) continue;
                if (touchLayer.BeginTouch(touch)) return true;
            }

            return false;
        }

        #endregion
    }
}