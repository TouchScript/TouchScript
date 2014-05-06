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
    [HutongGames.PlayMaker.Tooltip("Sends event when rotate gesture is performed on an object.")]
    public class Rotated : FsmStateAction
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
        public FsmFloat DeltaRotation;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent RotateStartedEvent;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent RotatedEvent;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent RotateCompletedEvent;

        protected SimpleRotateGesture gesture;

        public override void Reset()
        {
            GameObject = null;
            Component = null;
            ScreenPosition = null;
            PreviousScreenPosition = null;
            DeltaRotation = null;
            RotateStartedEvent = null;
            RotatedEvent = null;
            RotateCompletedEvent = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<SimpleRotateGesture>(Fsm, GameObject, Component, true);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

            gesture.RotateStarted += gestureRotateStartedHandler;
            gesture.Rotated += gestureRotatedHandler;
            gesture.RotateCompleted += gestureRotateCompletedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.RotateStarted -= gestureRotateStartedHandler;
            gesture.Rotated -= gestureRotatedHandler;
            gesture.RotateCompleted -= gestureRotateCompletedHandler;
        }

        private void gestureRotateStartedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (DeltaRotation != null) DeltaRotation.Value = gesture.DeltaRotation;

            if (RotateStartedEvent == null) return;
            Fsm.Event(RotateStartedEvent);
        }

        private void gestureRotatedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (DeltaRotation != null) DeltaRotation.Value = gesture.DeltaRotation;

            if (RotatedEvent == null) return;
            Fsm.Event(RotatedEvent);
        }

        private void gestureRotateCompletedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (DeltaRotation != null) DeltaRotation.Value = gesture.DeltaRotation;

            if (RotateCompletedEvent == null) return;
            Fsm.Event(RotateCompletedEvent);
        }

    }
}