/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;

namespace TouchScript
{
    /// <summary>
    /// <para>A delegate which can be set to <see cref="Gesture.Delegate"/> and control what this gesture can or can not do.</para>
    /// <para>This is a way to control very precisely how affected gestures work without inheriting from them and overriding their behavior.</para>
    /// <seealso cref="Gesture"/>
    /// </summary>
    public interface IGestureDelegate
    {
        /// <summary>
        /// Returns whether a gesture should receive a touch.
        /// </summary>
        /// <param name="gesture"> The gesture. </param>
        /// <param name="touch"> The touch. </param>
        /// <returns> <c>true</c> if it should; <c>false</c> otherwise. </returns>
        /// <remarks> Can be used to restrict what touches a gesture can receive and ignore the ones it shouldn't. </remarks>
        bool ShouldReceiveTouch(Gesture gesture, TouchPoint touch);

        /// <summary>
        /// Returns whether a gesture can now begin.
        /// </summary>
        /// <param name="gesture"> The gesture. </param>
        /// <returns> <c>true</c> if it can; <c>false</c> otherwise. </returns>
        /// <remarks> Can be used to stop a ready to begin gesture. </remarks>
        bool ShouldBegin(Gesture gesture);

        /// <summary>
        /// Returns whether two gestures can be recognized simultaneously or not.
        /// </summary>
        /// <param name="first"> The first gesture. </param>
        /// <param name="second"> The second gesture. </param>
        /// <returns> <c>true</c> if they should work together; <c>false</c> otherwise. </returns>
        /// <remarks> Can be used to restrict simultaneous gesture recognition. </remarks>
        bool ShouldRecognizeSimultaneously(Gesture first, Gesture second);
    }
}