using TouchScript.Gestures.Simple;
using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

[RequireComponent(typeof(MetaGesture))]
public class BackgroundSpawner : MonoBehaviour
{
	
	public GameObject Prefab;
	public float SpawnDelay = 0.05f;
	
	private bool shouldSpawn = false;
	private Vector3 spawnCoords;
	
	private void Start()
	{
		var metaGesture = GetComponent<MetaGesture>();
		metaGesture.TouchPointBegan += OnTouchEvent;
		metaGesture.TouchPointMoved += OnTouchEvent;
		metaGesture.TouchPointEnded += OnTouchEnded;
		metaGesture.TouchPointCancelled += OnTouchEnded;
		
		StartCoroutine(doSpawn());
	}

	private IEnumerator doSpawn()
	{
		while (true)
		{
			if (shouldSpawn) 
			{
//				shouldSpawn = false;
				var instance = Instantiate(Prefab, spawnCoords, Quaternion.identity) as GameObject;
				instance.transform.parent = transform;
			}
			yield return new WaitForSeconds(SpawnDelay);
		}
	}
	
	private void OnTouchEvent(object sender, MetaGestureEventArgs e)
	{
		shouldSpawn = true;
		spawnCoords = camera.ScreenToWorldPoint(new Vector3(e.TouchPoint.Position.x, e.TouchPoint.Position.y, camera.far));
	}
	
	void OnTouchEnded (object sender, MetaGestureEventArgs e)
	{
		if (((MetaGesture)sender).State == Gesture.GestureState.Ended) shouldSpawn = false;
	}
	
}
