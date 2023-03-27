/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;

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
        public const string POINTER_PRESSED_MESSAGE = "OnPointerPressed";

        /// <summary>
        /// Message dispatched when a pointer moves.
        /// </summary>
        public const string POINTER_MOVED_MESSAGE = "OnPointerMoved";

        /// <summary>
        /// Message dispatched when a pointer ends.
        /// </summary>
        public const string POINTER_RELEASED_MESSAGE = "OnPointerReleased";

        /// <summary>
        /// Message dispatched when a pointer is cancelled.
        /// </summary>
        public const string POINTER_CANCELLED_MESSAGE = "OnPointerCancelled";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a pointer is added.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerPressed
        {
            add { pointerPressedInvoker += value; }
            remove { pointerPressedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a pointer is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerUpdated
        {
            add { pointerUpdatedInvoker += value; }
            remove { pointerUpdatedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a pointer is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> PointerReleased
        {
            add { pointerReleasedInvoker += value; }
            remove { pointerReleasedInvoker -= value; }
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
        private EventHandler<MetaGestureEventArgs> pointerPressedInvoker,
                                                   pointerUpdatedInvoker,
                                                   pointerReleasedInvoker,
                                                   pointerCancelledInvoker;

		#endregion

		#region Private variables

		private CustomSampler gestureSampler;

		#endregion

		#region Unity

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();

			gestureSampler = CustomSampler.Create("[TouchScript] Meta Gesture");
		}

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
			basicEditor = true;
		}

		#endregion

		#region Gesture callbacks

		/// <inheritdoc />
		protected override void pointersPressed(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersPressed(pointers);

            if (State == GestureState.Idle) setState(GestureState.Began);

            var length = pointers.Count;
            if (pointerPressedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerPressedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_PRESSED_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void pointersUpdated(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersUpdated(pointers);

            if (State == GestureState.Began || State == GestureState.Changed) setState(GestureState.Changed);

            var length = pointers.Count;
            if (pointerUpdatedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerUpdatedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_MOVED_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void pointersReleased(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersReleased(pointers);

            if ((State == GestureState.Began || State == GestureState.Changed) && NumPointers == 0) setState(GestureState.Ended);

            var length = pointers.Count;
            if (pointerReleasedInvoker != null)
            {
                for (var i = 0; i < length; i++)
                    pointerReleasedInvoker.InvokeHandleExceptions(this, new MetaGestureEventArgs(pointers[i]));
            }
            if (UseSendMessage && SendMessageTarget != null)
            {
                for (var i = 0; i < length; i++) SendMessageTarget.SendMessage(POINTER_RELEASED_MESSAGE, pointers[i], SendMessageOptions.DontRequireReceiver);
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void pointersCancelled(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

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

			gestureSampler.End();
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