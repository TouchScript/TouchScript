using UnityEngine;
using System.Collections;

namespace TouchScript.Examples.Input
{
	public class Ball : MonoBehaviour 
	{
		public float Speed = 1f;
		
		private void Update()
		{
			Speed *= 1.01f;
			transform.position += transform.forward*Speed*Time.deltaTime;
			if (Speed > 1000) Destroy(gameObject);
		}
	}
}