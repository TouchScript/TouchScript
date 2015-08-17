/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Utils;
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
        private List<Gesture> gesturesToReset = new List<Gesture>(20);

        #endregion

        #region Temporary variables

        // Temporary variables for update methods.
        // Dictionary<Transform, List<ITouch>> - touch points sorted by targets
        private Dictionary<Transform, List<ITouch>> targetTouches = new Dictionary<Transform, List<ITouch>>(10);
        // Dictionary<Gesture, List<ITouch>> - touch points sorted by gesture
        private Dictionary<Gesture, List<ITouch>> gestureTouches = new Dictionary<Gesture, List<ITouch>>(10);
        private List<Gesture> activeGestures = new List<Gesture>(20);
        private static ObjectPool<List<Gesture>> gestureListPool = new ObjectPool<List<Gesture>>(() => new List<Gesture>(20), 
              /*(l) => Debug.Log("Getting List<Gesture> from pool. Pool.Count: " + gestureListPool.CountAll + ", Pool.CountInactive: " + gestureListPool.CountInactive)*/ null, (l) => l.Clear());
        private static ObjectPool<List<ITouch>> touchListPool = new ObjectPool<List<ITouch>>(() => new List<ITouch>(10), 
                                                                                      /*(l) => Debug.Log("Getting List<ITouch> from pool. Pool.Count: " + touchListPool.CountAll + ", Pool.CountInactive: " + touchListPool.CountInactive)*/ null, (l) => l.Clear());

        #endregion

        #region Unity

        private void Awake()
        {
            if (instance == null) instance = this;

//            for (var i = 0; i < 3; i++) gestureListPool.Release(new List<Gesture>(20));
//            for (var i = 0; i < 5; i++) touchListPool.Release(new List<ITouch>(10));
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

        internal Gesture.GestureState INTERNAL_GestureChangeState(Gesture gesture, Gesture.GestureState state)
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
                (gesture, touchPoints) => gesture.INTERNAL_TouchesBegan(touchPoints));
        }

        private void updateMoved(IList<ITouch> touches)
        {
            update(touches, processTarget,
                (gesture, touchPoints) => gesture.INTERNAL_TouchesMoved(touchPoints));
        }

        private void updateEnded(IList<ITouch> touches)
        {
            update(touches, processTarget,
                (gesture, touchPoints) => gesture.INTERNAL_TouchesEnded(touchPoints));
        }

        private void updateCancelled(IList<ITouch> touches)
        {
            update(touches, processTarget,
                (gesture, touchPoints) => gesture.INTERNAL_TouchesCancelled(touchPoints));
        }

        private void update(IList<ITouch> touches, Action<Transform> process, Action<Gesture, IList<ITouch>> dispatch)
        {
            // WARNING! Arcane magic ahead!
            // gestures which got any touch points
            // needed because there's no order in dictionary
            activeGestures.Clear();

            var count = touches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = touches[i];
                if (touch.Target != null)
                {
                    List<ITouch> list;
                    if (!targetTouches.TryGetValue(touch.Target, out list))
                    {
                        list = touchListPool.Get();
                        targetTouches.Add(touch.Target, list);
                    }
                    list.Add(touch);
                }
            }
            // arranged touch points by target

            foreach (var target in targetTouches.Keys)
            {
                process(target);
                touchListPool.Release(targetTouches[target]);
            }
            count = activeGestures.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGestures[i];
                var list = gestureTouches[gesture];
                if (gestureIsActive(gesture)) dispatch(gesture, list);
                touchListPool.Release(list);
            }

            targetTouches.Clear();
            gestureTouches.Clear();
        }

        private void processTarget(Transform target)
        {
            // gestures on objects in the hierarchy from "root" to target
            var list = gestureListPool.Get();
            getHierarchyEndingWith(target, list);

            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = list[i];
                if (!gestureIsActive(gesture)) continue;

                distributePointsByGestures(target, gesture, gesture.HasTouch);
            }
            gestureListPool.Release(list);
        }

        private void processTargetBegan(Transform target)
        {
            var containingList = gestureListPool.Get();
            var endingList = gestureListPool.Get();
            // gestures in the target's hierarchy which might affect gesture on the target
            getHierarchyContaining(target, containingList);
            // gestures on objects in the hierarchy from "root" to target
            getHierarchyEndingWith(target, endingList);
            var count = endingList.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = endingList[i];
                // WARNING! Gestures might change during this loop.
                // For example when one of them recognizes.
                if (!gestureIsActive(gesture)) continue;

                var canReceiveTouches = true;
                var activeCount = containingList.Count;
                for (var j = 0; j < activeCount; j++)
                {
                    var activeGesture = containingList[j];

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

            gestureListPool.Release(containingList);
            gestureListPool.Release(endingList);
        }

        private void distributePointsByGestures(Transform target, Gesture gesture, Predicate<ITouch> condition)
        {
            var list = touchListPool.Get();
            var targetList = targetTouches[target];
            var count = targetList.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = targetList[i];
                if (condition(touch)) list.Add(touch);
            }
            if (list.Count > 0)
            {
                if (gestureTouches.ContainsKey(gesture))
                {
                    gestureTouches[gesture].AddRange(list);
                    touchListPool.Release(list);
                }
                else
                {
                    activeGestures.Add(gesture);
                    gestureTouches.Add(gesture, list);
                }
            }
            else
            {
                touchListPool.Release(list);
            }
        }

        private void resetGestures()
        {
            if (gesturesToReset.Count == 0) return;

            var count = gesturesToReset.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = gesturesToReset[i];
                if (gesture == null) continue;
                gesture.INTERNAL_ResetGesture();
                gesture.INTERNAL_SetState(Gesture.GestureState.Possible);
            }
            gesturesToReset.Clear();
        }

        // parent <- parent <- target
        private void getHierarchyEndingWith(Transform target, List<Gesture> outputList)
        {
            while (target != null)
            {
                getEnabledGesturesOnTarget(target, outputList);
                target = target.parent;
            }
        }

        // target <- child*
        private void getHierarchyBeginningWith(Transform target, List<Gesture> outputList, bool includeSelf)
        {
            if (includeSelf)
            {
                getEnabledGesturesOnTarget(target, outputList);
            }

            var count = target.childCount;
            for (var i = 0; i < count; i++)
            {
                getHierarchyBeginningWith(target.GetChild(i), outputList, true);
            }
        }

        private void getHierarchyContaining(Transform target, List<Gesture> outputList)
        {
            getHierarchyEndingWith(target, outputList);
            getHierarchyBeginningWith(target, outputList, false);
        }

        private void getEnabledGesturesOnTarget(Transform target, List<Gesture> outputList)
        {
            if (target.gameObject.activeInHierarchy)
            {
                var list = gestureListPool.Get();
                target.GetComponents(list);
                var count = list.Count;
                for (var i = 0; i < count; i++)
                {
                    var gesture = list[i];
                    if (gesture != null && gesture.enabled) outputList.Add(gesture);
                }
                gestureListPool.Release(list);
            }
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

            var gesturesToFail = gestureListPool.Get();
            var gesturesInHierarchy = gestureListPool.Get();
            bool canRecognize = true;
            getHierarchyContaining(gesture.transform, gesturesInHierarchy);

            var count = gesturesInHierarchy.Count;
            for (var i = 0; i < count; i++)
            {
                var otherGesture = gesturesInHierarchy[i];
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
                count = gesturesToFail.Count;
                for (var i = 0; i < count; i++)
                {
                    failGesture(gesturesToFail[i]);
                }
            }

            gestureListPool.Release(gesturesToFail);
            gestureListPool.Release(gesturesInHierarchy);

            return canRecognize;
        }

        private void failGesture(Gesture gesture)
        {
            gesture.INTERNAL_SetState(Gesture.GestureState.Failed);
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
