// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Sets The degree to which this object is affected by gravity.  NOTE: Game object must have a rigidbody 2D.")]
	public class SetGravity2dScale : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		[RequiredField]
		public FsmFloat gravityScale;
		
		public override void Reset()
		{
			gameObject = null;
			gravityScale = 1f;
		}
		
		public override void OnEnter()
		{
			DoSetGravityScale();
			Finish();
		}
		
		void DoSetGravityScale()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			go.rigidbody2D.gravityScale = gravityScale.Value;
		}
	}
}