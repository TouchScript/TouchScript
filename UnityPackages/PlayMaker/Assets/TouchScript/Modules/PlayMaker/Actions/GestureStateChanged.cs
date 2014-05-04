/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using HutongGames.PlayMaker;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Modules.Playmaker.Actions
{
    [ActionCategory("TouchScript")]
    [HutongGames.PlayMaker.Tooltip("Sends events when a gesture changes state.")]
    public class GestureStateChanged : FsmStateAction
    {
        [HutongGames.PlayMaker.Tooltip("The GameObject that owns the Gesture.")]
        public FsmOwnerDefault GameObject;

        [UIHint(UIHint.Behaviour)]
        [HutongGames.PlayMaker.Tooltip("The name of the Gesture.")]
        public FsmString Gesture;

        [HutongGames.PlayMaker.Tooltip("Optionally drag a component directly into this field (gesture name will be ignored).")]
        public Component Component;

        public Gesture.GestureState TargetState = Gestures.Gesture.GestureState.Recognized;
        public FsmEvent SendEvent;

        private Gesture gesture;

        public override void Reset()
        {
            TargetState = Gestures.Gesture.GestureState.Recognized;
            GameObject = null;
            Gesture = null;
            Component = null;
            SendEvent = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<Gesture>(Fsm, GameObject, Gesture, Component);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

            gesture.StateChanged += gestureStateChangedHandler;
        }

        public override void OnExit()
        {
            if (gesture == null) return;
            gesture.StateChanged -= gestureStateChangedHandler;
        }

        public override string ErrorCheck()
        {
            if (GestureUtils.GetGesture<Gesture>(Fsm, GameObject, Gesture, Component) == null) return "Gesture is missing";
            return null;
        }

        private void gestureStateChangedHandler(object sender, GestureStateChangeEventArgs e)
        {
            if (e.State != TargetState) return;
            if (SendEvent == null) return;
            Fsm.Event(SendEvent);
        }

    }
}