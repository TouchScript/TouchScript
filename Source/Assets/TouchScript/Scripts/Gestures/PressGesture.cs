/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes when an object is touched. Works with any gesture unless a Delegate is set.
    /// </summary>
    /// <remarks>
    /// <para>PressGesture fires immediately and would ultimately kill every other non-friendly gesture. So one would have to manually make it friendly with everything in a general use-case. That's why it's made friendly with everyone by default.</para>
    /// <para>But there are cases when one would like to determine if parent container was pressed or its child. In current implementation both PressGestures will fire.</para>
    /// <para>One approach would be to somehow make parent's PressGesture not friendly with child's one. But looking at how gesture recognition works we can see that this won't work. Since we would like child's gesture to fail parent's gesture. When child's PressGesture is recognized the system asks it if it can prevent parent's gesture, and it obviously can't because it's friendly with everything. And it doesn't matter that parent's gesture can be prevented by child's one... because child's one can't prevent parent's gesture and this is asked first.</para>
    /// <para>This is basically what <see cref="IgnoreChildren"/> is for. It makes parent's PressGesture only listen for TouchPoints which lend directly on it.</para>
    /// </remarks>
    [AddComponentMenu("TouchScript/Gestures/Press Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_PressGesture.htm")]
    public class PressGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string PRESS_MESSAGE = "OnPress";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> Pressed
        {
            add { pressedInvoker += value; }
            remove { pressedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> pressedInvoker;

        /// <summary>
        /// Unity event, occurs when gesture is recognized.
        /// </summary>
		public GestureEvent OnPress = new GestureEvent();

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether actions coming from children should be ingored.
        /// </summary>
        /// <value> <c>true</c> if actions from children should be ignored; otherwise, <c>false</c>. </value>
        public bool IgnoreChildren
        {
            get { return ignoreChildren; }
            set { ignoreChildren = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        [ToggleLeft]
        private bool ignoreChildren = false;

		private CustomSampler gestureSampler;

		#endregion

		#region Unity

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();

			gestureSampler = CustomSampler.Create("[TouchScript] Press Gesture");
		}

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
			basicEditor = true; 
		}

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        public override bool ShouldReceivePointer(Pointer pointer)
        {
            if (!IgnoreChildren) return base.ShouldReceivePointer(pointer);
            if (!base.ShouldReceivePointer(pointer)) return false;

            if (pointer.GetPressData().Target != cachedTransform) return false;
            return true;
        }

        /// <inheritdoc />
        public override bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        public override bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        protected override void pointersPressed(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersPressed(pointers);

            if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                setState(GestureState.Recognized);
				gestureSampler.End();
                return;
            }
            if (pointersNumState == PointersNumState.PassedMinMaxThreshold)
            {
                setState(GestureState.Failed);
				gestureSampler.End();
                return;
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (pressedInvoker != null) pressedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(PRESS_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
			if (UseUnityEvents) OnPress.Invoke(this);
        }

        #endregion
    }
}