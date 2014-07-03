// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Sets the 2d Velocity of a Game Object. To leave any axis unchanged, set variable to 'None'. NOTE: Game object must have a rigidbody 2D.")]
	public class SetVelocity2d : RigidBody2dActionBase
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		
		[UIHint(UIHint.Variable)]
		public FsmVector2 vector;
		
		public FsmFloat x;
		public FsmFloat y;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			gameObject = null;
			vector = null;
			// default axis to variable dropdown with None selected.
			x = new FsmFloat { UseVariable = true };
			y = new FsmFloat { UseVariable = true };


			everyFrame = false;
		}
		
		public override void Awake()
		{
			Fsm.HandleFixedUpdate = true;
		}		
		
		// TODO: test this works in OnEnter!
		public override void OnEnter()
		{
			CacheRigidBody2d(Fsm.GetOwnerDefaultTarget(gameObject));

			DoSetVelocity();
			
			if (!everyFrame)
			{
				Finish();
			}		
		}
		
		public override void OnFixedUpdate()
		{
			DoSetVelocity();
			
			if (!everyFrame)
				Finish();
		}
		
		void DoSetVelocity()
		{
			if (rb2d == null)
			{
				return;
			}
			
			// init position
			
			Vector2 velocity;
			
			if (vector.IsNone)
			{
				velocity = rb2d.velocity;

			}
			else
			{
				velocity = vector.Value;
			}
			
			// override any axis
			
			if (!x.IsNone) velocity.x = x.Value;
			if (!y.IsNone) velocity.y = y.Value;
			
			// apply
			
			rb2d.velocity = velocity;
		}
	}
}