using UnityEngine;
using System.Collections;

namespace TouchScript.Examples 
{
	public class KillMe : MonoBehaviour 
	{

		public float Delay = 1f;

		private IEnumerator Start() 
		{
			yield return new WaitForSeconds(Delay);
			Destroy(gameObject);
		}
	}
}