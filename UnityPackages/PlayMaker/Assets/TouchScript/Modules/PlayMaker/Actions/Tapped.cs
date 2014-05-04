/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using HutongGames.PlayMaker;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Modules.Playmaker.Actions
{
    [ActionCategory("TouchScript")]
    [HutongGames.PlayMaker.Tooltip("Sends an event when an object is tapped.")]
    public class Tapped : GestureWrapper<TapGesture>
    {
        public FsmEvent SendEvent;

        public override void Reset()
        {
            SendEvent = null;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            gesture.Tapped += gestureTappedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.Tapped -= gestureTappedHandler;
        }

        private void gestureTappedHandler(object sender, EventArgs e)
        {
            if (SendEvent == null) return;
            Fsm.Event(SendEvent);
        }

    }
}