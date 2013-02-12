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
        public event EventHandler<MetaGestureEventArgs> TouchPointAdded;

        /// <summary>
        /// Occurs when a touch point is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointUpdated;

        /// <summary>
        /// Occurs when a touch point is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointRemoved;

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

            if (TouchPointAdded == null) return;
            foreach (var touchPoint in touches)
            {
                TouchPointAdded(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            if (State == GestureState.Began || State == GestureState.Changed)
            {
                setState(GestureState.Changed);
            }

            if (TouchPointUpdated == null) return;
            foreach (var touchPoint in touches)
            {
                TouchPointUpdated(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            if (TouchPointRemoved != null)
            {
                foreach (var touchPoint in touches)
                {
                    TouchPointRemoved(this, new MetaGestureEventArgs(touchPoint));
                }
            }

            if (State == GestureState.Began || State == GestureState.Changed)
            {
                if (activeTouches.Count == 0)
                {
                    setState(GestureState.Ended);
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            if (TouchPointCancelled != null)
            {
                foreach (var touchPoint in touches)
                {
                    TouchPointCancelled(this, new MetaGestureEventArgs(touchPoint));
                }
            }

            if (State == GestureState.Began || State == GestureState.Changed)
            {
                if (activeTouches.Count == 0)
                {
                    setState(GestureState.Ended);
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