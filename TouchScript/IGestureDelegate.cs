/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using TouchScript.Gestures;

namespace TouchScript {
    /// <summary>
    /// Interface to implement to be able to customize gestures behavior.
    /// </summary>
    public interface IGestureDelegate {
        /// <summary>
        /// Should the gesture receive touch or not
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <param name="touch">The touch.</param>
        /// <returns><c>true</c> if it should; <c>false</c> otherwise.</returns>
        bool ShouldReceiveTouch(Gesture gesture, TouchPoint touch);

        /// <summary>
        /// Shoulds the gesture begin or not.
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <returns><c>true</c> if it should; <c>false</c> otherwise.</returns>
        bool ShouldBegin(Gesture gesture);

        /// <summary>
        /// Shoulds two gestures be recognized simultaneously or not.
        /// </summary>
        /// <param name="first">First gesture</param>
        /// <param name="second">Second gesture</param>
        /// <returns><c>true</c> if they should; <c>false</c> otherwise.</returns>
        bool ShouldRecognizeSimultaneously(Gesture first, Gesture second);
    }
}