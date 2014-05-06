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
    [HutongGames.PlayMaker.Tooltip("Sends event when scale gesture is performed on an object.")]
    public class Scaled : FsmStateAction
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
        public FsmFloat LocalDeltaScale;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent ScaleStartedEvent;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent ScaledEvent;

        [UIHint(UIHint.FsmEvent)]
        public FsmEvent ScaleCompletedEvent;

        protected SimpleScaleGesture gesture;

        public override void Reset()
        {
            GameObject = null;
            Component = null;
            ScreenPosition = null;
            PreviousScreenPosition = null;
            LocalDeltaScale = null;
            ScaleStartedEvent = null;
            ScaledEvent = null;
            ScaleCompletedEvent = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<SimpleScaleGesture>(Fsm, GameObject, Component, true);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

            gesture.ScaleStarted += gestureRotateStartedHandler;
            gesture.Scaled += gestureRotatedHandler;
            gesture.ScaleCompleted += gestureRotateCompletedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.ScaleStarted -= gestureRotateStartedHandler;
            gesture.Scaled -= gestureRotatedHandler;
            gesture.ScaleCompleted -= gestureRotateCompletedHandler;
        }

        private void gestureRotateStartedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (LocalDeltaScale != null) LocalDeltaScale.Value = gesture.LocalDeltaScale;

            if (ScaleStartedEvent == null) return;
            Fsm.Event(ScaleStartedEvent);
        }

        private void gestureRotatedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (LocalDeltaScale != null) LocalDeltaScale.Value = gesture.LocalDeltaScale;

            if (ScaledEvent == null) return;
            Fsm.Event(ScaledEvent);
        }

        private void gestureRotateCompletedHandler(object sender, EventArgs e)
        {
            if (ScreenPosition != null) ScreenPosition.Value = gesture.ScreenPosition;
            if (PreviousScreenPosition != null) PreviousScreenPosition.Value = gesture.PreviousScreenPosition;
            if (LocalDeltaScale != null) LocalDeltaScale.Value = gesture.LocalDeltaScale;

            if (ScaleCompletedEvent == null) return;
            Fsm.Event(ScaleCompletedEvent);
        }

    }
}