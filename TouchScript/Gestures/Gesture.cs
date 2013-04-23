/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Events;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Base class for all gestures
    /// </summary>
    public abstract class Gesture : MonoBehaviour
    {
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

        #region Unity fields

        /// <summary>
        /// An array of gestures which are added to <see cref="ShouldRecognizeSimultaneouslyWith"/> DURING STARTUP so these other gestures could work with this one. 
        /// </summary>
        public Gesture[] WillRecognizeWith;

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

        private GestureState state = GestureState.Possible;

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
            get { return Clusters.Clusters.Get2DCenterPosition(activeTouches); }
        }

        /// <summary>
        /// Previous transformation center in screen coordinates.
        /// </summary>
        public virtual Vector2 PreviousScreenPosition
        {
            get { return Clusters.Clusters.GetPrevious2DCenterPosition(activeTouches); }
        }

        /// <summary>
        /// Transformation center in normalized screen coordinates.
        /// </summary>
        public Vector2 NormalizedScreenPosition
        {
            get
            {
                var position = ScreenPosition;
                return new Vector2(position.x/Screen.width, position.y/Screen.height);
            }
        }

        /// <summary>
        /// Previous center in screen coordinates.
        /// </summary>
        public Vector2 PreviousNormalizedScreenTransformCenter
        {
            get
            {
                var position = PreviousScreenPosition;
                return new Vector2(position.x/Screen.width, position.y/Screen.height);
            }
        }

        /// <summary>
        /// Touch points the gesture currently owns and works with.
        /// </summary>
        protected List<TouchPoint> activeTouches = new List<TouchPoint>();

        /// <summary>
        /// List of gesture's active touches.
        /// </summary>
        public List<TouchPoint> ActiveTouches
        {
            get { return new List<TouchPoint>(activeTouches); }
        }

        #endregion

        /// <summary>
        /// An object implementing <see cref="IGestureDelegate"/> to be asked for gesture specific actions.
        /// </summary>
        public IGestureDelegate Delegate { get; set; }

        #region Private variables

        /// <summary>
        /// Reference to global TouchManager.
        /// </summary>
        protected TouchManager manager { get; private set; }

        private List<int> shouldRecognizeWith = new List<int>();

        #endregion

        #region Unity

        /// <summary>
        /// Unity3d Awake handler.
        /// </summary>
        protected virtual void Awake()
        {
            if (WillRecognizeWith != null)
            {
                foreach (var gesture in WillRecognizeWith)
                {
                    if (gesture != null) ShouldRecognizeSimultaneouslyWith(gesture);
                }
            }
        }

        /// <summary>
        /// Unity3d Start handler.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">TouchManager instance is required!</exception>
        protected virtual void Start()
        {
            manager = TouchManager.Instance;
            if (manager == null) throw new InvalidOperationException("TouchManager instance is required!");
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
            manager = null;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether gesture controls a touch point.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <returns>
        ///   <c>true</c> if gesture controls the touch point; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTouchPoint(TouchPoint touch)
        {
            return activeTouches.Contains(touch);
        }

        /// <summary>
        /// Tells that the gesture should (or shouldn't) work simultaneously with this gesture.
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <param name="value">if set to <c>true</c> allows simultaneous recognition.</param>
        public void ShouldRecognizeSimultaneouslyWith(Gesture gesture, bool value = true)
        {
            if (gesture == this) return;

            var id = gesture.GetInstanceID();
            if (value && !shouldRecognizeWith.Contains(id))
            {
                shouldRecognizeWith.Add(id);
            } else
            {
                shouldRecognizeWith.Remove(id);
            }
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
                if (gesture.CanBePreventedByGesture(this))
                {
                    if (shouldRecognizeWith.Contains(gesture.GetInstanceID()))
                    {
                        return false;
                    } else
                    {
                        return true;
                    }
                } else
                {
                    return false;
                }
            } else
            {
                return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
            }
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
            if (Delegate == null)
            {
                if (shouldRecognizeWith.Contains(gesture.GetInstanceID()))
                {
                    return false;
                } else
                {
                    return true;
                }
            } else
            {
                return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
            }
        }

        /// <summary>
        /// Specifies if gesture can receive this specific touch point.
        /// </summary>
        /// <param name="touch">The touch.</param>
        /// <returns><c>true</c> if this touch should be received by the gesture; otherwise, <c>false</c>.</returns>
        public virtual bool ShouldReceiveTouch(TouchPoint touch)
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

        #region Internal functions

        internal void SetState(GestureState value)
        {
            setState(value);
        }

        internal void Reset()
        {
            activeTouches.Clear();
            reset();
        }

        internal void TouchesBegan(IList<TouchPoint> touches)
        {
            activeTouches.AddRange(touches);
            touchesBegan(touches);
        }

        internal void TouchesMoved(IList<TouchPoint> touches)
        {
            touchesMoved(touches);
        }

        internal void TouchesEnded(IList<TouchPoint> touches)
        {
            activeTouches.RemoveAll(touches.Contains);
            touchesEnded(touches);
        }

        internal void TouchesCancelled(IList<TouchPoint> touches)
        {
            activeTouches.RemoveAll(touches.Contains);
            touchesCancelled(touches);
        }

        #endregion

        #region Misc methods

        /// <summary>
        /// Tries to change gesture state.
        /// </summary>
        /// <param name="value">New state.</param>
        /// <returns><c>true</c> if state was changed; otherwise, <c>false</c>.</returns>
        protected bool setState(GestureState value)
        {
            if (manager == null) return false;
            if (value == state && state != GestureState.Changed) return false;

            var newState = manager.GestureChangeState(this, value);
            State = newState;

            return value == newState;
        }

        /// <summary>
        /// Manually ignore touch.
        /// </summary>
        /// <param name="touch">Touch to ignore.</param>
        protected void ignoreTouch(TouchPoint touch)
        {
            activeTouches.Remove(touch);
            manager.IgnoreTouch(touch);
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when new touches appear.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesBegan(IList<TouchPoint> touches)
        {}

        /// <summary>
        /// Called for moved touches.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesMoved(IList<TouchPoint> touches)
        {}

        /// <summary>
        /// Called if touches are removed.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesEnded(IList<TouchPoint> touches)
        {}

        /// <summary>
        /// Called when touches are cancelled.
        /// </summary>
        /// <param name="touches">The touches.</param>
        protected virtual void touchesCancelled(IList<TouchPoint> touches)
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
    }
}