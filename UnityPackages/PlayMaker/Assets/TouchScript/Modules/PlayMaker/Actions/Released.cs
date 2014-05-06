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
    [HutongGames.PlayMaker.Tooltip("Sends an event when an object is released.")]
    public class Released : FsmStateAction
    {

        [HutongGames.PlayMaker.Tooltip("The GameObject that owns the Gesture.")]
        public FsmOwnerDefault GameObject;

        [HutongGames.PlayMaker.Tooltip("Optionally drag a component directly into this field (gesture name will be ignored).")]
        public Component Component;
        
        public FsmEvent SendEvent;

        protected ReleaseGesture gesture;

        public override void Reset()
        {
            GameObject = null;
            Component = null;
            SendEvent = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<ReleaseGesture>(Fsm, GameObject, Component, true);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

            gesture.Released += gestureReleasedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.Released -= gestureReleasedHandler;
        }

        private void gestureReleasedHandler(object sender, EventArgs e)
        {
            if (SendEvent == null) return;
            Fsm.Event(SendEvent);
        }

    }
}