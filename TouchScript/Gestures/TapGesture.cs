/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures {
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : Gesture {
        #region Unity fields

        /// <summary>
        /// Maximum time to hold touches until gesture is considered to be failed.
        /// </summary>
        public float TimeLimit = float.PositiveInfinity;

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture is considered to be failed.
        /// </summary>
        public float DistanceLimit = float.PositiveInfinity;

        #endregion

        #region Private variables

        private float totalMovement = 0f;
        private float startTime;

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches) {
            foreach (var touch in touches) {
                if (ActiveTouches.Count == touches.Count) {
                    startTime = Time.time;
                }
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches) {
            totalMovement += (Cluster.GetCenterPosition(touches) - Cluster.GetPreviousCenterPosition(touches)).magnitude;
        }

        protected override void touchesEnded(IList<TouchPoint> touches) {
            foreach (var touch in touches) {
                if (ActiveTouches.Count == 0) {
                    if (totalMovement/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit || Time.time - startTime > TimeLimit) {
                        setState(GestureState.Failed);
                        return;
                    }

                    var target = Manager.GetHitTarget(touch);
                    if (target == null || !(transform == target || target.IsChildOf(transform))) {
                        setState(GestureState.Failed);
                    } else {
                        setState(GestureState.Recognized);
                    }
                }
            }
        }

        protected override void touchesCancelled(IList<TouchPoint> touches) {
            setState(GestureState.Failed);
        }

        protected override void reset() {
            totalMovement = 0f;
        }

        #endregion
    }
}