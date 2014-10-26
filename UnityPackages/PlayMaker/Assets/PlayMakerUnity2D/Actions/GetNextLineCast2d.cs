// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Iterate through a list of all colliders detected by a LineCast" +
	         "The colliders iterated are sorted in order of increasing Z coordinate. No iteration will take place if there are no colliders within the area.")]
	public class GetNextLineCast2d: FsmStateAction
	{
		[ActionSection("Setup")]
		
		[Tooltip("Start ray at game object position. \nOr use From Position parameter.")]
		public FsmOwnerDefault fromGameObject;
		
		[Tooltip("Start ray at a vector2 world position. \nOr use fromGameObject parameter. If both define, will add fromPosition to the fromGameObject position")]
		public FsmVector2 fromPosition;
		
		[Tooltip("End ray at game object position. \nOr use From Position parameter.")]
		public FsmGameObject toGameObject;
		
		[Tooltip("End ray at a vector2 world position. \nOr use fromGameObject parameter. If both define, will add toPosition to the ToGameObject position")]
		public FsmVector2 toPosition;
		
		[Tooltip("Only include objects with a Z coordinate (depth) greater than this value. leave to none for no effect")]
		public FsmInt minDepth;
		
		[Tooltip("Only include objects with a Z coordinate (depth) less than this value. leave to none")]
		public FsmInt maxDepth;
		
		[ActionSection("Filter")] 
		
		[UIHint(UIHint.Layer)]
		[Tooltip("Pick only from these layers.")]
		public FsmInt[] layerMask;
		
		[Tooltip("Invert the mask, so you pick from all layers except those defined above.")]
		public FsmBool invertMask;
		
		[ActionSection("Result")] 
		
		[Tooltip("Store the number of colliders found for this overlap.")]
		[UIHint(UIHint.Variable)]
		public FsmInt collidersCount;
		
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the next collider in a GameObject variable.")]
		public FsmGameObject storeNextCollider;
		
		[Tooltip("Get the 2d position of the next ray hit point and store it in a variable.")]
		public FsmVector2 storeNextHitPoint;
		
		[Tooltip("Get the 2d normal at the next hit point and store it in a variable.")]
		public FsmVector2 storeNextHitNormal;
		
		[Tooltip("Get the distance along the ray to the next hit point and store it in a variable.")]
		public FsmFloat storeNextHitDistance;
		
		[Tooltip("Event to send to get the next collider.")]
		public FsmEvent loopEvent;
		
		[Tooltip("Event to send when there are no more colliders to iterate.")]
		public FsmEvent finishedEvent;
		
		
		private RaycastHit2D[] hits;
		
		private int colliderCount;
		
		// increment an index as we loop through children
		private int nextColliderIndex;
		
		public override void Reset()
		{
			fromGameObject = null;
			fromPosition = new FsmVector2 { UseVariable = true };
			
			toGameObject = null;
			toPosition = new FsmVector2 { UseVariable = true };

			minDepth = new FsmInt { UseVariable = true };
			maxDepth = new FsmInt { UseVariable = true };
			
			layerMask = new FsmInt[0];
			invertMask = false;
			
			collidersCount = null;
			storeNextCollider = null;
			storeNextHitPoint = null;
			storeNextHitNormal = null;
			storeNextHitDistance = null;
			loopEvent = null;
			finishedEvent = null;
		}
		
		public override void OnEnter()
		{
			if (hits == null)
			{
				hits = GetLineCastAll();
				colliderCount = hits.Length;
				collidersCount.Value = colliderCount;
			}
			
			DoGetNextCollider();
			
			Finish();
			
		}
		
		void DoGetNextCollider()
		{
			
			// no more colliders?
			// check first to avoid errors.
			
			if (nextColliderIndex >= colliderCount)
			{
				hits = new RaycastHit2D[0];
				nextColliderIndex = 0;
				Fsm.Event(finishedEvent);
				return;
			}
			
			// get next collider
			PlayMakerUnity2d.RecordLastRaycastHitInfo(this.Fsm,hits[nextColliderIndex]);
			storeNextCollider.Value = hits[nextColliderIndex].collider.gameObject;
			storeNextHitPoint.Value = hits[nextColliderIndex].point;
			storeNextHitNormal.Value = hits[nextColliderIndex].normal;
			storeNextHitDistance.Value = hits[nextColliderIndex].fraction;
			
			// no more colliders?
			// check a second time to avoid process lock and possible infinite loop if the action is called again.
			// Practically, this enabled calling again this state and it will start again iterating from the first child.
			
			if (nextColliderIndex >= colliderCount)
			{
				hits = new RaycastHit2D[0];
				nextColliderIndex = 0;
				Fsm.Event(finishedEvent);
				return;
			}
			
			// iterate the next collider
			nextColliderIndex++;
			
			if (loopEvent != null)
			{
				Fsm.Event(loopEvent);
			}
		}
		
		
		RaycastHit2D[] GetLineCastAll()
		{

			Vector2 fromPos = fromPosition.Value;

			GameObject fromGo = Fsm.GetOwnerDefaultTarget(fromGameObject);
			
			if (fromGo!=null)
			{
				fromPos.x += fromGo.transform.position.x;
				fromPos.y += fromGo.transform.position.y;
			}

			Vector2 toPos = toPosition.Value;

			GameObject toGo = toGameObject.Value;
			
			if (toGo!=null)
			{
				toPos.x += toGo.transform.position.x;
				toPos.y += toGo.transform.position.y;
			}

			
			if (minDepth.IsNone && maxDepth.IsNone)
			{
				return Physics2D.LinecastAll(fromPos,toPos,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value));
			}else{
				float _minDepth = minDepth.IsNone? Mathf.NegativeInfinity : minDepth.Value;
				float _maxDepth = maxDepth.IsNone? Mathf.Infinity : maxDepth.Value;
				return Physics2D.LinecastAll(fromPos,toPos,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value),_minDepth,_maxDepth);
			}
			
			
		}
		
	}
}

