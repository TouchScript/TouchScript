// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Sets the Mass of a Game Object's Rigid Body 2D.")]
	public class SetMass2d : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		public FsmOwnerDefault gameObject;
		[RequiredField]
		[HasFloatSlider(0.1f,10f)]
		public FsmFloat mass;
		
		public override void Reset()
		{
			gameObject = null;
			mass = 1;
		}
		
		public override void OnEnter()
		{
			DoSetMass();
			
			Finish();
		}
		
		void DoSetMass()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null) return;
			if (go.rigidbody2D == null) return;
			
			go.rigidbody2D.mass = mass.Value;
		}
	}
}