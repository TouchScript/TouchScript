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

        public static T GetGesture<T>(Fsm fsm, FsmOwnerDefault owner, Component component, bool add) where T : Gesture
        {
            return GetGesture<T>(fsm, owner, null, component, add);
        }

        public static T GetGesture<T>(Fsm fsm, FsmOwnerDefault owner, FsmString gesture, Component component, bool add) where T : Gesture
        {
            if (component != null) return component as T;

            var go = fsm.GetOwnerDefaultTarget(owner);
            if (go == null) return null;
            T g;
            if (gesture == null || gesture.IsNone || string.IsNullOrEmpty(gesture.Value)) g = go.GetComponent<T>();
            else g = go.GetComponent(gesture.Value) as T;

            if (g != null) return g;
            if (add) return go.AddComponent<T>();
            return null;
        }
    }
}