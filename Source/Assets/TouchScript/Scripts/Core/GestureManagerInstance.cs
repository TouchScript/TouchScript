/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;

namespace TouchScript.Core
{
    /// <summary>
    /// Internal implementation of <see cref="IGestureManager"/>.
    /// </summary>
    internal sealed class GestureManagerInstance : MonoBehaviour, IGestureManager
    {
        #region Public properties

        /// <summary>
        /// Gets the instance of GestureManager singleton.
        /// </summary>
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

        /// <inheritdoc />
        public IGestureDelegate GlobalGestureDelegate { get; set; }

        #endregion

        #region Private variables

        private static GestureManagerInstance instance;
        private static bool shuttingDown = false;

        // Upcoming changes
        private List<Gesture> gesturesToReset = new List<Gesture>(20);
        private Dictionary<int, List<Gesture>> pointerToGestures = new Dictionary<int, List<Gesture>>(10);

		private CustomSampler gestureSampler;

        #endregion

        #region Temporary collections

        // Temporary collections for update methods.
        // Dictionary<Transform, List<Pointer>> - pointers sorted by targets
        private Dictionary<Transform, List<Pointer>> pointersOnTarget = new Dictionary<Transform, List<Pointer>>(10);
        // Dictionary<Gesture, List<Pointer>> - pointers sorted by gesture
        private Dictionary<Gesture, List<Pointer>> pointersToDispatchForGesture = new Dictionary<Gesture, List<Pointer>>(10);
        private List<Gesture> activeGesturesThisUpdate = new List<Gesture>(20);

        private Dictionary<Transform, List<Gesture>> hierarchyEndingWithCache = new Dictionary<Transform, List<Gesture>>(4);
        private Dictionary<Transform, List<Gesture>> hierarchyBeginningWithCache = new Dictionary<Transform, List<Gesture>>(4);

        #endregion

        #region Pools

        private static ObjectPool<List<Gesture>> gestureListPool = new ObjectPool<List<Gesture>>(10,
            () => new List<Gesture>(10), null, (l) => l.Clear(), "GestureManager/Gesture");

        private static ObjectPool<List<Pointer>> pointerListPool = new ObjectPool<List<Pointer>>(20,
            () => new List<Pointer>(10), null, (l) => l.Clear(), "GestureManager/Pointer");

        private static ObjectPool<List<Transform>> transformListPool = new ObjectPool<List<Transform>>(10,
            () => new List<Transform>(10), null, (l) => l.Clear(), "GestureManager/Transform");

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

            gestureListPool.WarmUp(20);
            pointerListPool.WarmUp(20);
            transformListPool.WarmUp(1);

			gestureSampler = CustomSampler.Create("[TouchScript] Update Gestures");
        }

