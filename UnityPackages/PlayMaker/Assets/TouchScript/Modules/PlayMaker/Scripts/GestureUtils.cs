/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using HutongGames.PlayMaker;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Modules.Playmaker
{
    public static class GestureUtils
    {
        public static T GetGesture<T>(Fsm fsm, FsmOwnerDefault owner, Component component) where T : Gesture
        {
            return GetGesture<T>(fsm, owner, null, component);
        }

        public static T GetGesture<T>(Fsm fsm, FsmOwnerDefault owner, FsmString gesture, Component component) where T : Gesture
        {
            var go = fsm.GetOwnerDefaultTarget(owner);

            if (component != null) return component as T;
            if (go == null) return null;
            if (gesture == null || gesture.IsNone || string.IsNullOrEmpty(gesture.Value)) return go.GetComponent<T>();

            return go.GetComponent(gesture.Value) as T;
        }
    }
}