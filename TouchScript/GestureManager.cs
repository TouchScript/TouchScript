/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Manages touch points dispatching within a hierarchy of gestures.
    /// </summary>
    [AddComponentMenu("TouchScript/Gesture Manager")]
    public class GestureManager : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static GestureManager Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(GestureManager)) as GestureManager;
                    if (instance == null && Application.isPlaying)
                    {
                        var touchManager = FindObjectOfType(typeof(TouchManager)) as TouchManager;
                        if (touchManager == null)
                        {
                            var go = GameObject.Find("TouchScript");
                            if (go == null) go = new GameObject("TouchScript");
                            instance = go.AddComponent<GestureManager>();
                        } else
                        {
                            instance = touchManager.gameObject.AddComponent<GestureManager>();
                        }
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Private variables

        private static GestureManager instance;
        // Flag to indicate that we are going out of Play Mode in the editor. Otherwise there might be a loop when while deinitializing other objects access TouchScript.Instance which recreates an instance of TouchManager and everything breaks.
        private static bool shuttingDown = false;

        // Upcoming changes
        private List<Gesture> gesturesToReset = new List<Gesture>();

        #endregion

        #region Temporary variables

        // Temporary variables for update methods.
        private Dictionary<Transform, List<TouchPoint>> targetTouches = new Dictionary<Transform, List<TouchPoint>>();
        private Dictionary<Gesture, List<TouchPoint>> gestureTouches = new Dictionary<Gesture, List<TouchPoint>>();
        private List<Gesture> activeGestures = new List<Gesture>();

        #endregion

        #region Unity

        private void Awake()
        {
            shuttingDown = false;
            if (instance == null) instance = this;

            TouchManager.Instance.FrameStarted += frameStartedHandler;
            TouchManager.Instance.FrameFinished += frameFinishedHandler;
            TouchManager.Instance.TouchesBegan += touchBeganHandler;
            TouchManager.Instance.TouchesMoved += touchMovedHandler;
            TouchManager.Instance.TouchesEnded += touchEndedHandler;
            TouchManager.Instance.TouchesCancelled += touchCancelledHandler;
        }

        private void OnDestroy()
        {
            if (!Application.isLoadingLevel) shuttingDown = true;
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
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", new object[] {gesture, state, gesture.State}));
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
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", new object[] {gesture, state, gesture.State}));
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
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}", new object[] {gesture, state, gesture.State}));
                            break;
                    }
                    break;
                case Gesture.GestureState.Cancelled:
                    if (!gesturesToReset.Contains(gesture)) gesturesToReset.Add(gesture);
                    break;
            }

            return state;
        }

        #endregion

        #region Private functions

        private void updateBegan(List<TouchPoint> points)
        {
            update(points, processTargetBegan,
                   (gesture, touchPoints) => gesture.TouchesBegan(touchPoints));
        }

        private void updateMoved(List<TouchPoint> points)
        {
            update(points, processTarget,
                   (gesture, touchPoints) => gesture.TouchesMoved(touchPoints));
        }

        private void updateEnded(List<TouchPoint> points)
        {
            update(points, processTarget,
                   (gesture, touchPoints) => gesture.TouchesEnded(touchPoints));
        }

        private void updateCancelled(List<TouchPoint> points)
        {
            update(points, processTarget,
                   (gesture, touchPoints) => gesture.TouchesCancelled(touchPoints));
        }

        private void update(List<TouchPoint> points, Action<Transform> process, Action<Gesture, List<TouchPoint>> dispatch)
        {
            // WARNING! Arcane magic ahead!

            // Dictionary<Transform, List<TouchPoint>> - touch points sorted by targets
            targetTouches.Clear();
            // Dictionary<Gesture, List<TouchPoint>> - touch points sorted by gesture
            gestureTouches.Clear();
            // gestures which got any touch points
            // needed because there's no order in dictionary
            activeGestures.Clear(); 

            foreach (var touch in points)
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
            // arranged touch points by target

            foreach (var target in targetTouches.Keys) process(target);
            foreach (var gesture in activeGestures)
                if (gestureIsActive(gesture)) dispatch(gesture, gestureTouches[gesture]);
        }

        private void processTarget(Transform target)
        {
            // gestures on objects in the hierarchy from "root" to target
            var possibleGestures = getHierarchyEndingWith(target);

            foreach (var gesture in possibleGestures)
            {
                if (!gestureIsActive(gesture)) continue;

                distributePointsByGestures(target, gesture, gesture.HasTouchPoint);
            }
        }

        private void processTargetBegan(Transform target)
        {
            // gestures in the target's hierarchy which might affect gesture on the target
            var mightBeActiveGestures = getHierarchyContaining(target);
            // gestures on objects in the hierarchy from "root" to target
            var possibleGestures = getHierarchyEndingWith(target);
            foreach (var gesture in possibleGestures)
            {
                // WARNING! Gestures might change during this loop.
                // For example when one of them recognizes.
                if (!gestureIsActive(gesture)) continue;

                var canReceiveTouches = true;
                foreach (var activeGesture in mightBeActiveGestures)
                {
                    if (gesture == activeGesture) continue;
                    if ((activeGesture.State == Gesture.GestureState.Began || activeGesture.State == Gesture.GestureState.Changed) && (activeGesture.CanPreventGesture(gesture)))
                    {
                        // there's a started gesture which prevents this one
                        canReceiveTouches = false;
                        break;
                    }
                }

                // check gesture's ShouldReceiveTouch callback
                if (canReceiveTouches) distributePointsByGestures(target, gesture, gesture.ShouldReceiveTouch);
            }
        }

        private void distributePointsByGestures(Transform target, Gesture gesture, Predicate<TouchPoint> condition)
        {
            var touchesToReceive =
                targetTouches[target].FindAll(condition);
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

        private void resetGestures()
        {
            if (gesturesToReset.Count == 0) return;
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

        private List<Gesture> getHierarchyBeginningWith(Transform target, bool includeSelf)
        {
            var hierarchy = new List<Gesture>();
            if (includeSelf)
            {
                hierarchy.AddRange(getEnabledGesturesOnTarget(target));
            }
            foreach (Transform child in target)
            {
                hierarchy.AddRange(getHierarchyBeginningWith(child, true));
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
            if (target.gameObject.activeInHierarchy)
            {
                var gestures = target.GetComponents<Gesture>();
                foreach (var gesture in gestures)
                {
                    if (gesture.enabled) result.Add(gesture);
                }
            }
            return result;
        }

        private bool gestureIsActive(Gesture gesture)
        {
            if (gesture.gameObject.activeInHierarchy == false) return false;
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

        #endregion

        #region Touch events handlers

        private void frameFinishedHandler(object sender, EventArgs eventArgs)
        {
            resetGestures();
        }

        private void frameStartedHandler(object sender, EventArgs eventArgs)
        {
            resetGestures();
        }

        private void touchBeganHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateBegan(touchEventArgs.TouchPoints);
        }

        private void touchMovedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateMoved(touchEventArgs.TouchPoints);
        }

        private void touchEndedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateEnded(touchEventArgs.TouchPoints);
        }

        private void touchCancelledHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateCancelled(touchEventArgs.TouchPoints);
        }

        #endregion
    }
}