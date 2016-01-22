/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Converts touch events for target object into separate events to be used somewhere else.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Meta Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_MetaGesture.htm")]
    public sealed class MetaGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message dispatched when a touch begins.
        /// </summary>
        public const string TOUCH_BEGAN_MESSAGE = "OnTouchBegan";

        /// <summary>
        /// Message dispatched when a touch moves.
        /// </summary>
        public const string TOUCH_MOVED_MESSAGE = "OnTouchMoved";

        /// <summary>
        /// Message dispatched when a touch ends.
        /// </summary>
        public const string TOUCH_ENDED_MESSAGE = "OnTouchEnded";

        /// <summary>
        /// Message dispatched when a touch is cancelled.
        /// </summary>
        public const string TOUCH_CANCELLED_MESSAGE = "OnTouchCancelled";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a touch point is added.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchBegan
        {
            add { touchBeganInvoker += value; }
            remove { touchBeganInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchMoved
        {
            add { touchMovedInvoker += value; }
            remove { touchMovedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchEnded
        {
            add { touchEndedInvoker += value; }
            remove { touchEndedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is cancelled.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchCancelled
        {
            add { touchCancelledInvoker += value; }
            remove { touchCancelledInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<MetaGestureEventArgs> touchBeganInvoker,
                                                   touchMovedInvoker,
                                                   touchEndedInvoker,
                                                   touchCancelledInvoker;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (State == GestureState.Possible) setState(GestureState.Began);

            var length = touches.Count;
            if (touchBeganInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    touchBeganInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_BEGAN_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (State == GestureState.Began || State == GestureState.Changed) setState(GestureState.Changed);

            var length = touches.Count;
            if (touchMovedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    touchMovedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_MOVED_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            base.touchesEnded(touches);

            if ((State == GestureState.Began || State == GestureState.Changed) && NumTouches == 0) setState(GestureState.Ended);

            var length = touches.Count;
            if (touchEndedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    touchEndedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_ENDED_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            base.touchesCancelled(touches);

            var length = touches.Count;
            if (touchCancelledInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    touchCancelledInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touches[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(TOUCH_CANCELLED_MESSAGE, touches[i], SendMessageOptions.DontRequireReceiver);
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
        public TouchPoint Touch { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaGestureEventArgs"/> class.
        /// </summary>
        /// <param name="touch"> Touch point the event is for. </param>
        public MetaGestureEventArgs(TouchPoint touch)
        {
            Touch = touch;
        }
    }
}