// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Tests if a Game Object's Rigid Body 2D is sleeping.")]
	public class IsSleeping2d : RigidBody2dActionBase
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

			DoIsSleeping();
			
			if (!everyFrame)
			{
				Finish();
			}
		}
		
		public override void OnUpdate()
		{
			DoIsSleeping();
		}
		
		void DoIsSleeping()
		{	
			if (rb2d == null)
			{
				return;
			}
			
			var isSleeping = rb2d.IsSleeping();
			store.Value = isSleeping;
			
			Fsm.Event(isSleeping ? trueEvent : falseEvent);
		}
	}
}

