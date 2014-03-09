/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils.Editor.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes when an object is touched.
    /// Works with any gesture unless a Delegate is set.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Press Gesture")]
    public class PressGesture : Gesture
    {

        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether actions coming from children should be ingored.
        /// </summary>
        /// <value><c>true</c> if actions from children should be ignored; otherwise, <c>false</c>.</value>
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
        public override bool ShouldReceiveTouch(ITouch touch)
        {
            if (!IgnoreChildren) return base.ShouldReceiveTouch(touch);
            if (!base.ShouldReceiveTouch(touch)) return false;
            
            if (touch.Target != transform) return false;
            return true;
        }

        /// <inheritdoc />
        public override bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        public override bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count == touches.Count) setState(GestureState.Recognized);
        }

        #endregion

    }
}