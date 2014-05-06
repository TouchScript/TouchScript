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
    [HutongGames.PlayMaker.Tooltip("Sends an event when an object is being pressed for some time.")]
    public class LongPressed : FsmStateAction
    {
        [HutongGames.PlayMaker.Tooltip("The GameObject that owns the Gesture.")]
        public FsmOwnerDefault GameObject;

        [HutongGames.PlayMaker.Tooltip("Optionally drag a component directly into this field (gesture name will be ignored).")]
        public Component Component;

        [UIHint(UIHint.Variable)]
        public FsmVector2 ScreenPosition;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent SendEvent;

        protected LongPressGesture gesture;

        public override void Reset()
        {
            GameObject = null;
            Component = null;
            ScreenPosition = null;
            SendEvent = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<LongPressGesture>(Fsm, GameObject, Component, true);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

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
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            Fsm.Event(SendEvent);
        }

    }
}