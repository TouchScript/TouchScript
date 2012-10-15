/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;

namespace TouchScript.Gestures {
    /// <summary>
    /// Converts touchpoint events for target object into separate events to be used somewhere else.
    /// </summary>
    public class MetaGesture : Gesture {
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

        protected override void touchesBegan(IList<TouchPoint> touches) {
            if (State == GestureState.Possible) {
                setState(GestureState.Began);
            }

            if (TouchPointAdded == null) return;
            foreach (var touchPoint in touches) {
                TouchPointAdded(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches) {
            if (State == GestureState.Began || State == GestureState.Changed) {
                setState(GestureState.Changed);
            }

            if (TouchPointUpdated == null) return;
            foreach (var touchPoint in touches) {
                TouchPointUpdated(this, new MetaGestureEventArgs(touchPoint));
            }
        }

        protected override void touchesEnded(IList<TouchPoint> touches) {
            if (TouchPointRemoved != null) {
                foreach (var touchPoint in touches) {
                    TouchPointRemoved(this, new MetaGestureEventArgs(touchPoint));
                }
            }

            if (State == GestureState.Began || State == GestureState.Changed) {
                if (activeTouches.Count == 0) {
                    setState(GestureState.Ended);
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches) {
            if (TouchPointCancelled != null) {
                foreach (var touchPoint in touches) {
                    TouchPointCancelled(this, new MetaGestureEventArgs(touchPoint));
                }
            }

            if (State == GestureState.Began || State == GestureState.Changed) {
                if (activeTouches.Count == 0) {
                    setState(GestureState.Ended);
                }
            }
        }

        #endregion
    }

    public class MetaGestureEventArgs : EventArgs {
        public TouchPoint TouchPoint;

        public MetaGestureEventArgs(TouchPoint touchPoint) {
            TouchPoint = touchPoint;
        }
    }
}