// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Rigid bodies 2D start sleeping when they come to rest. This action wakes up all rigid bodies 2D in the scene. E.g., if you Set Gravity 2D and want objects at rest to respond.")]
	public class WakeAllRigidBodies2d : FsmStateAction
	{
		public bool everyFrame;
		
		private Rigidbody2D[] bodies;
		
		public override void Reset()
		{
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			DoWakeAll();
			
			if (!everyFrame)
				Finish();		
		}
		
		public override void OnUpdate()
		{
			DoWakeAll();
		}
		
		void DoWakeAll()
		{
			bodies = Object.FindObjectsOfType(typeof(Rigidbody2D)) as Rigidbody2D[];
			
			if (bodies != null)
			{
				foreach (var body in bodies)
				{
					body.WakeUp();
				}
			}
		}
	}
}