/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using HutongGames.PlayMaker;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Modules.Playmaker
{
    public class GestureWrapper<T> : FsmStateAction where T : Gesture
    {
        [HutongGames.PlayMaker.Tooltip("The GameObject that owns the Gesture.")]
        public FsmOwnerDefault GameObject;

        [HutongGames.PlayMaker.Tooltip("Optionally drag a component directly into this field (gesture name will be ignored).")]
        public Component Component;

        protected T gesture;

        public override void Reset()
        {
            GameObject = null;
            Component = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<T>(Fsm, GameObject, Component);
            if (gesture == null)
            {
                var go = Fsm.GetOwnerDefaultTarget(GameObject);
                if (go == null)
                {
                    LogError("Gesture is missing");
                    return;
                }
                gesture = go.GetComponent<T>() ?? go.AddComponent<T>();
            }
        }
    }
}
