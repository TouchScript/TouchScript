/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using HutongGames.PlayMaker;
using TouchScript.Gestures;

namespace TouchScript.Modules.Playmaker.Actions
{
    [ActionCategory("TouchScript")]
    [Tooltip("Sends an event when an object is being pressed for some time.")]
    public class LongPressed : GestureWrapper<LongPressGesture>
    {
        public FsmEvent SendEvent;

        public override void Reset()
        {
            SendEvent = null;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            gesture.LongPressed += gesturePressedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.LongPressed -= gesturePressedHandler;
        }

        private void gesturePressedHandler(object sender, EventArgs e)
        {
            if (SendEvent == null) return;
            Fsm.Event(SendEvent);
        }

    }
}