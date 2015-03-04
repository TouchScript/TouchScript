#pragma strict
import TouchScript.Gestures;

var Power:float = 10.0f;

private var directions:Vector3[] = [
    new Vector3(1, -1, 1),
    new Vector3(-1, -1, 1),
    new Vector3(-1, -1, -1),
    new Vector3(1, -1, -1),
    new Vector3(1, 1, 1),
    new Vector3(-1, 1, 1),
    new Vector3(-1, 1, -1),
    new Vector3(1, 1, -1)
];

function OnTap(sender:Gesture):void
{
    // if we are not too small
    if (transform.localScale.x > 0.05f)
    {
        var color:Color = new Color(Random.value, Random.value, Random.value);
        // break this cube into 8 parts
        for (var i:int = 0; i < 8; i++)
        {
            var obj:GameObject = Instantiate(gameObject) as GameObject;
            var cube = obj.transform;
            cube.parent = transform.parent;
            cube.name = "Cube";
            cube.localScale = 0.5f * transform.localScale;
            cube.position = transform.TransformPoint(directions[i] / 4);
            cube.GetComponent.<Rigidbody>().AddForce(Power * Random.insideUnitSphere, ForceMode.VelocityChange);
            cube.GetComponent.<Renderer>().material.color = color;
        }
        Destroy(gameObject);
    }
}
