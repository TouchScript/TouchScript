using UnityEngine;
using System.Collections;

public class BackgroundCube : MonoBehaviour {

	public float DecaySpeed = 1f;
	public float RotationSpeed = 1f;
	
	private float startScale;
	private Vector3 axis;
	
	void Start () {
		startScale = transform.localScale.x;
		axis = Quaternion.Euler(Random.Range(0, 359), Random.Range(0, 359), Random.Range(0, 359)) * Vector3.forward;
	}
	
	void Update () {
		var scale = transform.localScale.x - DecaySpeed*Time.deltaTime*startScale;
		if (scale <= 0) {
			Destroy(gameObject);
			return;
		}
		
		transform.localScale = Vector3.one * scale;
		transform.RotateAroundLocal(axis, RotationSpeed * 360 * Time.deltaTime);
	}
}
