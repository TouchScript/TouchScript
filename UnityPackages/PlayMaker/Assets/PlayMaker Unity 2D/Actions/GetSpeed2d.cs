// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Gets the 2d Speed of a Game Object and stores it in a Float Variable. NOTE: The Game Object must have a rigid body 2D.")]
	public class GetSpeed2d : RigidBody2dActionBase
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeResult;
		public bool everyFrame;
		
		public override void Reset()
		{
			gameObject = null;
			storeResult = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			CacheRigidBody2d(Fsm.GetOwnerDefaultTarget(gameObject));

			DoGetSpeed();
			
			if (!everyFrame)
				Finish();		
		}
		
		public override void OnUpdate()
		{
			DoGetSpeed();
		}
		
		void DoGetSpeed()
		{
			if (storeResult == null || rb2d == null)
			{
				return;
			}

			storeResult.Value = rb2d.velocity.magnitude;
		}
		
		
	}
}