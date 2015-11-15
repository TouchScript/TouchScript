using UnityEngine;
using System.Collections;

namespace TouchScript.Examples.Portal 
{
	public class Vortex : MonoBehaviour 
	{
		private void OnTriggerEnter(Collider other)
		{
			var planet = other.GetComponent<Planet>();
			if (planet == null) return;
			planet.Fall();
		}
	}
}