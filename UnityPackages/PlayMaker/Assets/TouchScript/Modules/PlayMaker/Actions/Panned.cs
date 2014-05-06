/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using HutongGames.PlayMaker;
using TouchScript.Gestures.Simple;
using UnityEngine;

namespace TouchScript.Modules.Playmaker.Actions
{
    [ActionCategory("TouchScript")]
    [HutongGames.PlayMaker.Tooltip("Sends event when pan gesture is performed on an object.")]
    public class Panned : FsmStateAction
    {

        [HutongGames.PlayMaker.Tooltip("The GameObject that owns the Gesture.")]
        public FsmOwnerDefault GameObject;

        [HutongGames.PlayMaker.Tooltip("Optionally drag a component directly into this field (gesture name will be ignored).")]
        public Component Component;

        [UIHint(UIHint.Variable)]
        public FsmVector2 ScreenPosition;

        [UIHint(UIHint.Variable)]
        public FsmVector2 PreviousScreenPosition;

        [UIHint(UIHint.Variable)]
        public FsmVector3 WorldDeltaPosition;

        [UIHint(UIHint.Variable)]
        public FsmVector3 LocalDeltaPosition;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent PanStartedEvent;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent PannedEvent;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent PanCompletedEvent;

        protected SimplePanGesture gesture;

        public override void Reset()
        {
            GameObject = null;
            Component = null;
            ScreenPosition = null;
            PreviousScreenPosition = null;
            WorldDeltaPosition = null;
            LocalDeltaPosition = null;
            PanStartedEvent = null;
            PannedEvent = null;
            PanCompletedEvent = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<SimplePanGesture>(Fsm, GameObject, Component, true);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

            gesture.PanStarted += gesturePanStartedHandler;
            gesture.Panned += gesturePannedHandler;
            gesture.PanCompleted += gesturePanCompletedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.PanStarted -= gesturePanStartedHandler;
            gesture.Panned -= gesturePannedHandler;
            gesture.PanCompleted -= gesturePanCompletedHandler;
        }

        private void gesturePanStartedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (WorldDeltaPosition != null) WorldDeltaPosition.Value = gesture.WorldDeltaPosition;
            if (LocalDeltaPosition != null) LocalDeltaPosition.Value = gesture.LocalDeltaPosition;

            if (PanStartedEvent == null) return;
            Fsm.Event(PanStartedEvent);
        }

        private void gesturePannedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (WorldDeltaPosition != null) WorldDeltaPosition.Value = gesture.WorldDeltaPosition;
            if (LocalDeltaPosition != null) LocalDeltaPosition.Value = gesture.LocalDeltaPosition;

            if (PannedEvent == null) return;
            Fsm.Event(PannedEvent);
        }

        private void gesturePanCompletedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (WorldDeltaPosition != null) WorldDeltaPosition.Value = gesture.WorldDeltaPosition;
            if (LocalDeltaPosition != null) LocalDeltaPosition.Value = gesture.LocalDeltaPosition;

            if (PanCompletedEvent == null) return;
            Fsm.Event(PanCompletedEvent);
        }

    }
}