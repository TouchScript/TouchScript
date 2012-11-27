/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using TUIOSharp;
using UnityEngine;

namespace TouchScript.InputSources {
    /// <summary>
    /// Processes TUIO 1.0 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO Input")]
    public class TuioInput : InputSource {
        #region Unity fields

        /// <summary>
        /// Port to listen to.
        /// </summary>
        public int TuioPort = 3333;

        /// <summary>
        /// Use touch flicker prevention.
        /// Handles touches disappearing and reappearing again in short period of time.
        /// </summary>
        //public bool PreventTouchFlicker = true;

        /// <summary>
        /// Maximum distance in cm for a new touch to be considered as disappeared old touch.
        /// </summary>
        //public float TouchFlickerDistance = 0.5f;

        /// <summary>
        /// Maximum time in seconds while touch is considered to be still alive.
        /// </summary>
        //public float TouchFlickerDelay = 0.03f;

        #endregion

        #region Private variables

        private TUIOServer server;

        private readonly object sync = new object();
        private readonly Dictionary<TUIOCursor, int> cursorToInternalId = new Dictionary<TUIOCursor, int>();

        #endregion

        #region Unity

        protected override void Start() {
            base.Start();

			server = new TUIOServer(TuioPort);
			server.CursorAdded += OnCursorAdded;
            server.CursorUpdated += OnCursorUpdated;
            server.CursorRemoved += OnCursorRemoved;
            server.Connect();

            updateServerProperties();
        }

        protected override void Update() {
            base.Update();

            //updateServerProperties();
        }

        protected override void OnDestroy() {
            if (server != null) {
                server.CursorAdded -= OnCursorAdded;
                server.CursorUpdated -= OnCursorUpdated;
                server.CursorRemoved -= OnCursorRemoved;
                server.Disconnect();
            }
            base.OnDestroy();
        }

        #endregion

        #region Private functions

        private void updateServerProperties() {
            server.PreventTouchFlicker = false;// PreventTouchFlicker;
            //server.TouchFlickerDistance = TouchFlickerDistance*TouchManager.Instance.DotsPerCentimeter/ScreenWidth;
            //server.TouchFlickerDelay = TouchFlickerDelay;
        }

        #endregion

        #region Event handlers

        private void OnCursorAdded(object sender, TUIOCursorEventArgs tuioCursorEventArgs) {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (sync) {
                var x = cursor.X*ScreenWidth;
                var y = (1 - cursor.Y)*ScreenHeight;
                cursorToInternalId.Add(cursor, beginTouch(new Vector2(x, y)));
            }
        }

        private void OnCursorUpdated(object sender, TUIOCursorEventArgs tuioCursorEventArgs) {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (sync) {
                int existingCursor;
                if (!cursorToInternalId.TryGetValue(cursor, out existingCursor)) return;

                var x = cursor.X*ScreenWidth;
                var y = (1 - cursor.Y)*ScreenHeight;

                moveTouch(existingCursor, new Vector2(x, y));
            }
        }

        private void OnCursorRemoved(object sender, TUIOCursorEventArgs tuioCursorEventArgs) {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (sync) {
                int existingCursor;
                if (!cursorToInternalId.TryGetValue(cursor, out existingCursor)) return;

                cursorToInternalId.Remove(cursor);
                endTouch(existingCursor);
            }
        }

        #endregion
    }
}