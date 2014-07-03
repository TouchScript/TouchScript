// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Should the rigidbody2D be prevented from rotating?")]
	public class IsFixedAngle2d : RigidBody2dActionBase
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		
		public FsmEvent trueEvent;
		
		public FsmEvent falseEvent;
		
		[UIHint(UIHint.Variable)]
		public FsmBool store;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			gameObject = null;
			trueEvent = null;
			falseEvent = null;
			store = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			CacheRigidBody2d(Fsm.GetOwnerDefaultTarget(gameObject));
			
			DoIsFixedAngle();
			
			if (!everyFrame)
			{
				Finish();
			}
		}
		
		public override void OnUpdate()
		{
			DoIsFixedAngle();
		}
		
		void DoIsFixedAngle()
		{
			
			if (rb2d == null)
			{
				return;
			}
			
			var isfixedAngle = rb2d.fixedAngle;
			store.Value = isfixedAngle;
			
			Fsm.Event(isfixedAngle ? trueEvent : falseEvent);
		}
	}
}

