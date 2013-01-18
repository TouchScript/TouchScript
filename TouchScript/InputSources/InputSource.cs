/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using UnityEngine;

namespace TouchScript.InputSources {
    /// <summary>
    /// Base class for all touch input sources
    /// </summary>
    public abstract class InputSource : MonoBehaviour {
        #region Unity fields

        /// <summary>
        /// Optional remapper to use to change screen coordinates which go into the TouchManager.
        /// </summary>
        public ICoordinatesRemapper CoordinatesRemapper;

        #endregion

        #region Private variables

        protected TouchManager Manager;

        #endregion

        #region Unity

        protected virtual void Start() {
            Manager = TouchManager.Instance;
            if (Manager == null) throw new InvalidOperationException("TouchManager instance is required!");
        }

        protected virtual void OnDestroy() {}

        protected virtual void Update() {}

        #endregion

        #region Callbacks

        protected int beginTouch(Vector2 position) {
            if (CoordinatesRemapper != null) {
                position = CoordinatesRemapper.Remap(position);
            }
            return Manager.BeginTouch(position);
        }

        protected void endTouch(int id) {
            Manager.EndTouch(id);
        }

        protected void moveTouch(int id, Vector2 position) {
            if (CoordinatesRemapper != null) {
                position = CoordinatesRemapper.Remap(position);
            }
            Manager.MoveTouch(id, position);
        }

        protected void cancelTouch(int id) {
            Manager.CancelTouch(id);
        }

        #endregion
    }
}