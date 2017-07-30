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
    /// Recognizes when last pointer is released from target. Works with any gesture unless a Delegate is set. 
    /// </summary>
    /// <seealso cref="PressGesture"/>
    [AddComponentMenu("TouchScript/Gestures/Release Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_ReleaseGesture.htm")]
    public class ReleaseGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string RELEASE_MESSAGE = "OnRelease";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> Released
        {
            add { releasedInvoker += value; }
            remove { releasedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> releasedInvoker;

        /// <summary>
        /// Unity event, occurs when gesture is recognized.
        /// </summary>
		public GestureEvent OnRelease = new GestureEvent();

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

			gestureSampler = CustomSampler.Create("[TouchScript] Release Gesture");
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
                if (State == GestureState.Idle) setState(GestureState.Possible);
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
        protected override void pointersReleased(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersReleased(pointers);

            if (pointersNumState == PointersNumState.PassedMinThreshold) setState(GestureState.Recognized);

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (releasedInvoker != null) releasedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(RELEASE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
			if (UseUnityEvents) OnRelease.Invoke(this);
        }

        #endregion
    }
}