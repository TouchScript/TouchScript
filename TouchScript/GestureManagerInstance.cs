/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Internal implementation of <see cref="IGestureManager"/>.
    /// </summary>
    internal sealed class GestureManagerInstance : MonoBehaviour, IGestureManager
    {
        #region Public properties

        public static IGestureManager Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    if (!Application.isPlaying) return null;
                    var objects = FindObjectsOfType<GestureManagerInstance>();
                    if (objects.Length == 0)
                    {
                        var go = new GameObject("GestureManager Instance");
                        go.hideFlags = HideFlags.HideInHierarchy;
                        DontDestroyOnLoad(go);
                        instance = go.AddComponent<GestureManagerInstance>();
                    }
                    else if (objects.Length >= 1)
                    {
                        instance = objects[0];
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Private variables

        private static GestureManagerInstance instance;
        private static bool shuttingDown = false;

        // Upcoming changes
        private List<Gesture> gesturesToReset = new List<Gesture>(10);

        #endregion

        #region Temporary variables

        // Temporary variables for update methods.
        private Dictionary<Transform, List<ITouch>> targetTouches = new Dictionary<Transform, List<ITouch>>(10);
        private Dictionary<Gesture, List<ITouch>> gestureTouches = new Dictionary<Gesture, List<ITouch>>(10);
        private List<Gesture> activeGestures = new List<Gesture>(10);

        #endregion

        #region Unity

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void OnEnable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted += frameStartedHandler;
                touchManager.FrameFinished += frameFinishedHandler;
                touchManager.TouchesBegan += touchBeganHandler;
                touchManager.TouchesMoved += touchMovedHandler;
                touchManager.TouchesEnded += touchEndedHandler;
                touchManager.TouchesCancelled += touchCancelledHandler;
            }
        }

        private void OnDisable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted -= frameStartedHandler;
                touchManager.FrameFinished -= frameFinishedHandler;
                touchManager.TouchesBegan -= touchBeganHandler;
                touchManager.TouchesMoved -= touchMovedHandler;
                touchManager.TouchesEnded -= touchEndedHandler;
                touchManager.TouchesCancelled -= touchCancelledHandler;
            }
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Internal methods

        internal Gesture.GestureState GestureChangeState(Gesture gesture, Gesture.GestureState state)
        {
            bool recognized = false;
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
                    recognized = recognizeGestureIfNotPrevented(gesture);
                    if (!recognized)
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
                            recognized = recognizeGestureIfNotPrevented(gesture);
                            if (!recognized)
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

        private void updateBegan(IList<ITouch> touches)
        {
            update(touches, processTargetBegan,
                (gesture, touchPoints) => gesture.TouchesBegan(touchPoints));
        }

        private void updateMoved(IList<ITouch> touches)
        {
            update(touches, processTarget,
                (gesture, touchPoints) => gesture.TouchesMoved(touchPoints));
        }

        private void updateEnded(IList<ITouch> touches)
        {
            update(touches, processTarget,
                (gesture, touchPoints) => gesture.TouchesEnded(touchPoints));
        }

        private void updateCancelled(IList<ITouch> touches)
        {
            update(touches, processTarget,
                (gesture, touchPoints) => gesture.TouchesCancelled(touchPoints));
        }

        private void update(IList<ITouch> touches, Action<Transform> process, Action<Gesture, IList<ITouch>> dispatch)
        {
            // WARNING! Arcane magic ahead!

            // Dictionary<Transform, List<ITouch>> - touch points sorted by targets
            targetTouches.Clear();
            // Dictionary<Gesture, List<ITouch>> - touch points sorted by gesture
            gestureTouches.Clear();
            // gestures which got any touch points
            // needed because there's no order in dictionary
            activeGestures.Clear();

            foreach (var touch in touches)
            {
                if (touch.Target != null)
                {
                    List<ITouch> list;
                    if (!targetTouches.TryGetValue(touch.Target, out list))
                    {
                        list = new List<ITouch>();
                        targetTouches.Add(touch.Target, list);
                    }
                    list.Add(touch);
                }
            }
            // arranged touch points by target

            foreach (var target in targetTouches.Keys) process(target);
            foreach (var gesture in activeGestures)
            {
                if (gestureIsActive(gesture)) dispatch(gesture, gestureTouches[gesture]);
            }
        }

        private void processTarget(Transform target)
        {
            // gestures on objects in the hierarchy from "root" to target
            var possibleGestures = getHierarchyEndingWith(target);

            foreach (var gesture in possibleGestures)
            {
                if (!gestureIsActive(gesture)) continue;

                distributePointsByGestures(target, gesture, gesture.HasTouch);
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

        private void distributePointsByGestures(Transform target, Gesture gesture, Predicate<ITouch> condition)
        {
            var touchesToReceive = targetTouches[target].FindAll(condition);
            if (touchesToReceive.Count > 0)
            {
                if (gestureTouches.ContainsKey(gesture))
                {
                    gestureTouches[gesture].AddRange(touchesToReceive);
                }
                else
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
                if (gesture == null) continue;
                gesture.Reset();
                gesture.SetState(Gesture.GestureState.Possible);
            }
            gesturesToReset.Clear();
        }

        // parent <- parent <- target
        private List<Gesture> getHierarchyEndingWith(Transform target)
        {
            var hierarchy = new List<Gesture>(10);
            while (target != null)
            {
                hierarchy.AddRange(getEnabledGesturesOnTarget(target));
                target = target.parent;
            }
            return hierarchy;
        }

        // target <- child*
        private List<Gesture> getHierarchyBeginningWith(Transform target, bool includeSelf)
        {
            var hierarchy = new List<Gesture>(10);
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
            var result = new List<Gesture>(10);
            if (target.gameObject.activeInHierarchy)
            {
                var gestures = target.GetComponents<Gesture>();
                foreach (var gesture in gestures)
                {
                    if (gesture != null && gesture.enabled) result.Add(gesture);
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

        private bool recognizeGestureIfNotPrevented(Gesture gesture)
        {
            if (!gesture.ShouldBegin()) return false;

            bool canRecognize = true;
            List<Gesture> gesturesToFail = new List<Gesture>(10);
            var gestures = getHierarchyContaining(gesture.transform);

            foreach (var otherGesture in gestures)
            {
                if (gesture == otherGesture) continue;
                if (!gestureIsActive(otherGesture)) continue;

                if (otherGesture.State == Gesture.GestureState.Began || otherGesture.State == Gesture.GestureState.Changed)
                {
                    if (otherGesture.CanPreventGesture(gesture))
                    {
                        canRecognize = false;
                        break;
                    }
                }
                else
                {
                    if (gesture.CanPreventGesture(otherGesture))
                    {
                        gesturesToFail.Add(otherGesture);
                    }
                }
            }

            if (canRecognize)
            {
                foreach (var otherGesture in gesturesToFail)
                {
                    failGesture(otherGesture);
                }
            }

            return canRecognize;
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
            updateBegan(touchEventArgs.Touches);
        }

        private void touchMovedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateMoved(touchEventArgs.Touches);
        }

        private void touchEndedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateEnded(touchEventArgs.Touches);
        }

        private void touchCancelledHandler(object sender, TouchEventArgs touchEventArgs)
        {
            updateCancelled(touchEventArgs.Touches);
        }

        #endregion
    }
}
