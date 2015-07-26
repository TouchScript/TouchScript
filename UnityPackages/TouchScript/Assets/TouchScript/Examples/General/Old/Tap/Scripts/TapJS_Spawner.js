#pragma strict
import TouchScript.Hit;

var CubePrefab:Transform;
var Container:Transform;
var Scale:float = .5f;

function OnTap(sender:Gesture):void
{
    var hit:ITouchHit;
    sender.GetTargetHitResult(hit);
    var hit3d = hit as ITouchHit3D;
    if (hit3d == null) return;

    var color = new Color(Random.value, Random.value, Random.value);
    var cube = Instantiate(CubePrefab) as Transform;
    cube.parent = Container;
    cube.name = "Cube";
    cube.localScale = Vector3.one*Scale*cube.localScale.x;
    cube.position = hit3d.Point + hit3d.Normal*.5;
    cube.GetComponent.<Renderer>().material.color = color;
}