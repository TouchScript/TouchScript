/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Converts touch events for target object into separate events to be used somewhere else.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Meta Gesture")]
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Gestures_MetaGesture.htm")]
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
        protected override void touchBegan(TouchPoint touch)
        {
            base.touchBegan(touch);

            if (State == GestureState.Possible) setState(GestureState.Began);

            if (touchBeganInvoker != null) touchBeganInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touch));
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(TOUCH_BEGAN_MESSAGE, touch, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void touchMoved(TouchPoint touch)
        {
            base.touchMoved(touch);

            if (State == GestureState.Began || State == GestureState.Changed) setState(GestureState.Changed);

            if (touchMovedInvoker != null) touchMovedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touch));
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(TOUCH_MOVED_MESSAGE, touch, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void touchEnded(TouchPoint touch)
        {
            base.touchEnded(touch);

            if ((State == GestureState.Began || State == GestureState.Changed) && NumTouches == 0) setState(GestureState.Ended);

            if (touchEndedInvoker != null) touchEndedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touch));
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(TOUCH_ENDED_MESSAGE, touch, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void touchCancelled(TouchPoint touch)
        {
            base.touchCancelled(touch);

            if (touchCancelledInvoker != null) touchCancelledInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(touch));
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(TOUCH_CANCELLED_MESSAGE, touch, SendMessageOptions.DontRequireReceiver);
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