using System;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TouchScript.Examples.Tap 
{
	public class Spawn : MonoBehaviour 
	{
		public Transform CubePrefab;
		public Transform Container;
		public float Scale = .5f;
		
		private void OnEnable()
		{
			GetComponent<TapGesture>().Tapped += tappedHandler;
		}
		
		private void OnDisable()
		{
			GetComponent<TapGesture>().Tapped -= tappedHandler;
		}
		
		private void tappedHandler(object sender, EventArgs e)
		{
			var gesture = sender as TapGesture;
			ITouchHit hit;
			gesture.GetTargetHitResult(out hit);
			var hit3d = hit as ITouchHit3D;
			if (hit3d == null) return;
			
			var cube = Instantiate(CubePrefab) as Transform;
			cube.parent = Container;
			cube.name = "Cube";
			cube.localScale = Vector3.one*Scale*cube.localScale.x;
			cube.position = hit3d.Point + hit3d.Normal*.5f;
		}
	}
}