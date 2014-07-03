// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Casts a Ray against all Colliders in the scene." +
		"A linecast is an imaginary line between two points in world space. Any object making contact with the beam can be detected and reported. This differs from the similar raycast in that raycasting specifies the line using an origin and direction." +
		"Use GetRaycastHit2dInfo to get more detailed info.")]
	public class LineCast2d : FsmStateAction
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
		
		
		Transform _fromTrans;
		Transform _toTrans;

		int repeat;
		
		public override void Reset()
		{
			fromGameObject = null;
			fromPosition = new FsmVector2 { UseVariable = true };

			toGameObject = null;
			toPosition = new FsmVector2 { UseVariable = true };
		
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
			GameObject fromGo = Fsm.GetOwnerDefaultTarget(fromGameObject);
			
			if (fromGo!=null)
			{
				_fromTrans = fromGo.transform;
			}

			GameObject toGo = toGameObject.Value;
			
			if (toGo!=null)
			{
				_toTrans = toGo.transform;
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
			
			Vector2 fromPos = fromPosition.Value;
			
			if (_fromTrans!=null)
			{
				fromPos.x += _fromTrans.position.x;
				fromPos.y += _fromTrans.position.y;
			}

			Vector2 toPos = toPosition.Value;
			
			if (_toTrans!=null)
			{
				toPos.x += _toTrans.position.x;
				toPos.y += _toTrans.position.y;
			}

			
			RaycastHit2D hitInfo;
			
			if (minDepth.IsNone && maxDepth.IsNone)
			{
				hitInfo = Physics2D.Linecast(fromPos,toPos,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value));
			}else{
				float _minDepth = minDepth.IsNone? Mathf.NegativeInfinity : minDepth.Value;
				float _maxDepth = maxDepth.IsNone? Mathf.Infinity : maxDepth.Value;
				hitInfo = Physics2D.Linecast(fromPos,toPos,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value),_minDepth,_maxDepth);
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
				Vector3 start = new Vector3(fromPos.x,fromPos.y,0);
				Vector3 end = new Vector3(toPos.x,toPos.y,0);
				
				Debug.DrawLine(start,end, debugColor.Value);
			}
		}
	}
}

