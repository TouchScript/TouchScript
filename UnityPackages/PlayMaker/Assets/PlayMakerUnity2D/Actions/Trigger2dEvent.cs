// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Detect 2D trigger collisions between the Owner of this FSM and other Game Objects that have RigidBody2D components.\nNOTE: The system events, TRIGGER ENTER 2D, TRIGGER STAY 2D, and TRIGGER EXIT 2D are sent automatically on collisions triggers with any object. Use this action to filter collision triggers by Tag.")]
	public class Trigger2dEvent : FsmStateAction
	{
		
		[Tooltip("The type of trigger to detect.")]
		public PlayMakerUnity2d.Trigger2DType trigger;
		
		[UIHint(UIHint.Tag)]
		[Tooltip("Filter by Tag.")]
		public FsmString collideTag;
		
		[RequiredField]
		[Tooltip("Event to send if a collision is detected.")]
		public FsmEvent sendEvent;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the GameObject that collided with the Owner of this FSM.")]
		public FsmGameObject storeCollider;

		
		
		private PlayMakerUnity2DProxy _proxy;
		
		public override void Reset()
		{
			trigger =  PlayMakerUnity2d.Trigger2DType.OnTriggerEnter2D;
			collideTag = new FsmString(){UseVariable=true};
			sendEvent = null;
			storeCollider = null;
		}
		
		public override void OnEnter()
		{
			_proxy = (PlayMakerUnity2DProxy) this.Owner.GetComponent<PlayMakerUnity2DProxy>();
			
			if (_proxy == null)
			{
				_proxy = this.Owner.AddComponent<PlayMakerUnity2DProxy>();
			}
			
			switch (trigger)
			{
			case PlayMakerUnity2d.Trigger2DType.OnTriggerEnter2D:
				_proxy.AddOnTriggerEnter2dDelegate(this.DoTriggerEnter2D);
				break;
			case PlayMakerUnity2d.Trigger2DType.OnTriggerStay2D:
				_proxy.AddOnTriggerStay2dDelegate(this.DoTriggerStay2D);
				break;
			case PlayMakerUnity2d.Trigger2DType.OnTriggerExit2D:
				_proxy.AddOnTriggerExit2dDelegate(this.DoTriggerExit2D);
				break;
			}
		}
		
		public override void OnExit()
		{
			if (_proxy==null)
			{
				return;
			}
			
			switch (trigger)
			{
			case PlayMakerUnity2d.Trigger2DType.OnTriggerEnter2D:
				_proxy.RemoveOnTriggerEnter2dDelegate(this.DoTriggerEnter2D);
				break;
			case PlayMakerUnity2d.Trigger2DType.OnTriggerStay2D:
				_proxy.RemoveOnTriggerStay2dDelegate(this.DoTriggerStay2D);
				break;
			case PlayMakerUnity2d.Trigger2DType.OnTriggerExit2D:
				_proxy.RemoveOnTriggerExit2dDelegate(this.DoTriggerExit2D);
				break;
			}
		}
		
		void StoreCollisionInfo(Collider2D collisionInfo)
		{
			storeCollider.Value = collisionInfo.gameObject;
		}
		
		public void DoTriggerEnter2D(Collider2D collisionInfo)
		{
			if (trigger == PlayMakerUnity2d.Trigger2DType.OnTriggerEnter2D)
			{
				if (collisionInfo.gameObject.tag == collideTag.Value || collideTag.IsNone || string.IsNullOrEmpty(collideTag.Value) )
				{
					StoreCollisionInfo(collisionInfo);
					Fsm.Event(sendEvent);
				}
			}
		}
		
		public void DoTriggerStay2D(Collider2D collisionInfo)
		{
			if (trigger == PlayMakerUnity2d.Trigger2DType.OnTriggerStay2D)
			{
				if (collisionInfo.gameObject.tag == collideTag.Value || collideTag.IsNone || string.IsNullOrEmpty(collideTag.Value) )
				{
					StoreCollisionInfo(collisionInfo);
					Fsm.Event(sendEvent);
				}
			}
		}
		
		public void DoTriggerExit2D(Collider2D collisionInfo)
		{
			if (trigger == PlayMakerUnity2d.Trigger2DType.OnTriggerExit2D)
			{
				if (collisionInfo.gameObject.tag == collideTag.Value || collideTag.IsNone || string.IsNullOrEmpty(collideTag.Value))
				{
					StoreCollisionInfo(collisionInfo);
					Fsm.Event(sendEvent);
				}
			}
		}
		
		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (Owner != null && Owner.GetComponent<Collider2D>() == null && Owner.GetComponent<Rigidbody2D>() == null)
			{
				text += "Owner requires a RigidBody2D or Collider2D!\n";
			}
			return text;
		}
	}
}