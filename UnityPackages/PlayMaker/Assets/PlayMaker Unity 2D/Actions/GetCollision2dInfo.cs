// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Gets info on the last collision 2D event and store in variables. See Unity and PlayMaker docs on Unity 2D physics.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1151")]
	public class GetCollision2dInfo : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the GameObject hit.")]
		public FsmGameObject gameObjectHit;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the relative velocity of the collision.")]
		public FsmVector3 relativeVelocity;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the relative speed of the collision. Useful for controlling reactions. E.g., selecting an appropriate sound fx.")]
		public FsmFloat relativeSpeed;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the world position of the collision contact. Useful for spawning effects etc.")]
		public FsmVector3 contactPoint;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the collision normal vector. Useful for aligning spawned effects etc.")]
		public FsmVector3 contactNormal;

		[UIHint(UIHint.Variable)]
		[Tooltip("The number of separate shaped regions in the collider.")]
		public FsmInt shapeCount;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the name of the physics 2D material of the colliding GameObject. Useful for triggering different effects. Audio, particles...")]
		public FsmString physics2dMaterialName;
		
		public override void Reset()
		{
			gameObjectHit = null;
			relativeVelocity = null;
			relativeSpeed = null;
			contactPoint = null;
			contactNormal = null;
			shapeCount = null;
			physics2dMaterialName = null;
		}
		
		void StoreCollisionInfo()
		{
			PlayMakerUnity2DProxy _proxy = Fsm.GameObject.GetComponent<PlayMakerUnity2DProxy>();

			if (_proxy == null || _proxy.lastCollision2DInfo == null)
			{
				return;
			}
			
			gameObjectHit.Value = _proxy.lastCollision2DInfo.gameObject;
			relativeSpeed.Value = _proxy.lastCollision2DInfo.relativeVelocity.magnitude;
			relativeVelocity.Value = _proxy.lastCollision2DInfo.relativeVelocity;
			physics2dMaterialName.Value = _proxy.lastCollision2DInfo.collider.sharedMaterial!=null?_proxy.lastCollision2DInfo.collider.sharedMaterial.name:"";

			shapeCount.Value = _proxy.lastCollision2DInfo.collider.shapeCount;

			if (_proxy.lastCollision2DInfo.contacts != null && _proxy.lastCollision2DInfo.contacts.Length > 0)
			{
				contactPoint.Value = _proxy.lastCollision2DInfo.contacts[0].point;
				contactNormal.Value = _proxy.lastCollision2DInfo.contacts[0].normal;
			}
		}
		
		public override void OnEnter()
		{
			StoreCollisionInfo();
			
			Finish();
		}
	}
}