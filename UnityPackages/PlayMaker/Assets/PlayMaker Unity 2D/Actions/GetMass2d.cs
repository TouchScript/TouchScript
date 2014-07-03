// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Gets the Mass of a Game Object's Rigid Body 2D.")]
	public class GetMass2d : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeResult;
		
		public override void Reset()
		{
			gameObject = null;
			storeResult = null;
		}
		
		public override void OnEnter()
		{
			DoGetMass();
			
			Finish();
		}
		
		void DoGetMass()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			storeResult.Value = go.rigidbody2D.mass;
		}
	}
}