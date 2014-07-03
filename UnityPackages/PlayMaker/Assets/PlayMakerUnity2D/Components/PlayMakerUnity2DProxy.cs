// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.
using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

/// <summary>
/// This component is needed on gameObjects that have Physics 2D Colliders with Fsms listening to "TRIGGER 2D XXX" and "COLLISION 2D XXX" global events.
/// </summary>
public class PlayMakerUnity2DProxy : MonoBehaviour {

	public bool debug = false;

	// Flags to avoid unnecessary processing, if no fsm implements a particular Collider event, nothing will be processed.
	[HideInInspector]
	public bool HandleCollisionEnter2D = false;

	[HideInInspector]
	public bool HandleCollisionExit2D = false;

	[HideInInspector]
	public bool HandleCollisionStay2D = false;

	[HideInInspector]
	public bool HandleTriggerEnter2D = false;

	[HideInInspector]
	public bool HandleTriggerExit2D = false;

	[HideInInspector]
	public bool HandleTriggerStay2D = false;
	

	[HideInInspector]
	public Collision2D lastCollision2DInfo;
	[HideInInspector]
	public Collider2D lastTrigger2DInfo;

	
	#region DELEGATES

	// COLLISION ENTER
	public delegate void OnCollisionEnter2dDelegate(Collision2D collisionInfo);
	private OnCollisionEnter2dDelegate OnCollisionEnter2dDelegates;
	
	public void AddOnCollisionEnter2dDelegate(OnCollisionEnter2dDelegate del){ this.OnCollisionEnter2dDelegates += del; }
	public void RemoveOnCollisionEnter2dDelegate(OnCollisionEnter2dDelegate del){ this.OnCollisionEnter2dDelegates -= del; }

	// COLLISION STAY
	public delegate void OnCollisionStay2dDelegate(Collision2D collisionInfo);
	private OnCollisionStay2dDelegate OnCollisionStay2dDelegates;
	
	public void AddOnCollisionStay2dDelegate(OnCollisionStay2dDelegate del){ this.OnCollisionStay2dDelegates += del; }
	public void RemoveOnCollisionStay2dDelegate(OnCollisionStay2dDelegate del){ this.OnCollisionStay2dDelegates -= del; }

	// COLLISION EXIT
	public delegate void OnCollisionExit2dDelegate(Collision2D collisionInfo);
	private OnCollisionExit2dDelegate OnCollisionExit2dDelegates;
	
	public void AddOnCollisionExit2dDelegate(OnCollisionExit2dDelegate del){ this.OnCollisionExit2dDelegates += del; }
	public void RemoveOnCollisionExit2dDelegate(OnCollisionExit2dDelegate del){ this.OnCollisionExit2dDelegates -= del; }


	// TRIGGER ENTER
	public delegate void OnTriggerEnter2dDelegate(Collider2D collisionInfo);
	private OnTriggerEnter2dDelegate OnTriggerEnter2dDelegates;
	
	public void AddOnTriggerEnter2dDelegate(OnTriggerEnter2dDelegate del){ this.OnTriggerEnter2dDelegates += del; }
	public void RemoveOnTriggerEnter2dDelegate(OnTriggerEnter2dDelegate del){ this.OnTriggerEnter2dDelegates -= del; }
	
	// TRIGGER STAY
	public delegate void OnTriggerStay2dDelegate(Collider2D collisionInfo);
	private OnTriggerStay2dDelegate OnTriggerStay2dDelegates;
	
	public void AddOnTriggerStay2dDelegate(OnTriggerStay2dDelegate del){ this.OnTriggerStay2dDelegates += del; }
	public void RemoveOnTriggerStay2dDelegate(OnTriggerStay2dDelegate del){ this.OnTriggerStay2dDelegates -= del; }
	
	// TRIGGER EXIT
	public delegate void OnTriggerExit2dDelegate(Collider2D collisionInfo);
	private OnTriggerExit2dDelegate OnTriggerExit2dDelegates;
	
	public void AddOnTriggerExit2dDelegate(OnTriggerExit2dDelegate del){ this.OnTriggerExit2dDelegates += del; }
	public void RemoveOnTriggerExit2dDelegate(OnTriggerExit2dDelegate del){ this.OnTriggerExit2dDelegates -= del; }

	#endregion

	[ContextMenu("Help")]
	public void help ()
	{
		Application.OpenURL ("https://hutonggames.fogbugz.com/default.asp?W1150");
	}

	
	public void Start()
	{
		if ( ! PlayMakerUnity2d.isAvailable() )
		{
			Debug.LogError("PlayMakerUnity2DProxy requires the 'PlayMaker Unity 2D' Prefab in the Scene.\n" +
				"Use the menu 'PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2D to Scene' to correct the situation",this);
			this.enabled = false;
			return;
		}

		RefreshImplementation();
	}

	public void RefreshImplementation()
	{
		CheckGameObjectEventsImplementation();
	}


	#region Physics 2D Messages

