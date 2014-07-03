// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Controls whether the rigidbody 2D should be prevented from rotating")]
	public class SetIsFixedAngle2d : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		[RequiredField]
		public FsmBool isFixedAngle;
		
		public override void Reset()
		{
			gameObject = null;
			isFixedAngle = false;
		}
		
		public override void OnEnter()
		{
			DoSetIsFixedAngle();
			Finish();
		}
		
		void DoSetIsFixedAngle()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			go.rigidbody2D.fixedAngle = isFixedAngle.Value;
		}
	}
}

