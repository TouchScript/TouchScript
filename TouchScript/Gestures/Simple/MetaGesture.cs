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
    public sealed class MetaGesture : Gesture
    {
        #region Constants

        public const string TOUCH_POINT_BEGAN_MESSAGE = "OnTouchPointBegan";
        public const string TOUCH_POINT_MOVED_MESSAGE = "OnTouchPointMoved";
        public const string TOUCH_POINT_ENDED_MESSAGE = "OnTouchPointEnded";
        public const string TOUCH_POINT_CANCELLED_MESSAGE = "OnTouchPointCancelled";

        #endregion

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
        protected override void touchesBegan(IList<ITouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (State == GestureState.Possible) setState(GestureState.Began);

            var length = touches.Count;
            if (touchPointBeganInvoker != null)
            {
                for (var i = 0; i < length; i++) touchPointBeganInvoker(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_POINT_BEGAN_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (State == GestureState.Began || State == GestureState.Changed) setState(GestureState.Changed);

            var length = touches.Count;
            if (touchPointMovedInvoker != null)
            {
                for (var i = 0; i < length; i++) touchPointMovedInvoker(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_POINT_MOVED_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouchPoint> touches)
        {
            base.touchesEnded(touches);

            if ((State == GestureState.Began || State == GestureState.Changed) && touchPoints.Count == 0) setState(GestureState.Ended);

            var length = touches.Count;
            if (touchPointEndedInvoker != null)
            {
                for (var i = 0; i < length; i++) touchPointEndedInvoker(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_POINT_ENDED_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<ITouchPoint> touches)
        {
            base.touchesCancelled(touches);

            if ((State == GestureState.Began || State == GestureState.Changed) && touchPoints.Count == 0) setState(GestureState.Ended);

            var length = touches.Count;
            if (touchPointCancelledInvoker != null)
            {
                for (var i = 0; i < length; i++) touchPointCancelledInvoker(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_POINT_CANCELLED_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
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
        public ITouchPoint TouchPoint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaGestureEventArgs"/> class.
        /// </summary>
        /// <param name="touchPoint">Touch point the event is for.</param>
        public MetaGestureEventArgs(ITouchPoint touchPoint)
        {
            TouchPoint = touchPoint;
        }
    }
}