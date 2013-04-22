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
        public event EventHandler<MetaGestureEventArgs> TouchPointBegan
        {
            add { touchPointBeganInvoker += value; }
            remove { touchPointBeganInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is updated.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointMoved
        {
            add { touchPointMovedInvoker += value; }
            remove { touchPointMovedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is removed.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointEnded
        {
            add { touchPointEndedInvoker += value; }
            remove { touchPointEndedInvoker -= value; }
        }

        /// <summary>
        /// Occurs when a touch point is cancelled.
        /// </summary>
        public event EventHandler<MetaGestureEventArgs> TouchPointCancelled
        {
            add { touchPointCancelledInvoker += value; }
            remove { touchPointCancelledInvoker -= value; }
        }

        // iOS Events AOT Bug hack
        private EventHandler<MetaGestureEventArgs> touchPointBeganInvoker, touchPointMovedInvoker,
                                                   touchPointEndedInvoker, touchPointCancelledInvoker;

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (State == GestureState.Possible)
            {
                setState(GestureState.Began);
            }

            if (touchPointBeganInvoker == null) return;
            foreach (var touchPoint in touches)
            {
                touchPointBeganInvoker(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            if (State == GestureState.Began || State == GestureState.Changed)
            {
                setState(GestureState.Changed);
            }

            if (touchPointMovedInvoker == null) return;
            foreach (var touchPoint in touches)
            {
                touchPointMovedInvoker(this, new MetaGestureEventArgs(touchPoint));
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

            if (touchPointEndedInvoker == null) return;
            foreach (var touchPoint in touches)
            {
                touchPointEndedInvoker(this, new MetaGestureEventArgs(touchPoint));
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

            if (touchPointCancelledInvoker == null) return;
            foreach (var touchPoint in touches)
            {
                touchPointCancelledInvoker(this, new MetaGestureEventArgs(touchPoint));
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