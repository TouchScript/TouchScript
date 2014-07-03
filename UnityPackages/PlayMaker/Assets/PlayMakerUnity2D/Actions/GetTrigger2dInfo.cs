// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Physics 2d")]
	[Tooltip("Gets info on the last Trigger 2d event and store in variables.  See Unity and PlayMaker docs on Unity 2D physics.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1152")]
	public class GetTrigger2dInfo : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the GameObject hit.")]
		public FsmGameObject gameObjectHit;

		[UIHint(UIHint.Variable)]
		[Tooltip("The number of separate shaped regions in the collider.")]
		public FsmInt shapeCount;
	
		[UIHint(UIHint.Variable)]
		[Tooltip("Useful for triggering different effects. Audio, particles...")]
		public FsmString physics2dMaterialName;
		
		public override void Reset()
		{
			gameObjectHit = null;
			shapeCount = null;
			physics2dMaterialName = null;
		}
		
		void StoreTriggerInfo()
		{
			PlayMakerUnity2DProxy _proxy = Fsm.GameObject.GetComponent<PlayMakerUnity2DProxy>();
			
			if (_proxy == null || _proxy.lastTrigger2DInfo == null)
			{
				return;
			}
			
			gameObjectHit.Value = _proxy.lastTrigger2DInfo.gameObject;
			shapeCount.Value = _proxy.lastTrigger2DInfo.shapeCount;
			physics2dMaterialName.Value = _proxy.lastTrigger2DInfo.sharedMaterial!=null?_proxy.lastTrigger2DInfo.sharedMaterial.name:"";
		}
		
		public override void OnEnter()
		{
			StoreTriggerInfo();
			
			Finish();
		}
	}
}