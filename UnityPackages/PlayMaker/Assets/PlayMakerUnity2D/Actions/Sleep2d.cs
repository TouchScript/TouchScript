// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Forces a Game Object's Rigid Body 2D to Sleep at least one frame.")]
	public class Sleep2d : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		
		public override void Reset()
		{
			gameObject = null;
		}
		
		public override void OnEnter()
		{
			DoSleep();
			Finish();
		}
		
		void DoSleep()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			go.rigidbody2D.Sleep();
		}
	}
}