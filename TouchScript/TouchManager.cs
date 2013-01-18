/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using TouchScript.Events;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript {
    /// <summary>
    /// Singleton which handles all touch and gesture management.
    /// Shouldn't be instantiated manually.
    /// </summary>
    public class TouchManager : MonoBehaviour {
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
        public event EventHandler<TouchEventArgs> TouchPointsAdded;

        /// <summary>
        /// Occurs when touch points are updated.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchPointsUpdated;

        /// <summary>
        /// Occurs when touch points are removed.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchPointsRemoved;

        /// <summary>
        /// Occurs when touch points are cancelled.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchPointsCancelled;

        #endregion

        #region Public properties

        /// <summary>
        /// TouchManager singleton instance.
        /// </summary>
        public static TouchManager Instance { get; private set; }

        /// <summary>
        /// Active cameras to look for touch targets in specific order.
        /// </summary>
        public List<Camera> HitCameras { get; set; }

        /// <summary>
        /// Current touch device DPI.
        /// </summary>
        public float DPI { get; set; }

        /// <summary>
        /// Radius of single touch point on device in cm.
        /// </summary>
        public float TouchRadius { get; set; }

        /// <summary>
        /// Touch point radius in pixels.
        /// </summary>
        public float PixelTouchRadius {
            get { return TouchRadius*DotsPerCentimeter; }
        }

        /// <summary>
        /// Pixels in a cm with current DPI.
        /// </summary>
        public float DotsPerCentimeter {
            get { return CM_TO_INCH*DPI; }
        }

        /// <summary>
        /// Number of active touches.
        /// </summary>
        public int TouchesCount {
            get { return touches.Count; }
        }

        /// <summary>
        /// List of active touches.
        /// </summary>
        public List<TouchPoint> Touches {
            get { return new List<TouchPoint>(touches); }
        }

        #endregion

        #region private Variables

        private List<TouchPoint> touches = new List<TouchPoint>();
        private Dictionary<int, TouchPoint> idToTouch = new Dictionary<int, TouchPoint>();

        // Upcoming changes
        private List<TouchPoint> touchesBegan = new List<TouchPoint>();
        private List<TouchPoint> touchesEnded = new List<TouchPoint>();
        private List<TouchPoint> touchesCancelled = new List<TouchPoint>();
        private Dictionary<int, TouchPointUpdate> touchesMoved = new Dictionary<int, TouchPointUpdate>();
        private List<Gesture> gesturesToReset = new List<Gesture>();

        // Locks
        private readonly object sync = new object();

        private int nextTouchPointId = 0;

        #endregion

        #region Unity

        private void Awake() {
            if (Instance != null) throw new InvalidOperationException("Attempt to create another instance of TouchManager.");
            Instance = this;
            DPI = 72;
            TouchRadius = .75f;
        }

        private void Update() {
            updateTouches();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <returns>Object's transform which has been hit or null otherwise.</returns>
        public Transform GetHitTarget(TouchPoint touch) {
            RaycastHit hit;
            Camera hitCamera;
            return GetHitTarget(touch, out hit, out hitCamera);
        }

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <param name="hit">Output RaycastHit.</param>
        /// <param name="hitCamera">Output camera which was used to hit an object.</param>
        /// <returns>Object's transform which has been hit or null otherwise.</returns>
        public Transform GetHitTarget(TouchPoint touch, out RaycastHit hit, out Camera hitCamera) {
            hit = new RaycastHit();
            hitCamera = null;

            if (HitCameras == null) return null;

            foreach (var cam in HitCameras) {
                hitCamera = cam;
                var ray = cam.ScreenPointToRay(new Vector3(touch.Position.x, touch.Position.y, cam.nearClipPlane));
                var hits = Physics.RaycastAll(ray);
                if (hits.Length == 0) continue;

                var minDist = float.PositiveInfinity;
                foreach (var rayHit in hits) {
                    var dist = (rayHit.point - cam.transform.position).sqrMagnitude;
                    if (dist < minDist) {
                        minDist = dist;
                        hit = rayHit;
                    }
                }
                var hitTests = hit.transform.GetComponents<HitTest>();
                if (hitTests.Length == 0) return hit.transform;
                var result = true;
                foreach (var test in hitTests) {
                    result = test.IsHit(hit);
                    if (!result) break;
                }
                if (result) return hit.transform;
            }

            return null;
        }

        /// <summary>
        /// Registers a touch.
        /// </summary>
        /// <param name="position">Touch position.</param>
        /// <returns>Internal id of the new touch.</returns>
        public int BeginTouch(Vector2 position) {
            TouchPoint touch;
            lock (sync) {
                touch = new TouchPoint(nextTouchPointId++, position);
                touchesBegan.Add(touch);
            }
            return touch.Id;
        }

        /// <summary>
        /// Ends a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        public void EndTouch(int id) {
            lock (sync) {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch)) {
                    foreach (var addedTouch in touchesBegan) {
                        if (addedTouch.Id == id) {
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
        public void CancelTouch(int id) {
            lock (sync) {
                TouchPoint touch;
                if (!idToTouch.TryGetValue(id, out touch)) {
                    foreach (var addedTouch in touchesBegan) {
                        if (addedTouch.Id == id) {
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
        public void MoveTouch(int id, Vector2 position) {
            lock (sync) {
                TouchPointUpdate update;
                if (touchesMoved.TryGetValue(id, out update)) {
                    update.Position = position;
                } else {
                    touchesMoved.Add(id, new TouchPointUpdate(id, position));
                }
            }
        }

        #endregion

        #region Internal methods

        internal Gesture.GestureState GestureChangeState(Gesture gesture, Gesture.GestureState state) {
            switch (state) {
                case Gesture.GestureState.Possible:
                    break;
                case Gesture.GestureState.Began:
                    switch (gesture.State) {
                        case Gesture.GestureState.Possible:
                            break;
                        default:
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", gesture, state, gesture.State));
                            break;
                    }
                    if (gestureCanRecognize(gesture)) {
                        recognizeGesture(gesture);
                    } else {
                        if (!gesturesToReset.Contains(gesture)) gesturesToReset.Add(gesture);
                        return Gesture.GestureState.Failed;
                    }
                    break;
                case Gesture.GestureState.Changed:
                    switch (gesture.State) {
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
                    switch (gesture.State) {
                        case Gesture.GestureState.Possible:
                            if (gestureCanRecognize(gesture)) {
                                recognizeGesture(gesture);
                            } else {
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

        internal void IgnoreTouch(TouchPoint touch) {}

        #endregion

        #region Private functions

        private bool updateBegan() {
            if (touchesBegan.Count > 0) {
                // get touches per target
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                foreach (var touch in touchesBegan) {
                    touches.Add(touch);
                    idToTouch.Add(touch.Id, touch);
                    RaycastHit hit;
                    Camera hitCamera;
                    touch.Target = GetHitTarget(touch, out hit, out hitCamera);
                    touch.Hit = hit;
                    touch.HitCamera = hitCamera;

                    if (touch.Target != null) {
                        List<TouchPoint> list;
                        if (!targetTouches.TryGetValue(touch.Target, out list)) {
                            list = new List<TouchPoint>();
                            targetTouches.Add(touch.Target, list);
                        }
                        list.Add(touch);
                    }
                }

                // get touches per gesture
                // touches can come to a gesture from multiple targets in hierarchy
                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                foreach (var target in targetTouches.Keys) {
                    var mightBeActiveGestures = getHierarchyContaining(target);
                    var possibleGestures = getHierarchyEndingWith(target);
                    foreach (var gesture in possibleGestures) {
                        if (!gestureIsActive(gesture)) continue;

                        var canReceiveTouches = true;
                        foreach (var activeGesture in mightBeActiveGestures) {
                            if (gesture == activeGesture) continue;
                            if ((activeGesture.State == Gesture.GestureState.Began || activeGesture.State == Gesture.GestureState.Changed) && (activeGesture.CanPreventGesture(gesture))) {
                                canReceiveTouches = false;
                                break;
                            }
                        }
                        if (canReceiveTouches) {
                            var touchesToReceive =
                                targetTouches[target].FindAll((TouchPoint touch) => gesture.ShouldReceiveTouch(touch));
                            if (touchesToReceive.Count > 0) {
                                if (gestureTouches.ContainsKey(gesture)) {
                                    gestureTouches[gesture].AddRange(touchesToReceive);
                                } else {
                                    gestureTouches[gesture] = touchesToReceive;
                                }
                            }
                        }
                    }
                }

                foreach (KeyValuePair<Gesture, List<TouchPoint>> valuePair in gestureTouches) {
                    var gesture = valuePair.Key;
                    if (gestureIsActive(gesture)) gesture.TouchesBegan(valuePair.Value);
                }

                if (TouchPointsAdded != null) TouchPointsAdded(this, new TouchEventArgs(new List<TouchPoint>(touchesBegan)));
                touchesBegan.Clear();

                return true;
            }
            return false;
        }

        private bool updateMoved() {
            if (touchesMoved.Count > 0) {
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                var reallyMoved = new List<TouchPoint>();
                foreach (var update in touchesMoved.Values) {
                    TouchPoint touch;
                    if (!idToTouch.TryGetValue(update.Id, out touch)) continue;
                    if (touch.Position == update.Position) continue;

                    touch.Position = update.Position;
                    reallyMoved.Add(touch);
                    if (touch.Target != null) {
                        List<TouchPoint> list;
                        if (!targetTouches.TryGetValue(touch.Target, out list)) {
                            list = new List<TouchPoint>();
                            targetTouches.Add(touch.Target, list);
                        }
                        list.Add(touch);
                    }
                }

                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                foreach (var target in targetTouches.Keys) {
                    var possibleGestures = getHierarchyEndingWith(target);

                    foreach (var gesture in possibleGestures) {
                        if (!gestureIsActive(gesture)) continue;

                        var touchesToReceive =
                            targetTouches[target].FindAll(gesture.HasTouchPoint);
                        if (touchesToReceive.Count > 0) {
                            if (gestureTouches.ContainsKey(gesture)) {
                                gestureTouches[gesture].AddRange(touchesToReceive);
                            } else {
                                gestureTouches[gesture] = touchesToReceive;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<Gesture, List<TouchPoint>> valuePair in gestureTouches) {
                    var gesture = valuePair.Key;
                    if (gestureIsActive(gesture)) gesture.TouchesMoved(valuePair.Value);
                }

                if (reallyMoved.Count > 0 && TouchPointsUpdated != null) TouchPointsUpdated(this, new TouchEventArgs(new List<TouchPoint>(reallyMoved)));
                touchesMoved.Clear();

                return true;
            }
            return false;
        }

        private bool updateEnded() {
            if (touchesEnded.Count > 0) {
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                foreach (var touch in touchesEnded) {
                    idToTouch.Remove(touch.Id);
                    touches.Remove(touch);
                    if (touch.Target != null) {
                        List<TouchPoint> list;
                        if (!targetTouches.TryGetValue(touch.Target, out list)) {
                            list = new List<TouchPoint>();
                            targetTouches.Add(touch.Target, list);
                        }
                        list.Add(touch);
                    }
                }

                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                foreach (var target in targetTouches.Keys) {
                    var possibleGestures = getHierarchyEndingWith(target);
                    foreach (var gesture in possibleGestures) {
                        if (!gestureIsActive(gesture)) continue;

                        var touchesToReceive =
                            targetTouches[target].FindAll(gesture.HasTouchPoint);
                        if (touchesToReceive.Count > 0) {
                            if (gestureTouches.ContainsKey(gesture)) {
                                gestureTouches[gesture].AddRange(touchesToReceive);
                            } else {
                                gestureTouches[gesture] = touchesToReceive;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<Gesture, List<TouchPoint>> valuePair in gestureTouches) {
                    var gesture = valuePair.Key;
                    if (gestureIsActive(gesture)) gesture.TouchesEnded(valuePair.Value);
                }

                if (TouchPointsRemoved != null) TouchPointsRemoved(this, new TouchEventArgs(new List<TouchPoint>(touchesEnded)));
                touchesEnded.Clear();

                return true;
            }
            return false;
        }

        private bool updateCancelled() {
            if (touchesCancelled.Count > 0) {
                var targetTouches = new Dictionary<Transform, List<TouchPoint>>();
                foreach (var touch in touchesCancelled) {
                    idToTouch.Remove(touch.Id);
                    touches.Remove(touch);
                    if (touch.Target != null) {
                        List<TouchPoint> list;
                        if (!targetTouches.TryGetValue(touch.Target, out list)) {
                            list = new List<TouchPoint>();
                            targetTouches.Add(touch.Target, list);
                        }
                        list.Add(touch);
                    }
                }

                var gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
                foreach (var target in targetTouches.Keys) {
                    var possibleGestures = getHierarchyEndingWith(target);
                    foreach (var gesture in possibleGestures) {
                        if (!gestureIsActive(gesture)) continue;

                        var touchesToReceive =
                            targetTouches[target].FindAll(gesture.HasTouchPoint);
                        if (touchesToReceive.Count > 0) {
                            if (gestureTouches.ContainsKey(gesture)) {
                                gestureTouches[gesture].AddRange(touchesToReceive);
                            } else {
                                gestureTouches[gesture] = touchesToReceive;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<Gesture, List<TouchPoint>> valuePair in gestureTouches) {
                    valuePair.Key.TouchesCancelled(valuePair.Value);
                }

                if (TouchPointsCancelled != null) TouchPointsCancelled(this, new TouchEventArgs(new List<TouchPoint>(touchesCancelled)));
                touchesCancelled.Clear();

                return true;
            }
            return false;
        }

        private void updateTouches() {
            // reset gestures changed between update loops
            resetGestures();
            bool updated;
            lock (sync) {
                updated = updateBegan();
                updated = updateMoved() || updated;
                updated = updateEnded() || updated;
                updated = updateCancelled() || updated;
            }

            if (updated) resetGestures();
        }

        private void resetGestures() {
            foreach (var gesture in gesturesToReset) {
                gesture.Reset();
                gesture.SetState(Gesture.GestureState.Possible);
            }
            gesturesToReset.Clear();
        }

        private List<Gesture> getHierarchyEndingWith(Transform target) {
            var hierarchy = new List<Gesture>();
            while (target != null) {
                hierarchy.AddRange(getEnabledGesturesOnTarget(target));
                target = target.parent;
            }
            return hierarchy;
        }

        private List<Gesture> getHierarchyBeginningWith(Transform target, bool includeSelf = true) {
            var hierarchy = new List<Gesture>();
            if (includeSelf) {
                hierarchy.AddRange(getEnabledGesturesOnTarget(target));
            }
            foreach (Transform child in target) {
                hierarchy.AddRange(getHierarchyBeginningWith(child));
            }
            return hierarchy;
        }

        private List<Gesture> getHierarchyContaining(Transform target) {
            var hierarchy = getHierarchyEndingWith(target);
            hierarchy.AddRange(getHierarchyBeginningWith(target, false));
            return hierarchy;
        }

        private List<Gesture> getEnabledGesturesOnTarget(Transform target) {
            var result = new List<Gesture>();
            if (target.gameObject.active) {
                result.AddRange(target.GetComponents<Gesture>());
            }
            return result;
        }

        private bool gestureIsActive(Gesture gesture) {
            if (gesture.gameObject.active == false) return false;
            if (gesture.enabled == false) return false;
            switch (gesture.State) {
                case Gesture.GestureState.Failed:
                case Gesture.GestureState.Recognized:
                case Gesture.GestureState.Cancelled:
                    return false;
                default:
                    return true;
            }
        }

        private bool gestureCanRecognize(Gesture gesture) {
            if (!gesture.ShouldBegin()) return false;

            var gestures = getHierarchyContaining(gesture.transform);
            foreach (var otherGesture in gestures) {
                if (gesture == otherGesture) continue;
                if (!gestureIsActive(otherGesture)) continue;
                if ((otherGesture.State == Gesture.GestureState.Began || otherGesture.State == Gesture.GestureState.Changed) &&
                    otherGesture.CanPreventGesture(gesture)) {
                    return false;
                }
            }

            return true;
        }

        private void recognizeGesture(Gesture gesture) {
            var gestures = getHierarchyContaining(gesture.transform);
            foreach (var otherGesture in gestures) {
                if (gesture == otherGesture) continue;
                if (!gestureIsActive(otherGesture)) continue;
                if (!(otherGesture.State == Gesture.GestureState.Began || otherGesture.State == Gesture.GestureState.Changed) &&
                    gesture.CanPreventGesture(otherGesture)) {
                    failGesture(otherGesture);
                }
            }
        }

        private void failGesture(Gesture gesture) {
            gesture.SetState(Gesture.GestureState.Failed);
        }

        #endregion
    }

    internal class TouchPointUpdate {
        public int Id { get; private set; }
        public Vector2 Position { get; set; }

        public TouchPointUpdate(int id, Vector2 position) {
            Id = id;
            Position = position;
        }
    }
}