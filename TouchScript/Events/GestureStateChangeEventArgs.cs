/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;

namespace TouchScript.Events
{
    /// <summary>
    /// Gesture state change event arguments.
    /// </summary>
    public class GestureStateChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Previous gesture state.
        /// </summary>
        public Gesture.GestureState PreviousState;
        /// <summary>
        /// Current gesture state.
        /// </summary>
        public Gesture.GestureState State;

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureStateChangeEventArgs"/> class.
        /// </summary>
        /// <param name="state">Current gesture state.</param>
        /// <param name="previousState">Previous gesture state.</param>
        public GestureStateChangeEventArgs(Gesture.GestureState state, Gesture.GestureState previousState)
        {
            State = state;
            PreviousState = previousState;
        }
    }
}