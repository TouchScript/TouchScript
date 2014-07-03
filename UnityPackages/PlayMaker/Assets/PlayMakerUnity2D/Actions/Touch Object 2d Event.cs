// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends events when a 2d object is touched. Optionally filter by a fingerID. NOTE: Uses the MainCamera!")]
	public class TouchObject2dEvent : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Collider2D))]
		[Tooltip("The Game Object to detect touches on.")]
		public FsmOwnerDefault gameObject;
		
		[Tooltip("Only detect touches that match this fingerID, or set to None.")]
		public FsmInt fingerId;
		
		[ActionSection("Events")]
		
		[Tooltip("Event to send on touch began.")]
		public FsmEvent touchBegan;
		
		[Tooltip("Event to send on touch moved.")]
		public FsmEvent touchMoved;
		
		[Tooltip("Event to send on stationary touch.")]
		public FsmEvent touchStationary;
		
		[Tooltip("Event to send on touch ended.")]
		public FsmEvent touchEnded;
		
		[Tooltip("Event to send on touch cancel.")]
		public FsmEvent touchCanceled;
		
		[ActionSection("Store Results")]
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the fingerId of the touch.")]
		public FsmInt storeFingerId;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the 2d position where the object was touched.")]
		public FsmVector2 storeHitPoint;
		
		public override void Reset()
		{
			gameObject = null;
			fingerId = new FsmInt { UseVariable = true };
			
			touchBegan = null;
			touchMoved = null;
			touchStationary = null;
			touchEnded = null;
			touchCanceled = null;
			
			storeFingerId = null;
			storeHitPoint = null;
		}
		
		public override void OnUpdate()
		{
			if (Camera.main == null)
			{
				LogError("No MainCamera defined!");
				Finish();
				return;
			}
			
			if (Input.touchCount > 0)
			{
				var go = Fsm.GetOwnerDefaultTarget(gameObject);
				if (go == null)
				{
					return;
				}
				
				foreach (var touch in Input.touches)
				{
					if (fingerId.IsNone || touch.fingerId == fingerId.Value)
					{
						var screenPos = touch.position;
						
						RaycastHit2D hitInfo = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(screenPos),Mathf.Infinity);
						
						// Store hitInfo so it can be accessed by other actions
						// E.g., Get Raycast Hit 2d Info
						PlayMakerUnity2d.RecordLastRaycastHitInfo(this.Fsm,hitInfo);
						
						if (hitInfo.transform != null)
						{
							if (hitInfo.transform.gameObject == go)
							{
								storeFingerId.Value = touch.fingerId;
								storeHitPoint.Value = hitInfo.point;
								
								switch (touch.phase)
								{
								case TouchPhase.Began:
									Fsm.Event(touchBegan);
									return;
									
								case TouchPhase.Moved:
									Fsm.Event(touchMoved);
									return;
									
								case TouchPhase.Stationary:
									Fsm.Event(touchStationary);
									return;
									
								case TouchPhase.Ended:
									Fsm.Event(touchEnded);
									return;
									
								case TouchPhase.Canceled:
									Fsm.Event(touchCanceled);
									return;
								}
							}
						}
					}
				}
			}
		}
	}
}