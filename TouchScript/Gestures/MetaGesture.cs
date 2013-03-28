/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Converts touchpoint events for target object into separate events to be used somewhere else.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Meta Gesture")]
    public class MetaGesture : Gesture
    {
        #region Events

        /// <summary>
        /// Occurs when a touch point is added.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointBegan;

        /// <summary>
        /// Occurs when a touch point is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointMoved;

        /// <summary>
        /// Occurs when a touch point is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointEnded;

        /// <summary>
        /// Occurs when a touch point is cancelled.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointCancelled;

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (State == GestureState.Possible)
            {
                setState(GestureState.Began);
            }

            if (TouchPointBegan == null) return;
            foreach (var touchPoint in touches)
            {
                TouchPointBegan(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            if (State == GestureState.Began || State == GestureState.Changed)
            {
                setState(GestureState.Changed);
            }

            if (TouchPointMoved == null) return;
            foreach (var touchPoint in touches)
            {
                TouchPointMoved(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
			if (State == GestureState.Began || State == GestureState.Changed)
            {
                if (activeTouches.Count == 0)
                {
                    setState(GestureState.Ended);
                }
            }
			
            if (TouchPointEnded != null)
            {
                foreach (var touchPoint in touches)
                {
                    TouchPointEnded(this, new MetaGestureEventArgs(touchPoint));
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
			if (State == GestureState.Began || State == GestureState.Changed)
            {
                if (activeTouches.Count == 0)
                {
                    setState(GestureState.Ended);
                }
            }
			
            if (TouchPointCancelled != null)
            {
                foreach (var touchPoint in touches)
                {
                    TouchPointCancelled(this, new MetaGestureEventArgs(touchPoint));
                }
            }
        }

        #endregion
    }

    public class MetaGestureEventArgs : EventArgs
    {
        public TouchPoint TouchPoint;

        public MetaGestureEventArgs(TouchPoint touchPoint)
        {
            TouchPoint = touchPoint;
        }
    }
}