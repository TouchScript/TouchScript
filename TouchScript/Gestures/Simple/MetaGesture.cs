/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures.Simple
{
    /// <summary>
    /// Converts touchpoint events for target object into separate events to be used somewhere else.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Meta Gesture")]
    public class MetaGesture : Gesture
    {
        #region Events

        /// <summary>
        /// Occurs when a touch point is added.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointBegan
        {
            add { touchPointBeganInvoker += value; }
            remove { touchPointBeganInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointMoved
        {
            add { touchPointMovedInvoker += value; }
            remove { touchPointMovedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointEnded
        {
            add { touchPointEndedInvoker += value; }
            remove { touchPointEndedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is cancelled.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointCancelled
        {
            add { touchPointCancelledInvoker += value; }
            remove { touchPointCancelledInvoker -= value; }
        }

        // iOS Events AOT hack
        private EventHandler<MetaGestureEventArgs> touchPointBeganInvoker, touchPointMovedInvoker,
                                                   touchPointEndedInvoker, touchPointCancelledInvoker;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (State == GestureState.Possible) setState(GestureState.Began);

            if (touchPointBeganInvoker == null) return;
            foreach (var touchPoint in touches) touchPointBeganInvoker(this, new MetaGestureEventArgs(touchPoint));
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (State == GestureState.Began || State == GestureState.Changed) setState(GestureState.Changed);

            if (touchPointMovedInvoker == null) return;
            foreach (var touchPoint in touches) touchPointMovedInvoker(this, new MetaGestureEventArgs(touchPoint));
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if ((State == GestureState.Began || State == GestureState.Changed) && activeTouches.Count == 0) setState(GestureState.Ended);

            if (touchPointEndedInvoker == null) return;
            foreach (var touchPoint in touches) touchPointEndedInvoker(this, new MetaGestureEventArgs(touchPoint));
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            base.touchesCancelled(touches);

            if ((State == GestureState.Began || State == GestureState.Changed) && activeTouches.Count == 0) setState(GestureState.Ended);

            if (touchPointCancelledInvoker == null) return;
            foreach (var touchPoint in touches) touchPointCancelledInvoker(this, new MetaGestureEventArgs(touchPoint));
        }

        #endregion
    }

    /// <summary>
    /// EventArgs for MetaGesture events.
    /// </summary>
    public class MetaGestureEventArgs : EventArgs
    {
        /// <summary>
        /// Current touch point.
        /// </summary>
        public TouchPoint TouchPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaGestureEventArgs"/> class.
        /// </summary>
        /// <param name="touchPoint">Touch point the event is for.</param>
        public MetaGestureEventArgs(TouchPoint touchPoint)
        {
            TouchPoint = touchPoint;
        }
    }
}