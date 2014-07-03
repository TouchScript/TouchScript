// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{

	public abstract class RigidBody2dActionBase : FsmStateAction
	{

		protected Rigidbody2D rb2d;
		
		protected void CacheRigidBody2d(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			
			rb2d = go.rigidbody2D ;
			
			if (rb2d == null)
			{
				LogWarning("Missing rigid body 2D: " + go.name);
				return;
			}
			
			
		}



	}
}