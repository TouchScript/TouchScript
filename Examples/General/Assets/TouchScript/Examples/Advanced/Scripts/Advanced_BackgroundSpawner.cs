using TouchScript.Gestures.Simple;
using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

[RequireComponent(typeof(MetaGesture))]
public class Advanced_BackgroundSpawner : MonoBehaviour
{
    public GameObject Prefab;
    public float SpawnDelay = 0.05f;

    private bool shouldSpawn = false;
    private Vector3 spawnCoords;

    private void Start()
    {
        var metaGesture = GetComponent<MetaGesture>();
        metaGesture.TouchPointBegan += touchEventHandler;
        metaGesture.TouchPointMoved += touchEventHandler;
        metaGesture.TouchPointEnded += touchEndedHandler;
        metaGesture.TouchPointCancelled += touchEndedHandler;

        StartCoroutine(doSpawn());
    }

    private IEnumerator doSpawn()
    {
        while (true)
        {
            if (shouldSpawn)
            {
                var instance = Instantiate(Prefab, spawnCoords, Quaternion.identity) as GameObject;
                instance.transform.parent = transform;
            }
            yield return new WaitForSeconds(SpawnDelay);
        }
    }

    private void touchEventHandler(object sender, MetaGestureEventArgs e)
    {
        shouldSpawn = true;
        spawnCoords = camera.ScreenToWorldPoint(new Vector3(e.TouchPoint.Position.x, e.TouchPoint.Position.y, camera.farClipPlane));
    }

    private void touchEndedHandler(object sender, MetaGestureEventArgs e)
    {
        if (((MetaGesture)sender).State == Gesture.GestureState.Ended) shouldSpawn = false;
    }
}