/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Timers;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures {
    /// <summary>
    /// Gesture which recognizes a point cluster which didn't move for specified time since it appeared.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Long Press Gesture")]
    public class LongPressGesture : Gesture {
        #region Unity fields

        /// <summary>
        /// Maximum number of simultaneous touch points.
        /// </summary>
        public int MaxTouches = int.MaxValue;

        /// <summary>
        /// Total time in seconds required to hold touches still.
        /// </summary>
        public float TimeToPress = 1;

        /// <summary>
        /// Maximum distance in cm cluster can move before gesture fails.
        /// </summary>
        public float DistanceLimit = float.PositiveInfinity;

        #endregion

        #region Private fields

        private Cluster1 cluster = new Cluster1();
        private Vector2 totalMovement;
        private Timer timer = new Timer();
        private bool fireRecognizedNextUpdate = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Screen coordinates of the cluster when gesture is recognized.
        /// </summary>
        public Vector2 ScreenPressCoordinates { get; protected set; }

        #endregion

        #region Unity

        protected override void Awake() {
            base.Awake();
            timer.Elapsed += onTimerElapsed;
            timer.AutoReset = false;
        }

        protected void Update() {
            if (fireRecognizedNextUpdate) {
                ScreenPressCoordinates = cluster.GetCenterPosition();
                setState(GestureState.Recognized);
            }
        }

        #endregion

        #region Gesture callbacks

        protected override void touchesBegan(IList<TouchPoint> touches) {
            if (activeTouches.Count > MaxTouches) {
                setState(GestureState.Failed);
                return;
            }
            foreach (var touch in touches) {
                if (cluster.AddPoint(touch) == Cluster.OperationResult.FirstPointAdded) {
                    timer.Interval = TimeToPress*1000;
                    timer.Start();
                }
            }
        }

        protected override void touchesMoved(IList<TouchPoint> touches) {
            cluster.Invalidate();
            totalMovement += cluster.GetCenterPosition() - cluster.GetPreviousCenterPosition();
            if (totalMovement.magnitude/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit) setState(GestureState.Failed);
        }

        protected override void touchesEnded(IList<TouchPoint> touches) {
            foreach (var touch in touches) {
                var result = cluster.RemovePoint(touch);
                if (result == Cluster.OperationResult.LastPointRemoved) {
                    setState(GestureState.Failed);
                }
            }
        }

        protected override void reset() {
            fireRecognizedNextUpdate = false;
            cluster.RemoveAllPoints();
            timer.Stop();
        }

        #endregion

        #region Event handlers

        private void onTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            fireRecognizedNextUpdate = true;
        }

        #endregion
    }
}