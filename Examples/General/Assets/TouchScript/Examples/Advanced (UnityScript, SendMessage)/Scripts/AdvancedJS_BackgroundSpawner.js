#pragma strict
import TouchScript;

var Prefab:GameObject;
var SpawnDelay:float = 0.05f;

private var shouldSpawn:boolean = false;
private var spawnCoords:Vector3;

function Start () {
	StartCoroutine(doSpawn());
}

private function doSpawn()
{
    while (true)
    {
        if (shouldSpawn)
        {
            var instance:GameObject = Instantiate(Prefab, spawnCoords, Quaternion.identity);
            instance.transform.parent = transform;
        }
        yield WaitForSeconds(SpawnDelay);
    }
}

function OnTouchPointBegan(touch:TouchPoint)
{
	spawn(touch);
}

function OnTouchPointMoved(touch:TouchPoint)
{
	spawn(touch);
}

function OnTouchPointEnded()
{
	shouldSpawn = false;
}

function OnTouchPointCancelled()
{
	shouldSpawn = false;
}

private function spawn(touch:TouchPoint)
{
	shouldSpawn = true;
    spawnCoords = camera.ScreenToWorldPoint(new Vector3(touch.Position.x, touch.Position.y, camera.farClipPlane));
}