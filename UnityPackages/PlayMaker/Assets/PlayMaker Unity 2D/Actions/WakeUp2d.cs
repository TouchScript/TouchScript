// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Forces a Game Object's Rigid Body 2D to wake up.")]
	public class WakeUp2d : FsmStateAction
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
			DoWakeUp();
			Finish();
		}
		
		void DoWakeUp()
		{
			GameObject go = gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : gameObject.GameObject.Value;
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			go.rigidbody2D.WakeUp();
		}
	}
}