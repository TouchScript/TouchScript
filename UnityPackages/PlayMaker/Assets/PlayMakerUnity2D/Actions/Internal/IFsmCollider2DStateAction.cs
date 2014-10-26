using System;
using UnityEngine;
namespace HutongGames.PlayMaker
{
	public interface IFsmCollider2DStateAction
	{

		//
		// Methods
		//
		void DoCollisionEnter2D (Collision2D collisionInfo);
		
		void DoCollisionExit2D (Collision2D collisionInfo);
		
		void DoCollisionStay2D (Collision2D collisionInfo);
		
		/*
		void DoTriggerEnter2D (Collider2D other);
		
		void DoTriggerExit2D (Collider2D other);
		
		void DoTriggerStay2D (Collider2D other);
		*/
	}
}
