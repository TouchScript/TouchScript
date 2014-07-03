// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Adds a 2d torque (rotational force) to a Game Object.")]
	public class AddTorque2d : RigidBody2dActionBase
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		[Tooltip("The GameObject to add torque to.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Torque")]
		public FsmFloat torque;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		
		public override void Awake()
		{
			Fsm.HandleFixedUpdate = true;
		}

		public override void Reset()
		{
			gameObject = null;

			torque = null;

			everyFrame = false;
		}

		public override void OnEnter()
		{
			CacheRigidBody2d(Fsm.GetOwnerDefaultTarget(gameObject));

			DoAddTorque();
			
			if (!everyFrame)
			{
				Finish();
			}		
		}
		
		public override void OnFixedUpdate()
		{
			DoAddTorque();
		}
		
		void DoAddTorque()
		{
			if (!rb2d)
			{
				return;
			}

			rb2d.AddTorque(torque.Value);

		}
		
		
	}
}