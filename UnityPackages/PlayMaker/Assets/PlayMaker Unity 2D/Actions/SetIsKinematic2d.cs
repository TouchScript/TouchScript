// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Controls whether 2D physics affects the Game Object.")]
	public class SetIsKinematic2d : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		[RequiredField]
		public FsmBool isKinematic;
		
		public override void Reset()
		{
			gameObject = null;
			isKinematic = false;
		}
		
		public override void OnEnter()
		{
			DoSetIsKinematic();
			Finish();
		}
		
		void DoSetIsKinematic()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			go.rigidbody2D.isKinematic = isKinematic.Value;
		}
	}
}

