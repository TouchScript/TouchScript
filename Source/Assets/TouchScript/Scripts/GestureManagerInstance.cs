/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Utils;
using TouchScript.Pointers;
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

        private Action<Gesture, IList<Pointer>> _updatePressed, _updateUpdated, _updateReleased, _updateCancelled;
        private Action<Transform> _processTarget, _processTargetBegan;

        #endregion

        #region Temporary variables

        // Temporary variables for update methods.
        // Dictionary<Transform, List<Pointer>> - pointers sorted by targets
        private Dictionary<Transform, List<Pointer>> targetPointers = new Dictionary<Transform, List<Pointer>>(10);
        // Dictionary<Gesture, List<Pointer>> - pointers sorted by gesture
        private Dictionary<Gesture, List<Pointer>> gesturePointers = new Dictionary<Gesture, List<Pointer>>(10);
        private List<Gesture> activeGestures = new List<Gesture>(20);

        private static ObjectPool<List<Gesture>> gestureListPool = new ObjectPool<List<Gesture>>(10,
            () => new List<Gesture>(20), null, (l) => l.Clear());

        private static ObjectPool<List<Pointer>> pointerListPool = new ObjectPool<List<Pointer>>(20,
            () => new List<Pointer>(10), null, (l) => l.Clear());

        private static ObjectPool<List<Transform>> transformListPool = new ObjectPool<List<Transform>>(10,
            () => new List<Transform>(10), null, (l) => l.Clear());

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
            _updatePressed = doUpdatePressed;
            _updateUpdated = doUpdateUpdated;
            _updateReleased = doUpdateReleased;
            _updateCancelled = doUpdateCancelled;

            gestureListPool.WarmUp(5);
            pointerListPool.WarmUp(10);
            transformListPool.WarmUp(5);
        }

        private void OnEnable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted += frameStartedHandler;
                touchManager.FrameFinished += frameFinishedHandler;
                touchManager.PointersUpdated += PointersUpdatedHandler;
                touchManager.PointersPressed += pointersPressedHandler;
                touchManager.PointersReleased += pointersReleasedHandler;
                touchManager.PointersCancelled += pointersCancelledHandler;
            }
        }

        private void OnDisable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted -= frameStartedHandler;
                touchManager.FrameFinished -= frameFinishedHandler;
                touchManager.PointersUpdated -= PointersUpdatedHandler;
                touchManager.PointersPressed -= pointersPressedHandler;
                touchManager.PointersReleased -= pointersReleasedHandler;
                touchManager.PointersCancelled -= pointersCancelledHandler;
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

        private void doUpdatePressed(Gesture gesture, IList<Pointer> pointers)
        {
            gesture.INTERNAL_PointersPressed(pointers);
        }

        private void doUpdateUpdated(Gesture gesture, IList<Pointer> pointers)
        {
            gesture.INTERNAL_PointersUpdated(pointers);
        }

        private void doUpdateReleased(Gesture gesture, IList<Pointer> pointers)
        {
            gesture.INTERNAL_PointersReleased(pointers);
        }

        private void doUpdateCancelled(Gesture gesture, IList<Pointer> pointers)
        {
            gesture.INTERNAL_PointersCancelled(pointers);
        }

        private void update(IList<Pointer> pointers, Action<Transform> process,
                            Action<Gesture, IList<Pointer>> dispatch)
        {
            // WARNING! Arcane magic ahead!
            // gestures which got any pointers
            // needed because there's no order in dictionary
            activeGestures.Clear();
            var targets = transformListPool.Get();

            // arrange pointers by target
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = pointers[i];
                var target = pointer.GetPressData().Target;
                if (target != null)
                {
                    List<Pointer> list;
                    if (!targetPointers.TryGetValue(target, out list))
                    {
                        list = pointerListPool.Get();
                        targetPointers.Add(target, list);
                        targets.Add(target);
                    }
                    list.Add(pointer);
                }
            }

            // process all targets - get and sort all gestures on targets in hierarchy
            count = targets.Count;
            for (var i = 0; i < count; i++)
            {
                var target = targets[i];
                process(target);
                pointerListPool.Release(targetPointers[target]);
            }
            transformListPool.Release(targets);

            // dispatch gesture events with pointers assigned to them
            count = activeGestures.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGestures[i];
                var list = gesturePointers[gesture];
                if (gestureIsActive(gesture)) dispatch(gesture, list);
                pointerListPool.Release(list);
            }

            targetPointers.Clear();
            gesturePointers.Clear();
        }

        private void processTarget(Transform target)
        {
            var targetList = targetPointers[target];
            var pointerCount = targetList.Count;

            // gestures on objects in the hierarchy from "root" to target
            var list = gestureListPool.Get();
            getHierarchyEndingWith(target, list);

            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = list[i];
                if (!gestureIsActive(gesture)) continue;

                var pointerList = pointerListPool.Get();
                for (var j = 0; j < pointerCount; j++)
                {
                    var pointer = targetList[j];
                    if (gesture.HasPointer(pointer)) pointerList.Add(pointer);
                }

                if (pointerList.Count > 0)
                {
                    if (gesturePointers.ContainsKey(gesture))
                    {
                        gesturePointers[gesture].AddRange(pointerList);
                        pointerListPool.Release(pointerList);
                    }
                    else
                    {
                        activeGestures.Add(gesture);
                        gesturePointers.Add(gesture, pointerList);
                    }
                }
                else
                {
                    pointerListPool.Release(pointerList);
                }
            }
            gestureListPool.Release(list);
        }

        private void processTargetBegan(Transform target)
        {
            var targetList = targetPointers[target];
            var pointerCount = targetList.Count;

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

                var canReceivePointers = true;
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
                        canReceivePointers = false;
                        break;
                    }
                }

                // check gesture's ShouldReceivePointer callback
                if (!canReceivePointers) continue;

                var pointerList = pointerListPool.Get();
                for (var j = 0; j < pointerCount; j++)
                {
                    var pointer = targetList[j];
                    if (shouldReceivePointer(gesture, pointer)) pointerList.Add(pointer);
                }
                if (pointerList.Count > 0)
                {
                    if (gesturePointers.ContainsKey(gesture))
                    {
                        gesturePointers[gesture].AddRange(pointerList);
                        pointerListPool.Release(pointerList);
                    }
                    else
                    {
                        activeGestures.Add(gesture);
                        gesturePointers.Add(gesture, pointerList);
                    }
                }
                else
                {
                    pointerListPool.Release(pointerList);
                }
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

        private bool shouldReceivePointer(Gesture gesture, Pointer pointer)
        {
            bool result = true;
            if (GlobalGestureDelegate != null) result = GlobalGestureDelegate.ShouldReceivePointer(gesture, pointer);
            return result && gesture.ShouldReceivePointer(pointer);
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

        #region Pointer events handlers

        private void frameFinishedHandler(object sender, EventArgs eventArgs)
        {
            resetGestures();
        }

        private void frameStartedHandler(object sender, EventArgs eventArgs)
        {
            resetGestures();
        }

        private void pointersPressedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            update(pointerEventArgs.Pointers, _processTargetBegan, _updatePressed);
        }

        private void PointersUpdatedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            update(pointerEventArgs.Pointers, _processTarget, _updateUpdated);
        }

        private void pointersReleasedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            update(pointerEventArgs.Pointers, _processTarget, _updateReleased);
        }

        private void pointersCancelledHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            update(pointerEventArgs.Pointers, _processTarget, _updateCancelled);
        }

        #endregion
    }
}