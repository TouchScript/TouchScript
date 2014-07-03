// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Gets the 2d Velocity of a Game Object and stores it in a Vector2 Variable or each Axis in a Float Variable. NOTE: The Game Object must have a Rigid Body 2D.")]
	public class GetVelocity2d : RigidBody2dActionBase
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		[UIHint(UIHint.Variable)]
		public FsmVector2 vector;
		[UIHint(UIHint.Variable)]
		public FsmFloat x;
		[UIHint(UIHint.Variable)]
		public FsmFloat y;

		public Space space;
		public bool everyFrame;
		
		public override void Reset()
		{
			gameObject = null;
			vector = null;
			x = null;
			y = null;

			space = Space.World;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			CacheRigidBody2d(Fsm.GetOwnerDefaultTarget(gameObject));

			DoGetVelocity();
			
			if (!everyFrame)
				Finish();		
		}
		
		public override void OnUpdate()
		{
			DoGetVelocity();
		}
		
		void DoGetVelocity()
		{

			if (rb2d == null){
				return;
			}
			
			Vector2 velocity = rb2d.velocity;
			
			if (space == Space.Self)
				velocity = rb2d.transform.InverseTransformDirection(velocity);
			
			vector.Value = velocity;
			x.Value = velocity.x;
			y.Value = velocity.y;
		}
		
		
	}
}