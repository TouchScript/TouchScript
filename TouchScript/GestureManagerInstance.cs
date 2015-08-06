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
        private List<Gesture> gesturesToReset = new List<Gesture>(20);

        #endregion

        #region Temporary variables

        // Temporary variables for update methods.
        private Dictionary<Transform, List<ITouch>> targetTouches = new Dictionary<Transform, List<ITouch>>(10);
        private Dictionary<Gesture, List<ITouch>> gestureTouches = new Dictionary<Gesture, List<ITouch>>(10);
        private List<Gesture> activeGestures = new List<Gesture>(20);
        private List<Gesture> tmpList_Gesture_getEnabledGesturesOnTarget = new List<Gesture>(20);
        private List<Gesture> tmpList_Gesture = new List<Gesture>(20); 
        private List<Gesture> tmpList2_Gesture = new List<Gesture>(20); 

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

            var count = touches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = touches[i];
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
            count = activeGestures.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGestures[i];
                if (gestureIsActive(gesture)) dispatch(gesture, gestureTouches[gesture]);
            }
        }

        private void processTarget(Transform target)
        {
            // gestures on objects in the hierarchy from "root" to target
            tmpList_Gesture.Clear();
            getHierarchyEndingWith(target, tmpList_Gesture);

            var count = tmpList_Gesture.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = tmpList_Gesture[i];
                if (!gestureIsActive(gesture)) continue;

                distributePointsByGestures(target, gesture, gesture.HasTouch);
            }
        }

        private void processTargetBegan(Transform target)
        {
            tmpList_Gesture.Clear();
            tmpList2_Gesture.Clear();
            // gestures in the target's hierarchy which might affect gesture on the target
            getHierarchyContaining(target, tmpList_Gesture);
            // gestures on objects in the hierarchy from "root" to target
            getHierarchyEndingWith(target, tmpList2_Gesture);
            var count = tmpList2_Gesture.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = tmpList2_Gesture[i];
                // WARNING! Gestures might change during this loop.
                // For example when one of them recognizes.
                if (!gestureIsActive(gesture)) continue;

                var canReceiveTouches = true;
                var activeCount = tmpList_Gesture.Count;
                for (var j = 0; j < activeCount; j++)
                {
                    var activeGesture = tmpList_Gesture[j];

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

            var count = gesturesToReset.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = gesturesToReset[i];
                if (gesture == null) continue;
                gesture.ResetGesture();
                gesture.SetState(Gesture.GestureState.Possible);
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
                target.GetComponents(tmpList_Gesture_getEnabledGesturesOnTarget);
                var count = tmpList_Gesture_getEnabledGesturesOnTarget.Count;
                for (var i = 0; i < count; i++)
                {
                    var gesture = tmpList_Gesture_getEnabledGesturesOnTarget[i];
                    if (gesture != null && gesture.enabled) outputList.Add(gesture);
                }
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

            tmpList_Gesture.Clear();
            tmpList2_Gesture.Clear();
            bool canRecognize = true;
            getHierarchyContaining(gesture.transform, tmpList2_Gesture);

            var count = tmpList2_Gesture.Count;
            for (var i = 0; i < count; i++)
            {
                var otherGesture = tmpList2_Gesture[i];
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
                        tmpList_Gesture.Add(otherGesture);
                    }
                }
            }

            if (canRecognize)
            {
                count = tmpList_Gesture.Count;
                for (var i = 0; i < count; i++)
                {
                    failGesture(tmpList_Gesture[i]);
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
