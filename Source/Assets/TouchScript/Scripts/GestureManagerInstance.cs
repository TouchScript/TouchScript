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

        public IGestureDelegate GlobalGestureDelegate { get; set; }

        #endregion

        #region Private variables

        private static GestureManagerInstance instance;
        private static bool shuttingDown = false;

        // Upcoming changes
        private List<Gesture> gesturesToReset = new List<Gesture>(20);

        private Action<Gesture, TouchPoint> _updateBegan, _updateMoved, _updateEnded, _updateCancelled;
        private Action<Transform, TouchPoint> _processTarget, _processTargetBegan;

        #endregion

        #region Temporary variables

        // Temporary variables for update methods.
        private List<Gesture> activeGestures = new List<Gesture>(20);

        private static ObjectPool<List<Gesture>> gestureListPool = new ObjectPool<List<Gesture>>(10,
            () => new List<Gesture>(20), null, (l) => l.Clear());

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

            _processTarget = processTarget;
            _processTargetBegan = processTargetBegan;
            _updateBegan = doUpdateBegan;
            _updateMoved = doUpdateMoved;
            _updateEnded = doUpdateEnded;
            _updateCancelled = doUpdateCancelled;

            gestureListPool.WarmUp(5);
        }

        private void OnEnable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted += frameStartedHandler;
                touchManager.FrameFinished += frameFinishedHandler;
                touchManager.TouchBegan += touchBeganHandler;
                touchManager.TouchMoved += touchMovedHandler;
                touchManager.TouchEnded += touchEndedHandler;
                touchManager.TouchCancelled += touchCancelledHandler;
            }
        }

        private void OnDisable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted -= frameStartedHandler;
                touchManager.FrameFinished -= frameFinishedHandler;
                touchManager.TouchBegan -= touchBeganHandler;
                touchManager.TouchMoved -= touchMovedHandler;
                touchManager.TouchEnded -= touchEndedHandler;
                touchManager.TouchCancelled -= touchCancelledHandler;
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
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}",
                                new object[] {gesture, state, gesture.State}));
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
                            print(String.Format("Gesture {0} erroneously tried to enter state {1} from state {2}",
                                new object[] {gesture, state, gesture.State}));
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
                            print(string.Format("Gesture {0} erroneously tried to enter state {1} from state {2}",
                                new object[] {gesture, state, gesture.State}));
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

        private void doUpdateBegan(Gesture gesture, TouchPoint touch)
        {
            gesture.INTERNAL_TouchBegan(touch);
        }

        private void doUpdateMoved(Gesture gesture, TouchPoint touch)
        {
            gesture.INTERNAL_TouchMoved(touch);
        }

        private void doUpdateEnded(Gesture gesture, TouchPoint touch)
        {
            gesture.INTERNAL_TouchEnded(touch);
        }

        private void doUpdateCancelled(Gesture gesture, TouchPoint touch)
        {
            gesture.INTERNAL_TouchCancelled(touch);
        }

        private void update(TouchPoint touch, Action<Transform, TouchPoint> process,
                            Action<Gesture, TouchPoint> dispatch)
        {
            // WARNING! Arcane magic ahead!
            // gestures which got any touch points
            // needed because there's no order in dictionary
            activeGestures.Clear();

            if (touch.Target != null) process(touch.Target, touch);
            var count = activeGestures.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGestures[i];
                if (gestureIsActive(gesture)) dispatch(gesture, touch);
            }
        }

        private void processTarget(Transform target, TouchPoint touch)
        {
            // gestures on objects in the hierarchy from "root" to target
            var endingList = gestureListPool.Get();
            getHierarchyEndingWith(target, endingList);

            var count = endingList.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = endingList[i];
                if (!gestureIsActive(gesture)) continue;

                if (gesture.HasTouch(touch)) activeGestures.Add(gesture);
            }
            gestureListPool.Release(endingList);
        }

        private void processTargetBegan(Transform target, TouchPoint touch)
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
                    if ((activeGesture.State == Gesture.GestureState.Began ||
                         activeGesture.State == Gesture.GestureState.Changed) &&
                        (canPreventGesture(activeGesture, gesture)))
                    {
                        // there's a started gesture which prevents this one
                        canReceiveTouches = false;
                        break;
                    }
                }

                if (canReceiveTouches && shouldReceiveTouch(gesture, touch)) activeGestures.Add(gesture);
            }

            gestureListPool.Release(containingList);
            gestureListPool.Release(endingList);
        }

        private void resetGestures()
        {
            if (gesturesToReset.Count == 0) return;

            var count = gesturesToReset.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = gesturesToReset[i];
                if (gesture == null) continue;
                gesture.INTERNAL_Reset();
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
            if (!shouldBegin(gesture)) return false;

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

                if (otherGesture.State == Gesture.GestureState.Began ||
                    otherGesture.State == Gesture.GestureState.Changed)
                {
                    if (canPreventGesture(otherGesture, gesture))
                    {
                        canRecognize = false;
                        break;
                    }
                }
                else
                {
                    if (canPreventGesture(gesture, otherGesture))
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

        private bool shouldReceiveTouch(Gesture gesture, TouchPoint touch)
        {
            bool result = true;
            if (GlobalGestureDelegate != null) result = GlobalGestureDelegate.ShouldReceiveTouch(gesture, touch);
            return result && gesture.ShouldReceiveTouch(touch);
        }

        private bool shouldBegin(Gesture gesture)
        {
            bool result = true;
            if (GlobalGestureDelegate != null) result = GlobalGestureDelegate.ShouldBegin(gesture);
            return result && gesture.ShouldBegin();
        }

        private bool canPreventGesture(Gesture first, Gesture second)
        {
            bool result = true;
            if (GlobalGestureDelegate != null) result = !GlobalGestureDelegate.ShouldRecognizeSimultaneously(first, second);
            return result && first.CanPreventGesture(second);
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
            update(touchEventArgs.Touch, _processTargetBegan, _updateBegan);
        }

        private void touchMovedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            update(touchEventArgs.Touch, _processTarget, _updateMoved);
        }

        private void touchEndedHandler(object sender, TouchEventArgs touchEventArgs)
        {
            update(touchEventArgs.Touch, _processTarget, _updateEnded);
        }

        private void touchCancelledHandler(object sender, TouchEventArgs touchEventArgs)
        {
            update(touchEventArgs.Touch, _processTarget, _updateCancelled);
        }

        #endregion
    }
}