using UnityEngine;
using System.Collections;

/// <exclude />
public class ShowMe : MonoBehaviour 
{
	IEnumerator Start () 
	{
		var canvas = GetComponent<Canvas>();
		canvas.enabled = false;
		yield return new WaitForSeconds(.5f);
		canvas.enabled = true;
	}
}
