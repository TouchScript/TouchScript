/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using HutongGames.PlayMaker;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Modules.Playmaker.Actions
{
    [ActionCategory("TouchScript")]
    [HutongGames.PlayMaker.Tooltip("Retrieves 3D hit details from a gesture.")]
    public class Get3DHitResult : FsmStateAction
    {
        [HutongGames.PlayMaker.Tooltip("The GameObject that owns the Gesture.")]
        public FsmOwnerDefault GameObject;

        [UIHint(UIHint.Behaviour)]
        [HutongGames.PlayMaker.Tooltip("The name of the Gesture.")]
        public FsmString Gesture;

        [HutongGames.PlayMaker.Tooltip("Optionally drag a component directly into this field (gesture name will be ignored).")]
        public Component Component;

        #region Output

        [UIHint(UIHint.Variable)]
        public FsmObject Collider;

        [UIHint(UIHint.Variable)]
        public FsmVector3 Normal;

        [UIHint(UIHint.Variable)]
        public FsmVector3 Point;

        [UIHint(UIHint.Variable)]
        public FsmObject RigidBody;

        #endregion

        private Gesture gesture;

        public override void Reset()
        {
            GameObject = null;
            Gesture = null;
            Component = null;

            Collider = null;
            Normal = Vector3.zero;
            Point = Vector3.zero;
            RigidBody = null;
        }

        public override void OnEnter()
        {
            gesture = GestureUtils.GetGesture<Gesture>(Fsm, GameObject, Gesture, Component);
            if (gesture == null)
            {
                LogError("Gesture is missing");
                return;
            }

            ITouchHit hit;
            gesture.GetTargetHitResult(out hit);
            var hit3d = hit as ITouchHit3D;
            if (hit3d == null) return;

            if (Collider != null) Collider.Value = hit3d.Collider;
            if (RigidBody != null) RigidBody.Value = hit3d.Rigidbody;
            if (Normal != null) Normal.Value = hit3d.Normal;
            if (Point != null) Point.Value = hit3d.Point;

            Finish();
        }

        public override string ErrorCheck()
        {
            if (GestureUtils.GetGesture<Gesture>(Fsm, GameObject, Gesture, Component) == null) return "Gesture is missing";
            return null;
        }

    }
}
