/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;

namespace TouchScript
{
    /// <summary>
    /// Interface to implement to be able to customize gestures' behavior.
    /// </summary>
    public interface IGestureDelegate
    {
        /// <summary>
        /// Should the gesture receive touch or not
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <param name="touch">The touch.</param>
        /// <returns><c>true</c> if it should; <c>false</c> otherwise.</returns>
        bool ShouldReceiveTouch(Gesture gesture, ITouch touch);

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