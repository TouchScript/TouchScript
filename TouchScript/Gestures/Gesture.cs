/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TouchScript.Hit;
using TouchScript.Layers;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Base class for all gestures
    /// </summary>
    public abstract class Gesture : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Message sent when gesture changes state if SendMessage is used.
        /// </summary>
        public const string STATE_CHANGE_MESSAGE = "OnGestureStateChange";

        /// <summary>
        /// Possible states of a gesture.
        /// </summary>
        public enum GestureState
        {
            /// <summary>
            /// Gesture is possible.
            /// </summary>
            Possible,

            /// <summary>
            /// Continuous gesture has just begun.
            /// </summary>
            Began,

            /// <summary>
            /// Started continuous gesture is updated.
            /// </summary>
            Changed,

            /// <summary>
            /// Continuous gesture is ended.
            /// </summary>
            Ended,

            /// <summary>
            /// Gesture is cancelled.
            /// </summary>
            Cancelled,

            /// <summary>
            /// Gesture is failed by itself or by another recognized gesture.
            /// </summary>
            Failed,

            /// <summary>
            /// Gesture is recognized.
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

        /// <summary>
        /// Gets or sets another gesture which must fail before this gesture can be recognized.
        /// </summary>
        /// <value>
        /// The gesture which must fail before this gesture can be recognized;
        /// </value>
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

        /// <summary>
        /// Gets or sets the flag if touches should be treated as a cluster.
        /// </summary>
        /// <value><c>true</c> if touches should be treated as a cluster; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// At the end of a gesture when touches are lifted off due to the fact that computers are faster than humans the very last touch's position will be gesture's <see cref="ScreenPosition"/> after that. This flag is used to combine several touches which from the point of a user were lifted off simultaneously and set their centroid as gesture's <see cref="ScreenPosition"/>.
        /// </remarks>
        public bool CombineTouches
        {
            get { return combineTouches; }
            set { combineTouches = value; }
        }

        /// <summary>
        /// Gets or sets time interval before gesture is recognized to combine all lifted touch points into a cluster to use its center as <see cref="ScreenPosition"/>.
        /// </summary>
        /// <value>Time in seconds to treat touches lifted off during this interval as a single gesture.</value>
        public float CombineTouchesInterval
        {
            get { return combineTouchesInterval; }
            set { combineTouchesInterval = value; }
        }

        /// <summary>
        /// Gets or sets whether gesture should use Unity's SendMessage in addition to C# events.
        /// </summary>
        /// <value><c>true</c> if gesture uses SendMessage; otherwise, <c>false</c>.</value>
        public bool UseSendMessage
        {
            get { return useSendMessage; }
            set { useSendMessage = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether state change events are broadcasted if <see cref="UseSendMessage"/> is true..
        /// </summary>
        /// <value><c>true</c> if state change events should be broadcaster; otherwise, <c>false</c>.</value>
        public bool SendStateChangeMessages
        {
            get { return sendStateChangeMessages; }
            set { sendStateChangeMessages = value; }
        }

        /// <summary>
        /// Gets or sets the target of Unity messages sent from this gesture.
        /// </summary>
        /// <value>The target of Unity messages.</value>
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
        /// Gets current gesture state.
        /// </summary>
        /// <value>Current state of the gesture.</value>
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

                stateChangedInvoker.InvokeHandleExceptions(this, new GestureStateChangeEventArgs(state, PreviousState));
                if (useSendMessage && sendStateChangeMessages && SendMessageTarget != null) sendMessageTarget.SendMessage(STATE_CHANGE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Gets previous gesture state.
        /// </summary>
        /// <value>Previous state of the gesture.</value>
        public GestureState PreviousState { get; private set; }

        /// <summary>
        /// Gets current screen position.
        /// </summary>
        /// <value>Gesture's position in screen coordinates.</value>
        public virtual Vector2 ScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0)
                {
                    if (!TouchManager.IsInvalidPosition(cachedScreenPosition)) return cachedScreenPosition;
                    return TouchManager.INVALID_POSITION;
                }
                return ClusterUtils.Get2DCenterPosition(activeTouches);
            }
        }

        /// <summary>
        /// Gets previous screen position.
        /// </summary>
        /// <value>Gesture's previous position in screen coordinates.</value>
        public virtual Vector2 PreviousScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0)
                {
                    if (!TouchManager.IsInvalidPosition(cachedPreviousScreenPosition)) return cachedPreviousScreenPosition;
                    return TouchManager.INVALID_POSITION;
                }
                return ClusterUtils.GetPrevious2DCenterPosition(activeTouches);
            }
        }

        /// <summary>
        /// Gets normalized screen position.
        /// </summary>
        /// <value>Gesture's position in normalized screen coordinates.</value>
        public Vector2 NormalizedScreenPosition
        {
            get
            {
                var position = ScreenPosition;
                if (TouchManager.IsInvalidPosition(position)) return TouchManager.INVALID_POSITION;
                return new Vector2(position.x / Screen.width, position.y / Screen.height);
            }
        }

        /// <summary>
        /// Gets previous screen position.
        /// </summary>
        /// <value>Gesture's previous position in normalized screen coordinates.</value>
        public Vector2 PreviousNormalizedScreenPosition
        {
            get
            {
                var position = PreviousScreenPosition;
                if (TouchManager.IsInvalidPosition(position)) return TouchManager.INVALID_POSITION;
                return new Vector2(position.x / Screen.width, position.y / Screen.height);
            }
        }

        /// <summary>
        /// Gets list of gesture's active touch points.
        /// </summary>
        /// <value>The list of touches owned by this gesture.</value>
        public IList<ITouch> ActiveTouches
        {
            get { return new ReadOnlyCollection<ITouch>(activeTouches); }
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
        protected List<ITouch> activeTouches = new List<ITouch>();

        /// <summary>
        /// Cached transform of the parent object.
        /// </summary>
        protected Transform cachedTransform;

#pragma warning disable 0169
        [SerializeField]
        private bool advancedProps; // is used to save if advanced properties are opened or closed
#pragma warning restore 0169

        [SerializeField]
        [ToggleLeft]
        private bool combineTouches = false;

        [SerializeField]
        private float combineTouchesInterval = .3f;

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
        // Serialized list of gestures for Unity IDE.
        private List<Gesture> friendlyGestures = new List<Gesture>();

        // List of gestures for realtime.
        private List<int> friendlyGestureIds = new List<int>();

        private TimedSequence<ITouch> touchSequence = new TimedSequence<ITouch>();
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
        /// Checks if a gesture is friendly with this gesture.
        /// </summary>
        /// <param name="gesture">A gesture to check.</param>
        /// <returns>True if gestures are friendly; false otherwise.</returns>
        public bool IsFriendly(Gesture gesture)
        {
            return friendlyGestureIds.Contains(gesture.GetInstanceID());
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

            if (cachedTransform == hit.Transform || hit.Transform.IsChildOf(cachedTransform)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether gesture controls a touch point.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <returns>
        ///   <c>true</c> if gesture controls the touch point; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTouch(ITouch touch)
        {
            return activeTouches.Contains(touch);
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
        public virtual bool ShouldReceiveTouch(ITouch touch)
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

        /// <inheritdoc />
        protected virtual void Awake()
        {
            cachedTransform = GetComponent<Transform>();

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
        }

        /// <summary>
        /// Unity3d OnDestroy handler.
        /// </summary>
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
            activeTouches.Clear();
            delayedStateChange = GestureState.Possible;
            requiredGestureFailed = false;
            reset();
        }

        internal void TouchesBegan(IList<ITouch> touches)
        {
            activeTouches.AddRange(touches);
            touchesBegan(touches);
        }

        internal void TouchesMoved(IList<ITouch> touches)
        {
            touchesMoved(touches);
        }

        internal void TouchesEnded(IList<ITouch> touches)
        {
            for (var i = 0; i < touches.Count; i++) activeTouches.Remove(touches[i]);
            touchesEnded(touches);
        }

        internal void TouchesCancelled(IList<ITouch> touches)
        {
            for (var i = 0; i < touches.Count; i++) activeTouches.Remove(touches[i]);
            touchesCancelled(touches);
        }

        internal virtual void RemoveFriendlyGesture(Gesture gesture)
        {
            if (gesture == null || gesture == this) return;

            unregisterFriendlyGesture(gesture);
            gesture.unregisterFriendlyGesture(this);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Should the gesture cache this touch to use it later in calculation of <see cref="ScreenPosition"/>.
        /// </summary>
        /// <param name="value">Touch to cache.</param>
        /// <returns><c>true</c> if touch should be cached; <c>false</c> otherwise.</returns>
        protected virtual bool shouldCacheTouchPosition(ITouch value)
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
                    case GestureState.Failed:
                    case GestureState.Cancelled:
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
        protected virtual void touchesBegan(IList<ITouch> touches) {}

        /// <summary>
        /// Called for moved touches.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesMoved(IList<ITouch> touches) {}

        /// <summary>
        /// Called if touches are removed.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesEnded(IList<ITouch> touches)
        {
            if (combineTouches)
            {
                foreach (var touch in touches)
                {
                    touchSequence.Add(touch);
                }

                if (activeTouches.Count == 0)
                {
                    // Checking which points were removed in clusterExistenceTime seconds to set their centroid as cached screen position
                    var cluster = touchSequence.FindElementsLaterThan(Time.time - combineTouchesInterval, shouldCacheTouchPosition);
                    cachedScreenPosition = ClusterUtils.Get2DCenterPosition(cluster);
                    cachedPreviousScreenPosition = ClusterUtils.GetPrevious2DCenterPosition(cluster);
                }
            }
            else
            {
                if (activeTouches.Count == 0)
                {
                    var lastPoint = touches[touches.Count - 1];
                    if (shouldCacheTouchPosition(lastPoint))
                    {
                        cachedScreenPosition = lastPoint.Position;
                        cachedPreviousScreenPosition = lastPoint.PreviousPosition;
                    }
                    else
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
        protected virtual void touchesCancelled(IList<ITouch> touches) {}

        /// <summary>
        /// Called to reset gesture state after it fails or recognizes.
        /// </summary>
        protected virtual void reset()
        {
            cachedScreenPosition = TouchManager.INVALID_POSITION;
            cachedPreviousScreenPosition = TouchManager.INVALID_POSITION;
        }

        /// <summary>
        /// Called when state is changed to Possible.
        /// </summary>
        protected virtual void onPossible() {}

        /// <summary>
        /// Called when state is changed to Began.
        /// </summary>
        protected virtual void onBegan() {}

        /// <summary>
        /// Called when state is changed to Changed.
        /// </summary>
        protected virtual void onChanged() {}

        /// <summary>
        /// Called when state is changed to Recognized.
        /// </summary>
        protected virtual void onRecognized() {}

        /// <summary>
        /// Called when state is changed to Failed.
        /// </summary>
        protected virtual void onFailed() {}

        /// <summary>
        /// Called when state is changed to Cancelled.
        /// </summary>
        protected virtual void onCancelled() {}

        #endregion

        #region Private functions

        private void registerFriendlyGesture(Gesture gesture)
        {
            if (gesture == null || gesture == this) return;

            addFriendlyGestureId(gesture);
            if (!friendlyGestures.Contains(gesture)) friendlyGestures.Add(gesture);
        }

        // Gets also called by the custom inspector.
        private void addFriendlyGestureId(Gesture gesture)
        {
            var id = gesture.GetInstanceID();
            if (!friendlyGestureIds.Contains(id)) friendlyGestureIds.Add(id);
        }

        private void unregisterFriendlyGesture(Gesture gesture)
        {
            if (gesture == null || gesture == this) return;

            removeFriendlyGestureId(gesture);
            friendlyGestures.Remove(gesture);
        }

        // Gets also called by the custom inspector.
        private void removeFriendlyGestureId(Gesture gesture)
        {
            friendlyGestureIds.Remove(gesture.GetInstanceID());
        }

        #endregion

        #region Event handlers

        private void requiredToFailGestureStateChangedHandler(object sender, GestureStateChangeEventArgs e)
        {
            if ((sender as Gesture) != requireGestureToFail) return;
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
                case GestureState.Cancelled:
                    if (state != GestureState.Failed) setState(GestureState.Failed);
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for Gesture events
    /// </summary>
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
