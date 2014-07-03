// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Sends Events based on mouse interactions with a 2d Game Object: MouseOver, MouseDown, MouseUp, MouseOff.")]
	public class MousePick2dEvent : FsmStateAction
	{
		[CheckForComponent(typeof(Collider2D))]
		public FsmOwnerDefault GameObject;
		
		[Tooltip("Event to send when the mouse is over the GameObject.")]
		public FsmEvent mouseOver;
		
		[Tooltip("Event to send when the mouse is pressed while over the GameObject.")]
		public FsmEvent mouseDown;
		
		[Tooltip("Event to send when the mouse is released while over the GameObject.")]
		public FsmEvent mouseUp;
		
		[Tooltip("Event to send when the mouse moves off the GameObject.")]
		public FsmEvent mouseOff;
		
		[Tooltip("Pick only from these layers.")]
		[UIHint(UIHint.Layer)]
		public FsmInt[] layerMask;
		
		[Tooltip("Invert the mask, so you pick from all layers except those defined above.")]
		public FsmBool invertMask;
		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
		
		public override void Reset()
		{
			GameObject = null;
			mouseOver = null;
			mouseDown = null;
			mouseUp = null;
			mouseOff = null;
			layerMask = new FsmInt[0];
			invertMask = false;
			everyFrame = true;
		}
		
		public override void OnEnter()
		{
			DoMousePickEvent();
			
			if (!everyFrame)
			{
				Finish();
			}
		}
		
		public override void OnUpdate()
		{
			DoMousePickEvent();
		}
		
		void DoMousePickEvent()
		{
			// Do the raycast
			
			bool isMouseOver = DoRaycast();
			

			
			// Send events based on the raycast and mouse buttons
			
			if (isMouseOver)
			{
				if (mouseDown != null && Input.GetMouseButtonDown(0))
				{
					Fsm.Event(mouseDown);
				}
				
				if (mouseOver != null)
				{
					Fsm.Event(mouseOver);
				}
				
				if (mouseUp != null &&Input.GetMouseButtonUp(0))
				{
					Fsm.Event(mouseUp);
				}
			}
			else
			{
				if (mouseOff != null)
				{
					Fsm.Event(mouseOff);
				}
			}
		}
		
		bool DoRaycast()
		{
			GameObject testObject = GameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : GameObject.GameObject.Value;
			
			// ActionHelpers uses a cache to try and minimize Raycasts
			RaycastHit2D hitInfo = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition),Mathf.Infinity,ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask.Value));

			// Store mouse pick info so it can be seen by Get Raycast Hit Info action
			PlayMakerUnity2d.RecordLastRaycastHitInfo(this.Fsm,hitInfo);

			if (hitInfo.transform != null)
			{
				if (hitInfo.transform.gameObject == testObject)
				{
					return true;
				}
			}
			return false;
		}
	}
}