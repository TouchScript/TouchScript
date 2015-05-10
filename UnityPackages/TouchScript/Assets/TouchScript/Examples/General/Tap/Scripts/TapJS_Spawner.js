#pragma strict
import TouchScript.Hit;

var CubePrefab:Transform;
var Container:Transform;
var Scale:float = .5f;

function OnTap(sender:Gesture):void
{
    var hit:TouchHit;
    sender.GetTargetHitResult(hit);

    var color = new Color(Random.value, Random.value, Random.value);
    var cube = Instantiate(CubePrefab) as Transform;
    cube.parent = Container;
    cube.name = "Cube";
    cube.localScale = Vector3.one*Scale*cube.localScale.x;
    cube.position = hit.Point + hit.Normal*.5;
    cube.GetComponent.<Renderer>().material.color = color;
}