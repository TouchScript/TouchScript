/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using HutongGames.PlayMaker;
using TouchScript.Gestures;

namespace TouchScript.Modules.Playmaker.Actions
{
    [ActionCategory("TouchScript")]
    [Tooltip("Sends an event when an object is released.")]
    public class Released : GestureWrapper<ReleaseGesture>
    {
        public FsmEvent SendEvent;

        public override void Reset()
        {
            SendEvent = null;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            gesture.Released += gesturePressedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.Released -= gesturePressedHandler;
        }

        private void gesturePressedHandler(object sender, EventArgs e)
        {
            if (SendEvent == null) return;
            Fsm.Event(SendEvent);
        }

    }
}