        private void OnEnable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager != null)
            {
                touchManager.FrameStarted += frameStartedHandler;
                touchManager.FrameFinished += frameFinishedHandler;
                touchManager.PointersUpdated += pointersUpdatedHandler;
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
                touchManager.PointersUpdated -= pointersUpdatedHandler;
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
                case Gesture.GestureState.Idle:
                case Gesture.GestureState.Possible:
                    break;
                case Gesture.GestureState.Began:
                    switch (gesture.State)
                    {
                        case Gesture.GestureState.Idle:
                        case Gesture.GestureState.Possible:
                            break;
                        default:
                            print(string.Format("Gesture {0} erroneously tried to enter state {1} from state {2}",
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
                            print(string.Format("Gesture {0} erroneously tried to enter state {1} from state {2}",
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
                        case Gesture.GestureState.Idle:
                        case Gesture.GestureState.Possible:
                            recognized = recognizeGestureIfNotPrevented(gesture);
                            if (!recognized) return Gesture.GestureState.Failed;
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

        private void updatePressed(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            var activeTargets = transformListPool.Get();
            var gesturesInHierarchy = gestureListPool.Get();
            var startedGestures = gestureListPool.Get();

            // Arrange pointers by target.
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = pointers[i];
                var target = pointer.GetPressData().Target;
                if (target == null) continue;

                List<Pointer> list;
                if (!pointersOnTarget.TryGetValue(target, out list))
                {
                    list = pointerListPool.Get();
                    pointersOnTarget.Add(target, list);
                    activeTargets.Add(target);
                }
                list.Add(pointer);
            }

            // Process all targets - get and sort all gestures on targets in hierarchy.
            count = activeTargets.Count;
            for (var i = 0; i < count; i++)
            {
                var target = activeTargets[i];

                // Pointers that hit <target>.
                var targetPointers = pointersOnTarget[target];
                var targetPointersCount = targetPointers.Count;

                // Gestures on objects in the hierarchy from "root" to target.
                var gesturesOnParentsAndMe = getHierarchyEndingWith(target);

                // Gestures in the target's hierarchy which might affect gestures on the target.
                // Gestures on all parents and all children.
                gesturesInHierarchy.AddRange(gesturesOnParentsAndMe);
                gesturesInHierarchy.AddRange(getHierarchyBeginningWith(target));
                var gesturesInHierarchyCount = gesturesInHierarchy.Count;

                for (var j = 0; j < gesturesInHierarchyCount; j++)
                {
                    var gesture = gesturesInHierarchy[j];
                    if (gesture.State == Gesture.GestureState.Began || gesture.State == Gesture.GestureState.Changed) startedGestures.Add(gesture);
                }

                var startedCount = startedGestures.Count;
                var possibleGestureCount = gesturesOnParentsAndMe.Count;
                for (var j = 0; j < possibleGestureCount; j++)
                {
                    // WARNING! Gesture state might change during this loop.
                    // For example when one of them recognizes.

                    var possibleGesture = gesturesOnParentsAndMe[j];

                    // If the gesture is not active it can't start or recognize.
                    if (!gestureIsActive(possibleGesture)) continue;

                    var canReceivePointers = true;

                    // For every possible gesture in gesturesInHierarchy we need to check if it prevents gestureOnParentOrMe from getting pointers.
                    for (var k = 0; k < startedCount; k++)
                    {
                        var startedGesture = startedGestures[k];

                        if (possibleGesture == startedGesture) continue;

                        // This gesture has started. Is gestureOnParentOrMe allowed to work in parallel?
                        if (canPreventGesture(startedGesture, possibleGesture))
                        {
                            // activeGesture has already began and prevents gestureOnParentOrMe from getting pointers.
                            canReceivePointers = false;
                            break;
                        }
                    }

                    if (!canReceivePointers) continue;

                    // Filter incoming pointers for gesture.
                    var pointersSentToGesture = pointerListPool.Get();
                    for (var k = 0; k < targetPointersCount; k++)
                    {
                        var pointer = targetPointers[k];
                        if (shouldReceivePointer(possibleGesture, pointer)) pointersSentToGesture.Add(pointer);
                    }

                    // If there are any pointers to send.
                    if (pointersSentToGesture.Count > 0)
                    {
                        if (pointersToDispatchForGesture.ContainsKey(possibleGesture))
                        {
                            pointersToDispatchForGesture[possibleGesture].AddRange(pointersSentToGesture);
                            pointerListPool.Release(pointersSentToGesture);
                        }
                        else
                        {
                            // Add gesture to the list of active gestures this update.
                            activeGesturesThisUpdate.Add(possibleGesture);
                            pointersToDispatchForGesture.Add(possibleGesture, pointersSentToGesture);
                        }
                    }
                    else
                    {
                        pointerListPool.Release(pointersSentToGesture);
                    }
                }

                gesturesInHierarchy.Clear();
                startedGestures.Clear();
                pointerListPool.Release(targetPointers);
            }

            gestureListPool.Release(gesturesInHierarchy);
            gestureListPool.Release(startedGestures);
            transformListPool.Release(activeTargets);

            // Dispatch gesture events with pointers assigned to them.
            count = activeGesturesThisUpdate.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGesturesThisUpdate[i];
                var list = pointersToDispatchForGesture[gesture];
                if (!gestureIsActive(gesture))
                {
                    pointerListPool.Release(list);
                    continue;
                }

                var numPointers = list.Count;
                for (var j = 0; j < numPointers; j++)
                {
                    var pointer = list[j];
                    List<Gesture> gestureList;
                    if (!pointerToGestures.TryGetValue(pointer.Id, out gestureList))
                    {
                        gestureList = gestureListPool.Get();
                        pointerToGestures.Add(pointer.Id, gestureList);
                    }
                    gestureList.Add(gesture);
                }

                gesture.INTERNAL_PointersPressed(list);
                pointerListPool.Release(list);
            }

            pointersOnTarget.Clear();
            activeGesturesThisUpdate.Clear();
            pointersToDispatchForGesture.Clear();

			gestureSampler.End();
        }

        private void updateUpdated(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            sortPointersForActiveGestures(pointers);

            var count = activeGesturesThisUpdate.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGesturesThisUpdate[i];
                var list = pointersToDispatchForGesture[gesture];
                if (gestureIsActive(gesture))
                {
                    gesture.INTERNAL_PointersUpdated(list);
                }
                pointerListPool.Release(list);
            }

            activeGesturesThisUpdate.Clear();
            pointersToDispatchForGesture.Clear();

			gestureSampler.End();
        }

        private void updateReleased(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            sortPointersForActiveGestures(pointers);

            var count = activeGesturesThisUpdate.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGesturesThisUpdate[i];
                var list = pointersToDispatchForGesture[gesture];
                if (gestureIsActive(gesture))
                {
                    gesture.INTERNAL_PointersReleased(list);
                }
                pointerListPool.Release(list);
            }

            removePointers(pointers);
            activeGesturesThisUpdate.Clear();
            pointersToDispatchForGesture.Clear();

			gestureSampler.End();
        }

        private void updateCancelled(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            sortPointersForActiveGestures(pointers);

            var count = activeGesturesThisUpdate.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = activeGesturesThisUpdate[i];
                var list = pointersToDispatchForGesture[gesture];
                if (gestureIsActive(gesture))
                {
                    gesture.INTERNAL_PointersCancelled(list);
                }
                pointerListPool.Release(list);
            }

            removePointers(pointers);
            activeGesturesThisUpdate.Clear();
            pointersToDispatchForGesture.Clear();

			gestureSampler.End();
        }

        private void sortPointersForActiveGestures(IList<Pointer> pointers)
        {
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = pointers[i];
                List<Gesture> gestures;
                if (!pointerToGestures.TryGetValue(pointer.Id, out gestures)) continue;

                var gestureCount = gestures.Count;
                for (var j = 0; j < gestureCount; j++)
                {
                    var gesture = gestures[j];
                    List<Pointer> toDispatch;
                    if (!pointersToDispatchForGesture.TryGetValue(gesture, out toDispatch))
                    {
                        toDispatch = pointerListPool.Get();
                        pointersToDispatchForGesture.Add(gesture, toDispatch);
                        activeGesturesThisUpdate.Add(gesture);
                    }
                    toDispatch.Add(pointer);
                }
            }
        }

        private void removePointers(IList<Pointer> pointers)
        {
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = pointers[i];
                List<Gesture> list;
                if (!pointerToGestures.TryGetValue(pointer.Id, out list)) continue;

                pointerToGestures.Remove(pointer.Id);
                gestureListPool.Release(list);
            }
        }

        private void resetGestures()
        {
            if (gesturesToReset.Count == 0) return;

            var count = gesturesToReset.Count;
            for (var i = 0; i < count; i++)
            {
                var gesture = gesturesToReset[i];
                if (Equals(gesture, null)) continue; // Reference comparison

                var activePointers = gesture.ActivePointers;
                var activeCount = activePointers.Count;
                for (var j = 0; j < activeCount; j++)
                {
                    var pointer = activePointers[j];
                    List<Gesture> list;
                    if (pointerToGestures.TryGetValue(pointer.Id, out list)) list.Remove(gesture);
                }

                if (gesture == null) continue; // Unity "null" comparison
                gesture.INTERNAL_Reset();
                gesture.INTERNAL_SetState(Gesture.GestureState.Idle);
            }
            gesturesToReset.Clear();
        }

        private void clearFrameCaches()
        {
			foreach (var kv in hierarchyEndingWithCache) gestureListPool.Release(kv.Value);
			foreach (var kv in hierarchyBeginningWithCache) gestureListPool.Release(kv.Value);
            hierarchyEndingWithCache.Clear();
            hierarchyBeginningWithCache.Clear();
        }

        // parent <- parent <- target
        private List<Gesture> getHierarchyEndingWith(Transform target)
        {
            List<Gesture> list;
            if (hierarchyEndingWithCache.TryGetValue(target, out list)) return list;

            list = gestureListPool.Get();
            target.GetComponentsInParent(false, list);
            hierarchyEndingWithCache.Add(target, list);

            return list;
        }

        // target <- child*
        private List<Gesture> getHierarchyBeginningWith(Transform target)
        {
            List<Gesture> list;
            if (hierarchyBeginningWithCache.TryGetValue(target, out list)) return list;

            list = gestureListPool.Get();
            target.GetComponentsInChildren(list);
            hierarchyBeginningWithCache.Add(target, list);

            return list;
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
            bool canRecognize = true;
            var target = gesture.transform;

            var gesturesInHierarchy = gestureListPool.Get();
            gesturesInHierarchy.AddRange(getHierarchyEndingWith(target));
            gesturesInHierarchy.AddRange(getHierarchyBeginningWith(target));

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
                else if (otherGesture.State == Gesture.GestureState.Possible)
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
            clearFrameCaches();
        }

        private void frameStartedHandler(object sender, EventArgs eventArgs)
        {
            resetGestures();
        }

        private void pointersPressedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            updatePressed(pointerEventArgs.Pointers);
        }

        private void pointersUpdatedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            updateUpdated(pointerEventArgs.Pointers);
        }

        private void pointersReleasedHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            updateReleased(pointerEventArgs.Pointers);
        }

        private void pointersCancelledHandler(object sender, PointerEventArgs pointerEventArgs)
        {
            updateCancelled(pointerEventArgs.Pointers);
        }

        #endregion
    }
}