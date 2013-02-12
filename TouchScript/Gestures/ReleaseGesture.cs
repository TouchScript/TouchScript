/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes when last touch is released from target.
    /// Works with any gesture unless a Delegate is set.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Release Gesture")]
    public class ReleaseGesture : Gesture
    {
        public override bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        public override bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (activeTouches.Count == 0) setState(GestureState.Recognized);
        }
    }
}