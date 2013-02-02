/**
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
        public Gesture.GestureState PreviousState;
        public Gesture.GestureState State;

        public GestureStateChangeEventArgs(Gesture.GestureState state, Gesture.GestureState previousState)
        {
            State = state;
            PreviousState = previousState;
        }
    }
}