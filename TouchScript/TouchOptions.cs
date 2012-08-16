/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript {
    /// <summary>
    /// Component to configure and update TouchManager's options.
    /// </summary>
    internal class TouchOptions : MonoBehaviour {
        #region Unity fields

        /// <summary>
        /// Active cameras to look for touch targets in specific order.
        /// </summary>
        public Camera[] HitCameras;

        /// <summary>
        /// Current touch device DPI.
        /// </summary>
        public float DPI = 72;

        /// <summary>
        /// Current DPI to test in editor.
        /// </summary>
        public float EditorDPI = 72;

        /// <summary>
        /// Radius of single touch point on device in cm.
        /// </summary>
        public float TouchRadius = .75f;

        #endregion

        #region Private variables

        private TouchManager manager;

        #endregion

        #region Unity

        private void Awake() {
            if (TouchManager.Instance == null) {
                gameObject.AddComponent<TouchManager>();
            }
            manager = TouchManager.Instance;
            updateManager();
        }

        private void Update() {
            if (HitCameras.Length == 0 && Camera.mainCamera != null) HitCameras = new Camera[] {Camera.mainCamera};
            updateManager();
        }

        #endregion

        #region Private functions

        private void updateManager() {
            if (Application.isEditor) {
                manager.DPI = EditorDPI;
            } else {
                manager.DPI = DPI;
            }
            manager.TouchRadius = TouchRadius;
            manager.HitCameras = new List<Camera>(HitCameras);
        }

        #endregion
    }
}