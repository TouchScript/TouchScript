// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Sets the gravity vector, or individual axis.")]
	public class SetGravity2d : FsmStateAction
	{
		public FsmVector2 vector;
		public FsmFloat x;
		public FsmFloat y;
		public bool everyFrame;
		
		public override void Reset()
		{
			vector = null;
			x = new FsmFloat { UseVariable = true };
			y = new FsmFloat { UseVariable = true };
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			DoSetGravity();
			
			if (!everyFrame)
				Finish();		
		}
		
		public override void OnUpdate()
		{
			DoSetGravity();
		}
		
		void DoSetGravity()
		{
			Vector2 gravity = vector.Value;
			
			if (!x.IsNone)
				gravity.x = x.Value;
			if (!y.IsNone)
				gravity.y = y.Value;

			Physics2D.gravity = gravity;
		}
	}
}