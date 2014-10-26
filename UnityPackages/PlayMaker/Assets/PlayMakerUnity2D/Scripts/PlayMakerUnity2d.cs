// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;

/// <summary>
/// This component is needed on scenes featuring GameObjects Physics 2D Colliders with Fsms listening to "TRIGGER XXX 2D" and "COLLISION XXX 2D" global events.
/// This component also keep track of 2d raycast infos when using the action "GetRaycasthit2dIngo".
/// </summary>
public class PlayMakerUnity2d : MonoBehaviour {

	static PlayMakerFSM fsmProxy;
	public static string PlayMakerUnity2dProxyName = "PlayMaker Unity 2D";

	static FsmOwnerDefault goTarget; // simple cache.

	public static string OnCollisionEnter2DEvent	= "COLLISION ENTER 2D";
	public static string OnCollisionExit2DEvent		= "COLLISION EXIT 2D";
	public static string OnCollisionStay2DEvent		= "COLLISION STAY 2D";
	public static string OnTriggerEnter2DEvent		= "TRIGGER ENTER 2D";
	public static string OnTriggerExit2DEvent		= "TRIGGER EXIT 2D";
	public static string OnTriggerStay2DEvent		= "TRIGGER STAY 2D";


	static Dictionary<Fsm,RaycastHit2D> lastRaycastHit2DInfoLUT;

	public static void RecordLastRaycastHitInfo(Fsm fsm,RaycastHit2D info)
	{
		if (lastRaycastHit2DInfoLUT==null)
		{
			lastRaycastHit2DInfoLUT = new Dictionary<Fsm, RaycastHit2D>();
		}
	
		lastRaycastHit2DInfoLUT[fsm] = info;
	}

	public static RaycastHit2D GetLastRaycastHitInfo(Fsm fsm)
	{
		if (lastRaycastHit2DInfoLUT==null)
		{
			lastRaycastHit2DInfoLUT[fsm] = new RaycastHit2D();
			return lastRaycastHit2DInfoLUT[fsm];
		}
		
		return lastRaycastHit2DInfoLUT[fsm];
	}


	public enum Collision2DType
	{
		OnCollisionEnter2D,
		OnCollisionStay2D,
		OnCollisionExit2D,
	}

	public enum Trigger2DType
	{
		OnTriggerEnter2D,
		OnTriggerStay2D,
		OnTriggerExit2D,
	}

	void Awake () {
		fsmProxy = this.GetComponent<PlayMakerFSM>();

		if (fsmProxy==null)
		{
			Debug.LogError("'PlayMaker Unity 2D' is missing." ,this);
		}

		// set the target to be this gameObject.
		goTarget = new FsmOwnerDefault();
		goTarget.GameObject = new FsmGameObject();
		goTarget.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
		
		// send the event to this gameObject
		FsmEventTarget eventTarget = new FsmEventTarget();
		eventTarget.excludeSelf = false;
		eventTarget.target = FsmEventTarget.EventTarget.GameObject;
		eventTarget.gameObject = goTarget;
		eventTarget.sendToChildren = false;
	}

	void OnLevelWasLoaded(int level) {
	
		if (lastRaycastHit2DInfoLUT!=null)
		{
			// clean up raycasting info LUT to avoid build up and memory consumption as more and more fsm uses raycasting.
			lastRaycastHit2DInfoLUT.Clear();
		}
	} 


	static public bool isAvailable()
	{
		return fsmProxy!=null;
	}
	

	static public void ForwardEventToGameObject(GameObject target,string eventName)
	{

		// set the target to be this gameObject.
		goTarget.GameObject.Value = target;

		// send the event to this gameObject
		FsmEventTarget eventTarget = new FsmEventTarget();
		eventTarget.target = FsmEventTarget.EventTarget.GameObject;
		eventTarget.gameObject = goTarget;

		// create the event
		FsmEvent fsmEvent = new FsmEvent(eventName);
		
		// send the event
		fsmProxy.Fsm.Event(eventTarget,fsmEvent.Name); // Doesn't work if we pass just the fsmEvent itself.

	}

	static public void ForwardCollisionToCurrentState(GameObject target,Collision2DType type, Collision2D CollisionInfo)
	{
		PlayMakerFSM[] _fsms = target.GetComponents<PlayMakerFSM>();
		foreach(PlayMakerFSM _fsm in _fsms)
		{
			
			FsmState _currentState = null;
			
			foreach(FsmState _state in _fsm.FsmStates)
			{
				if (_state.Name.Equals(_fsm.ActiveStateName))
				{
					_currentState = _state;
					break;
				}
			}
			
			if (_currentState!=null)
			{
				foreach(IFsmCollider2DStateAction _Action in _currentState.Actions )
				{
					if (type == Collision2DType.OnCollisionEnter2D)
					{
						_Action.DoCollisionEnter2D(CollisionInfo);
					}
				}
			}
			
		}
	}


}
