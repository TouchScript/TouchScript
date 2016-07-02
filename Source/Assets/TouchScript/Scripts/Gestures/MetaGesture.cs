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
    /// Converts Pointer events for target object into separate events to be used somewhere else.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Meta Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_MetaGesture.htm")]
    public sealed class MetaGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message dispatched when a pointer begins.
        /// </summary>
        public const string POINTER_BEGAN_MESSAGE = "OnPointerBegan";

        /// <summary>
        /// Message dispatched when a pointer moves.
        /// </summary>
        public const string POINTER_MOVED_MESSAGE = "OnPointerMoved";

        /// <summary>
        /// Message dispatched when a pointer ends.
        /// </summary>
        public const string POINTER_ENDED_MESSAGE = "OnPointerEnded";

        /// <summary>
        /// Message dispatched when a pointer is cancelled.
        /// </summary>
        public const string POINTER_CANCELLED_MESSAGE = "OnPointerCancelled";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a pointer is added.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerBegan
        {
            add { pointerBeganInvoker += value; }
            remove { pointerBeganInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a pointer is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerMoved
        {
            add { pointerMovedInvoker += value; }
            remove { pointerMovedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a pointer is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerEnded
        {
            add { pointerEndedInvoker += value; }
            remove { pointerEndedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a pointer is cancelled.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerCancelled
        {
            add { pointerCancelledInvoker += value; }
            remove { pointerCancelledInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<MetaGestureEventArgs> pointerBeganInvoker,
                                                   pointerMovedInvoker,
                                                   pointerEndedInvoker,
                                                   pointerCancelledInvoker;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void pointersBegan(IList<Pointer> pointers)
        {
            base.pointersBegan(pointers);

            if (State == GestureState.Possible) setState(GestureState.Began);

            var length = pointers.Count;
            if (pointerBeganInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerBeganInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_BEGAN_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void pointersMoved(IList<Pointer> pointers)
        {
            base.pointersMoved(pointers);

            if (State == GestureState.Began || State == GestureState.Changed) setState(GestureState.Changed);

            var length = pointers.Count;
            if (pointerMovedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerMovedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_MOVED_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void pointersEnded(IList<Pointer> pointers)
        {
            base.pointersEnded(pointers);

            if ((State == GestureState.Began || State == GestureState.Changed) && NumPointers == 0) setState(GestureState.Ended);

            var length = pointers.Count;
            if (pointerEndedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerEndedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_ENDED_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <inheritdoc />
        protected override void pointersCancelled(IList<Pointer> pointers)
        {
            base.pointersCancelled(pointers);

            var length = pointers.Count;
            if (pointerCancelledInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerCancelledInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_CANCELLED_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
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
        /// Current pointer.
        /// </summary>
        public Pointer Pointer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaGestureEventArgs"/> class.
        /// </summary>
        /// <param name="pointer"> Pointer the event is for. </param>
        public MetaGestureEventArgs(Pointer pointer)
        {
            Pointer = pointer;
        }
    }
}