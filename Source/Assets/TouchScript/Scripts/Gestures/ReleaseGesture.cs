/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes when last touch is released from target.
    /// Works with any gesture unless a Delegate is set.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Release Gesture")]
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Gestures_ReleaseGesture.htm")]
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

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        public override bool ShouldReceiveTouch(TouchPoint touch)
        {
            if (!IgnoreChildren) return base.ShouldReceiveTouch(touch);
            if (!base.ShouldReceiveTouch(touch)) return false;

            if (touch.Target != cachedTransform) return false;
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
        protected override void touchEnded(TouchPoint touch)
        {
            base.touchEnded(touch);

            if (touchesNumState == TouchesNumState.PassedMinThreshold) setState(GestureState.Recognized);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (releasedInvoker != null) releasedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(RELEASE_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        #endregion
    }
}