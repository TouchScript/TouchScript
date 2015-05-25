using UnityEngine;
using System.Collections;

public class UI_AddImage : MonoBehaviour 
{

	public void AddImage()
	{
		var toClone = transform.GetChild(Random.Range(0, transform.childCount));
		var clone = Instantiate(toClone.gameObject) as GameObject;
		clone.transform.SetParent(transform);
		clone.transform.localScale = Vector3.one;
		clone.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
		clone.transform.localPosition = new Vector3(Random.Range(-500, 500), Random.Range(-500, 500), toClone.localPosition.z);
	}

}
