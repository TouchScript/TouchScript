/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers;
using TouchScript.Utils;
using TouchScript.Utils.Editor.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Base class for all gestures
    /// </summary>
    public abstract class Gesture : MonoBehaviour
    {
        #region Constants

        public const string STATE_CHANGED_MESSAGE = "OnGestureStateChanged";

        /// <summary>
        /// Possible states of a gesture.
        /// </summary>
        public enum GestureState
        {
            /// <summary>
            /// Gesture is possible
            /// </summary>
            Possible,

            /// <summary>
            /// Continuous gesture has just begun
            /// </summary>
            Began,

            /// <summary>
            /// Started continuous gesture is updated
            /// </summary>
            Changed,

            /// <summary>
            /// Continuous gesture is ended
            /// </summary>
            Ended,

            /// <summary>
            /// Gesture is cancelled
            /// </summary>
            Cancelled,

            /// <summary>
            /// Gesture is failed by itself or by another recognized gesture
            /// </summary>
            Failed,

            /// <summary>
            /// Gesture is recognized
            /// </summary>
            Recognized = Ended
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture changes state.
        /// </summary>
        public event EventHandler<GestureStateChangeEventArgs> StateChanged
        {
            add { stateChangedInvoker += value; }
            remove { stateChangedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<GestureStateChangeEventArgs> stateChangedInvoker;

        #endregion

        #region Public properties

        public Gesture RequireGestureToFail
        {
            get { return requireGestureToFail; }
            set
            {
                if (requireGestureToFail != null) requireGestureToFail.StateChanged -= requiredToFailGestureStateChangedHandler;
                requireGestureToFail = value;
                if (requireGestureToFail != null) requireGestureToFail.StateChanged += requiredToFailGestureStateChangedHandler;
            }
        }

        public bool CombineTouchPoints
        {
            get { return combineTouchPoints; }
            set { combineTouchPoints = value; }
        }

        /// <summary>
        /// Time interval before gesture is recognized to combine all lifted touch points into a cluster and calculate their screen positions.
        /// </summary>
        public float CombineTouchPointsInterval
        {
            get { return combineTouchPointsInterval; }
            set { combineTouchPointsInterval = value; }
        }

        public bool UseSendMessage
        {
            get { return useSendMessage; }
            set { useSendMessage = value; }
        }

        public bool SendStateChangeMessages
        {
            get { return sendStateChangeMessages; }
            set { sendStateChangeMessages = value; }
        }

        public GameObject SendMessageTarget
        {
            get { return sendMessageTarget; }
            set
            {
                sendMessageTarget = value;
                if (value == null) sendMessageTarget = gameObject;
            }
        }

        /// <summary>
        /// Current gesture state.
        /// </summary>
        public GestureState State
        {
            get { return state; }
            private set
            {
                PreviousState = state;
                state = value;

                switch (value)
                {
                    case GestureState.Possible:
                        onPossible();
                        break;
                    case GestureState.Began:
                        onBegan();
                        break;
                    case GestureState.Changed:
                        onChanged();
                        break;
                    case GestureState.Recognized:
                        onRecognized();
                        break;
                    case GestureState.Failed:
                        onFailed();
                        break;
                    case GestureState.Cancelled:
                        onCancelled();
                        break;
                }

                if (stateChangedInvoker != null) stateChangedInvoker(this, new GestureStateChangeEventArgs(state, PreviousState));
                if (useSendMessage && sendStateChangeMessages) sendMessageTarget.SendMessage(STATE_CHANGED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Previous gesture state.
        /// </summary>
        public GestureState PreviousState { get; private set; }

        /// <summary>
        /// Transformation center in screen coordinates.
        /// </summary>
        public virtual Vector2 ScreenPosition
        {
            get
            {
                if (touchPoints.Count == 0)
                {
                    if (!TouchManager.IsInvalidPosition(cachedScreenPosition)) return cachedScreenPosition;
                    return TouchManager.INVALID_POSITION;
                }
                return ClusterUtils.Get2DCenterPosition(touchPoints);
            }
        }

        /// <summary>
        /// Previous transformation center in screen coordinates.
        /// </summary>
        public virtual Vector2 PreviousScreenPosition
        {
            get
            {
                if (touchPoints.Count == 0)
                {
                    if (!TouchManager.IsInvalidPosition(cachedPreviousScreenPosition)) return cachedPreviousScreenPosition;
                    return TouchManager.INVALID_POSITION;
                }
                return ClusterUtils.GetPrevious2DCenterPosition(touchPoints);
            }
        }

        /// <summary>
        /// Transformation center in normalized screen coordinates.
        /// </summary>
        public Vector2 NormalizedScreenPosition
        {
            get
            {
                var position = ScreenPosition;
                if (TouchManager.IsInvalidPosition(position)) return TouchManager.INVALID_POSITION;
                return new Vector2(position.x/Screen.width, position.y/Screen.height);
            }
        }

        /// <summary>
        /// Previous center in screen coordinates.
        /// </summary>
        public Vector2 PreviousNormalizedScreenPosition
        {
            get
            {
                var position = PreviousScreenPosition;
                if (TouchManager.IsInvalidPosition(position)) return TouchManager.INVALID_POSITION;
                return new Vector2(position.x/Screen.width, position.y/Screen.height);
            }
        }

        /// <summary>
        /// List of gesture's active touch points.
        /// </summary>
        public IList<ITouchPoint> TouchPoints
        {
            get { return touchPoints.AsReadOnly(); }
        }

        /// <summary>
        /// An object implementing <see cref="IGestureDelegate"/> to be asked for gesture specific actions.
        /// </summary>
        public IGestureDelegate Delegate { get; set; }

        #endregion

        #region Private variables

        /// <summary>
        /// Reference to global GestureManager.
        /// </summary>
        protected IGestureManager gestureManager
        {
            // implemented as a property because it returns IGestureManager but we need to reference GestureManagerInstance to access internal methods
            get { return gestureManagerInstance; }
        }

        /// <summary>
        /// Reference to global TouchManager.
        /// </summary>
        protected ITouchManager touchManager { get; private set; }

        /// <summary>
        /// Touch points the gesture currently owns and works with.
        /// </summary>
        protected List<ITouchPoint> touchPoints = new List<ITouchPoint>();

        [SerializeField]
        [ToggleLeft]
        private bool combineTouchPoints = false;

        [SerializeField]
        private float combineTouchPointsInterval = .3f;

        [SerializeField]
        [ToggleLeft]
        private bool useSendMessage = false;

        [SerializeField]
        [ToggleLeft]
        private bool sendStateChangeMessages = false;

        [SerializeField]
        private GameObject sendMessageTarget;

        [SerializeField]
        [NullToggle]
        private Gesture requireGestureToFail;

        [SerializeField]
        private List<Gesture> friendlyGestures = new List<Gesture>();

        private List<int> friendlyGestureIds = new List<int>();

        private TimedTouchSequence touchSequence = new TimedTouchSequence();
        private GestureManagerInstance gestureManagerInstance;
        private GestureState delayedStateChange = GestureState.Possible;
        private bool requiredGestureFailed = false;
        private GestureState state = GestureState.Possible;

        /// <summary>
        /// Cached screen position. 
        /// Used to keep tap's position which can't be calculated from touch points when the gesture is recognized since all touch points are gone.
        /// </summary>
        private Vector2 cachedScreenPosition;

        /// <summary>
        /// Cached previous screen position.
        /// Used to keep tap's position which can't be calculated from touch points when the gesture is recognized since all touch points are gone.
        /// </summary>
        private Vector2 cachedPreviousScreenPosition;

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a friendly gesture.
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        public virtual void AddFriendlyGesture(Gesture gesture)
        {
            if (gesture == null || gesture == this) return;

            registerFriendlyGesture(gesture);
            gesture.registerFriendlyGesture(this);
        }

        /// <summary>
        /// Gets result of casting a ray from gesture touch points' centroid screen position.
        /// </summary>
        /// <returns>true if ray hits gesture's target; otherwise, false.</returns>
        public virtual bool GetTargetHitResult()
        {
            ITouchHit hit;
            return GetTargetHitResult(ScreenPosition, out hit);
        }

        /// <summary>
        /// Gets result of casting a ray from gesture touch points centroid screen position.
        /// </summary>
        /// <param name="hit">Raycast result</param>
        /// <returns>true if ray hits gesture's target; otherwise, false.</returns>
        public virtual bool GetTargetHitResult(out ITouchHit hit)
        {
            return GetTargetHitResult(ScreenPosition, out hit);
        }

        /// <summary>
        /// Gets result of casting a ray from specific screen position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>true if ray hits gesture's target; otherwise, false.</returns>
        public virtual bool GetTargetHitResult(Vector2 position)
        {
            ITouchHit hit;
            return GetTargetHitResult(position, out hit);
        }

        /// <summary>
        /// Gets result of casting a ray from specific screen position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="hit">Raycast result.</param>
        /// <returns>true if ray hits gesture's target; otherwise, false.</returns>
        public virtual bool GetTargetHitResult(Vector2 position, out ITouchHit hit)
        {
            TouchLayer layer = null;
            if (!touchManager.GetHitTarget(position, out hit, out layer)) return false;

            if (transform == hit.Transform || hit.Transform.IsChildOf(transform)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether gesture controls a touch point.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <returns>
        ///   <c>true</c> if gesture controls the touch point; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTouchPoint(ITouchPoint touch)
        {
            return touchPoints.Contains(touch);
        }

        /// <summary>
        /// Determines whether this instance can prevent the specified gesture.
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <returns>
        ///   <c>true</c> if this instance can prevent the specified gesture; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null)
            {
                if (gesture.CanBePreventedByGesture(this)) return !IsFriendly(gesture);
                return false;
            }
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <summary>
        /// Determines whether this instance can be prevented by specified gesture.
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be prevented by specified gesture; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return !IsFriendly(gesture);
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <summary>
        /// Specifies if gesture can receive this specific touch point.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <returns><c>true</c> if this touch should be received by the gesture; otherwise, <c>false</c>.</returns>
        public virtual bool ShouldReceiveTouch(ITouchPoint touch)
        {
            if (Delegate == null) return true;
            return Delegate.ShouldReceiveTouch(this, touch);
        }

        /// <summary>
        /// Specifies if gesture can begin or recognize.
        /// </summary>
        /// <returns><c>true</c> if gesture should begin; otherwise, <c>false</c>.</returns>
        public virtual bool ShouldBegin()
        {
            if (Delegate == null) return true;
            return Delegate.ShouldBegin(this);
        }

        #endregion

        #region Unity methods

        protected virtual void Awake()
        {
            foreach (var gesture in friendlyGestures)
            {
                AddFriendlyGesture(gesture);
            }
            RequireGestureToFail = requireGestureToFail;
        }

        /// <summary>
        /// Unity3d Start handler.
        /// </summary>
        protected virtual void OnEnable()
        {
            // TouchManager might be different in another scene
            touchManager = TouchManager.Instance;
            gestureManagerInstance = GestureManager.Instance as GestureManagerInstance;

            if (touchManager == null) Debug.LogError("No TouchManager found! Please add an instance of TouchManager to the scene!");
            if (gestureManagerInstance == null) Debug.LogError("No GesturehManager found! Please add an instance of GesturehManager to the scene!");

            if (sendMessageTarget == null) sendMessageTarget = gameObject;
            Reset();
        }

        /// <summary>
        /// Unity3d OnDisable handler.
        /// </summary>
        protected virtual void OnDisable()
        {
            setState(GestureState.Failed);

            gestureManagerInstance = null;
            touchManager = null;
        }

        protected virtual void OnDestroy()
        {
            var copy = new List<Gesture>(friendlyGestures);
            foreach (var gesture in copy)
            {
                RemoveFriendlyGesture(gesture);
            }
            RequireGestureToFail = null;
        }

        #endregion

        #region Internal functions

        internal void SetState(GestureState value)
        {
            setState(value);
        }

        internal void Reset()
        {
            touchPoints.Clear();
            delayedStateChange = GestureState.Possible;
            requiredGestureFailed = false;
            reset();
        }

        internal void TouchesBegan(IList<ITouchPoint> touches)
        {
            touchPoints.AddRange(touches);
            touchesBegan(touches);
        }

        internal void TouchesMoved(IList<ITouchPoint> touches)
        {
            touchesMoved(touches);
        }

        internal void TouchesEnded(IList<ITouchPoint> touches)
        {
            touchPoints.RemoveAll(touches.Contains);
            touchesEnded(touches);
        }

        internal void TouchesCancelled(IList<ITouchPoint> touches)
        {
            touchPoints.RemoveAll(touches.Contains);
            touchesCancelled(touches);
        }

        internal virtual void RemoveFriendlyGesture(Gesture gesture)
        {
            if (gesture == null || gesture == this) return;

            unregisterFriendlyGesture(gesture);
            gesture.unregisterFriendlyGesture(this);
        }

        internal bool IsFriendly(Gesture gesture)
        {
            return friendlyGestureIds.Contains(gesture.GetInstanceID());
        }

        #endregion

        #region Protected methods

        protected virtual bool shouldCacheTouchPointPosition(ITouchPoint value)
        {
            return true;
        }

        /// <summary>
        /// Tries to change gesture state.
        /// </summary>
        /// <param name="value">New state.</param>
        /// <returns><c>true</c> if state was changed; otherwise, <c>false</c>.</returns>
        protected bool setState(GestureState value)
        {
            if (gestureManagerInstance == null) return false;
            if (requireGestureToFail != null)
            {
                switch (value)
                {
                    case GestureState.Recognized:
                    case GestureState.Began:
                        if (!requiredGestureFailed)
                        {
                            delayedStateChange = value;
                            return false;
                        }
                        break;
                    case GestureState.Possible:
                        delayedStateChange = GestureState.Possible;
                        break;
                }
            }

            var newState = gestureManagerInstance.GestureChangeState(this, value);
            State = newState;

            return value == newState;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when new touches appear.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesBegan(IList<ITouchPoint> touches)
        {}

        /// <summary>
        /// Called for moved touches.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesMoved(IList<ITouchPoint> touches)
        {}

        /// <summary>
        /// Called if touches are removed.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesEnded(IList<ITouchPoint> touches)
        {
            if (combineTouchPoints)
            {
                foreach (var touch in touches)
                {
                    touchSequence.Add(touch, Time.time);
                }

                if (touchPoints.Count == 0)
                {
                    // Checking which points were removed in clusterExistenceTime seconds to set their centroid as cached screen position
                    var cluster = touchSequence.FindTouchPointsLaterThan(Time.time - combineTouchPointsInterval, shouldCacheTouchPointPosition);
                    cachedScreenPosition = ClusterUtils.Get2DCenterPosition(cluster);
                    cachedPreviousScreenPosition = ClusterUtils.GetPrevious2DCenterPosition(cluster);
                }
            } else
            {
                if (touchPoints.Count == 0)
                {
                    var lastPoint = touches[touches.Count - 1];
                    if (shouldCacheTouchPointPosition(lastPoint))
                    {
                        cachedScreenPosition = lastPoint.Position;
                        cachedPreviousScreenPosition = lastPoint.PreviousPosition;
                    } else
                    {
                        cachedScreenPosition = TouchManager.INVALID_POSITION;
                        cachedPreviousScreenPosition = TouchManager.INVALID_POSITION;
                    }
                }
            }
        }

        /// <summary>
        /// Called when touches are cancelled.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesCancelled(IList<ITouchPoint> touches)
        {}

        /// <summary>
        /// Called to reset gesture state after it fails or recognizes.
        /// </summary>
        protected virtual void reset()
        {}

        /// <summary>
        /// Called when state is changed to Possible.
        /// </summary>
        protected virtual void onPossible()
        {}

        /// <summary>
        /// Called when state is changed to Began.
        /// </summary>
        protected virtual void onBegan()
        {}

        /// <summary>
        /// Called when state is changed to Changed.
        /// </summary>
        protected virtual void onChanged()
        {}

        /// <summary>
        /// Called when state is changed to Recognized.
        /// </summary>
        protected virtual void onRecognized()
        {}

        /// <summary>
        /// Called when state is changed to Failed.
        /// </summary>
        protected virtual void onFailed()
        {}

        /// <summary>
        /// Called when state is changed to Cancelled.
        /// </summary>
        protected virtual void onCancelled()
        {}

        #endregion

        #region Private functions

        private void registerFriendlyGesture(Gesture gesture)
        {
            if (gesture == null || gesture == this || friendlyGestures.Contains(gesture)) return;

            friendlyGestures.Add(gesture);
        }

        private void unregisterFriendlyGesture(Gesture gesture)
        {
            friendlyGestures.Remove(gesture);
        }

        #endregion

        #region Event handlers

        private void requiredToFailGestureStateChangedHandler(object sender, GestureStateChangeEventArgs e)
        {
            if (sender != requireGestureToFail) return;
            switch (e.State)
            {
                case GestureState.Failed:
                    requiredGestureFailed = true;
                    if (delayedStateChange != GestureState.Possible)
                    {
                        setState(delayedStateChange);
                    }
                    break;
                case GestureState.Began:
                case GestureState.Recognized:
                    setState(GestureState.Failed);
                    break;
            }
        }

        #endregion
    }

    public class GestureStateChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Previous gesture state.
        /// </summary>
        public Gesture.GestureState PreviousState { get; private set; }

        /// <summary>
        /// Current gesture state.
        /// </summary>
        public Gesture.GestureState State { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureStateChangeEventArgs"/> class.
        /// </summary>
        /// <param name="state">Current gesture state.</param>
        /// <param name="previousState">Previous gesture state.</param>
        public GestureStateChangeEventArgs(Gesture.GestureState state, Gesture.GestureState previousState)
        {
            State = state;
            PreviousState = previousState;
        }
    }

}