using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

public class Breaker : MonoBehaviour {
	public Transform CubePrefab;
	public float Power = 10.0f;
	// Use this for initialization
	void Start () {
		GetComponent<TapGesture>().StateChanged += HandleStateChanged;;
	}

	void HandleStateChanged (object sender, TouchScript.Events.GestureStateChangeEventArgs e)
	{
		Vector3[] directions = {
			new Vector3(1, -1, 1),
			new Vector3(-1, -1, 1),
			new Vector3(-1, -1, -1),
			new Vector3(1, -1, -1),
			new Vector3(1, 1, 1),
			new Vector3(-1, 1, 1),
			new Vector3(-1, 1, -1),
			new Vector3(1, 1, -1)
		};
		if (e.State == Gesture.GestureState.Recognized) {			
			if (transform.localScale.x > 0.05f) {
				Color color = new Color(Random.value, Random.value, Random.value);
				for (int i = 0; i < 8; i++) {
					var c = Instantiate(CubePrefab) as Transform;
					c.localScale = 0.5f * transform.localScale;
					c.position = transform.TransformPoint(c.localScale.x / 10.0f * directions[i]);					
					c.rigidbody.velocity = Power * Random.insideUnitSphere;
					c.renderer.material.color = color;				
				}
			}			
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
