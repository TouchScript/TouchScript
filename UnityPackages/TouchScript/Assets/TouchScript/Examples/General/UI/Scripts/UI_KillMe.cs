using UnityEngine;
using System.Collections;

public class UI_KillMe : MonoBehaviour 
{

	public void KillMe()
	{
		Destroy(transform.parent.gameObject);
	}

}