	void OnCollisionEnter2D(Collision2D coll)
	{
		//if (debug) Debug.Log("OnCollisionEnter2D "+HandleCollisionEnter2D,this.gameObject);

		if (HandleCollisionEnter2D)
		{
			lastCollision2DInfo = coll;

			PlayMakerUnity2d.ForwardEventToGameObject(this.gameObject,PlayMakerUnity2d.OnCollisionEnter2DEvent);
		}

		if (this.OnCollisionEnter2dDelegates!=null) this.OnCollisionEnter2dDelegates(coll);
	}


	void OnCollisionStay2D(Collision2D coll)
	{
		if (debug) Debug.Log("OnCollisionStay2D "+HandleCollisionStay2D,this.gameObject);

		if (HandleCollisionStay2D)
		{
			lastCollision2DInfo = coll;

			PlayMakerUnity2d.ForwardEventToGameObject(this.gameObject,PlayMakerUnity2d.OnCollisionStay2DEvent);
		}

		if (this.OnCollisionStay2dDelegates!=null) this.OnCollisionStay2dDelegates(coll);
	}

	void OnCollisionExit2D(Collision2D coll)
	{
		if (debug) Debug.Log("OnCollisionExit2D "+HandleCollisionExit2D,this.gameObject);
		
		if (HandleCollisionExit2D)
		{
			lastCollision2DInfo = coll;
			
			PlayMakerUnity2d.ForwardEventToGameObject(this.gameObject,PlayMakerUnity2d.OnCollisionExit2DEvent);
		}
		
		if (this.OnCollisionExit2dDelegates!=null) this.OnCollisionExit2dDelegates(coll);
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (debug) Debug.Log(this.gameObject.name+" OnTriggerEnter2D "+coll.gameObject.name,this.gameObject);

		if (HandleTriggerEnter2D)
		{
			lastTrigger2DInfo = coll;

			PlayMakerUnity2d.ForwardEventToGameObject(this.gameObject,PlayMakerUnity2d.OnTriggerEnter2DEvent);
		}

		if (this.OnTriggerEnter2dDelegates!=null) this.OnTriggerEnter2dDelegates(coll);
	}
	
	void OnTriggerStay2D(Collider2D coll)
	{
		if (debug) Debug.Log(this.gameObject.name+" OnTriggerStay2D "+coll.gameObject.name,this.gameObject);

		if (HandleTriggerStay2D)
		{
			lastTrigger2DInfo = coll;

			PlayMakerUnity2d.ForwardEventToGameObject(this.gameObject,PlayMakerUnity2d.OnTriggerStay2DEvent);
		}

		if (this.OnTriggerStay2dDelegates!=null) this.OnTriggerStay2dDelegates(coll);
	}


	void OnTriggerExit2D(Collider2D coll)
	{
		if (debug) Debug.Log(this.gameObject.name+" OnTriggerExit2D "+coll.gameObject.name,this.gameObject);
		
		if (HandleTriggerExit2D)
		{
			lastTrigger2DInfo = coll;
			
			PlayMakerUnity2d.ForwardEventToGameObject(this.gameObject,PlayMakerUnity2d.OnTriggerExit2DEvent);
		}

		if (this.OnTriggerExit2dDelegates!=null) this.OnTriggerExit2dDelegates(coll);
	}
	
	#endregion


	#region Internal

	void CheckGameObjectEventsImplementation()
	{
		PlayMakerFSM[] fsms = this.GetComponents<PlayMakerFSM>();
		foreach(PlayMakerFSM fsm in fsms)
		{
			CheckFsmEventsImplementation(fsm);
		}
	}

	void CheckFsmEventsImplementation(PlayMakerFSM fsm)
	{
		foreach(FsmTransition _transition in fsm.FsmGlobalTransitions)
		{
			CheckTransition(_transition.EventName);
		}
		
		foreach(FsmState _state in fsm.FsmStates)
		{
			
			foreach(FsmTransition _transition in _state.Transitions)
			{
				CheckTransition(_transition.EventName);
			}
		}
	}

	void CheckTransition(string transitionName)
	{
		if (transitionName.Equals(PlayMakerUnity2d.OnCollisionEnter2DEvent))
		{
			HandleCollisionEnter2D = true;
		}
		if (transitionName.Equals(PlayMakerUnity2d.OnCollisionExit2DEvent))
		{
			HandleCollisionExit2D = true;
		}
		if (transitionName.Equals(PlayMakerUnity2d.OnCollisionStay2DEvent))
		{
			HandleCollisionStay2D = true;
		}
		if (transitionName.Equals(PlayMakerUnity2d.OnTriggerEnter2DEvent))
		{
			HandleTriggerEnter2D = true;
		}
		if (transitionName.Equals(PlayMakerUnity2d.OnTriggerExit2DEvent))
		{
			HandleTriggerExit2D = true;
		}
		if (transitionName.Equals(PlayMakerUnity2d.OnTriggerStay2DEvent))
		{
			HandleTriggerStay2D = true;
		}
	}

	#endregion
	
}
