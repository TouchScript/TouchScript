// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Casts a Ray against all Colliders in the scene. " +
		"A raycast is conceptually like a laser beam that is fired from a point in space along a particular direction. Any object making contact with the beam can be detected and reported. " +
		"Use GetRaycastHit2dInfo to get more detailed info.")]
	public class RayCast2d : FsmStateAction
	{
		[ActionSection("Setup")]
		
		[Tooltip("Start ray at game object position. \nOr use From Position parameter.")]
		public FsmOwnerDefault fromGameObject;
		
		[Tooltip("Start ray at a vector2 world position. \nOr use Game Object parameter.")]
		public FsmVector2 fromPosition;
		
		[Tooltip("A vector2 direction vector")]
		public FsmVector2 direction;

		[Tooltip("Cast the ray in world or local space. Note if no Game Object is specified, the direction is in world space.")]
		public Space space;

		[Tooltip("The length of the ray. Set to -1 for infinity.")]
		public FsmFloat distance;

		[Tooltip("Only include objects with a Z coordinate (depth) greater than this value. leave to none for no effect")]
		public FsmInt minDepth;

		[Tooltip("Only include objects with a Z coordinate (depth) less than this value. leave to none")]
		public FsmInt maxDepth;

		[ActionSection("Result")] 
		
		[Tooltip("Event to send if the ray hits an object.")]
		[UIHint(UIHint.Variable)]
		public FsmEvent hitEvent;
		
		[Tooltip("Set a bool variable to true if hit something, otherwise false.")]
		[UIHint(UIHint.Variable)]
		public FsmBool storeDidHit;
		
		[Tooltip("Store the game object hit in a variable.")]
		[UIHint(UIHint.Variable)]
		public FsmGameObject storeHitObject;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the 2d position of the ray hit point and store it in a variable.")]
		public FsmVector2 storeHitPoint;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the 2d normal at the hit point and store it in a variable.")]
		public FsmVector2 storeHitNormal;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the distance along the ray to the hit point and store it in a variable.")]
		public FsmFloat storeHitDistance;
		
		[ActionSection("Filter")] 
		
		[Tooltip("Set how often to cast a ray. 0 = once, don't repeat; 1 = everyFrame; 2 = every other frame... \nSince raycasts can get expensive use the highest repeat interval you can get away with.")]
		public FsmInt repeatInterval;
		
		[UIHint(UIHint.Layer)]
		[Tooltip("Pick only from these layers.")]
		public FsmInt[] layerMask;
		
		[Tooltip("Invert the mask, so you pick from all layers except those defined above.")]
		public FsmBool invertMask;
		
		[ActionSection("Debug")] 
		
		[Tooltip("The color to use for the debug line.")]
		public FsmColor debugColor;
		
		[Tooltip("Draw a debug line. Note: Check Gizmos in the Game View to see it in game.")]
		public FsmBool debug;


		Transform _trans;

		int repeat;
		
		public override void Reset()
		{
			fromGameObject = null;
			fromPosition = new FsmVector2 { UseVariable = true };
			direction = new FsmVector2 { UseVariable = true };

			space = Space.Self;

			minDepth = new FsmInt {UseVariable =true};
			maxDepth = new FsmInt {UseVariable =true};

			distance = 100;
			hitEvent = null;
			storeDidHit = null;
			storeHitObject = null;
			storeHitPoint = null;
			storeHitNormal = null;
			storeHitDistance = null;
			repeatInterval = 1;
			layerMask = new FsmInt[0];
			invertMask = false;
			debugColor = Color.yellow;
			debug = false;
		}
		
		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(fromGameObject);

			if (go!=null)
			{
				_trans = go.transform;
			}

			DoRaycast();
			
			if (repeatInterval.Value == 0)
			{
				Finish();
			}		
		}
		
		public override void OnUpdate()
		{
			repeat--;
			
			if (repeat == 0)
			{
				DoRaycast();
			}
		}
		
		void DoRaycast()
		{
			repeat = repeatInterval.Value;
			
			if (distance.Value == 0)
			{
				return;
			}

			Vector2 originPos = fromPosition.Value;

			if (_trans!=null)
			{
				originPos.x += _trans.position.x;
				originPos.y += _trans.position.y;
			}

			float rayLength = Mathf.Infinity;
			if (distance.Value > 0 )
			{
				rayLength = distance.Value;
			}
			
			Vector2 dirVector2 = direction.Value.normalized; // normalized to get the proper distance later using fraction from the rayCastHitinfo.

			if(_trans != null && space == Space.Self)
			{

				Vector3 dirVector = _trans.TransformDirection(new Vector3(direction.Value.x,direction.Value.y,0f));
				dirVector2.x = dirVector.x;
				dirVector2.y = dirVector.y;
			}

			RaycastHit2D hitInfo;

			if (minDepth.IsNone && maxDepth.IsNone)
			{
				hitInfo = Physics2D.Raycast(originPos,dirVector2,rayLength,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value));
			}else{
				float _minDepth = minDepth.IsNone? Mathf.NegativeInfinity : minDepth.Value;
				float _maxDepth = maxDepth.IsNone? Mathf.Infinity : maxDepth.Value;
				hitInfo = Physics2D.Raycast(originPos,dirVector2,rayLength,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value),_minDepth,_maxDepth);
			}

			PlayMakerUnity2d.RecordLastRaycastHitInfo(this.Fsm,hitInfo);
			
			bool didHit = hitInfo.collider != null;
			
			storeDidHit.Value = didHit;
			
			if (didHit)
			{
				storeHitObject.Value = hitInfo.collider.gameObject;
				storeHitPoint.Value = hitInfo.point;
				storeHitNormal.Value = hitInfo.normal;
				storeHitDistance.Value = hitInfo.fraction;
				Fsm.Event(hitEvent);
			}
			
			if (debug.Value)
			{
				var debugRayLength = Mathf.Min(rayLength, 1000);
				Vector3 start = new Vector3(originPos.x,originPos.y,0);
				Vector3 dirVector3 = new Vector3(dirVector2.x,dirVector2.y,0);
				Vector3 end = start + dirVector3 * debugRayLength;

				Debug.DrawLine(start,end, debugColor.Value);
			}
		}
	}
}

