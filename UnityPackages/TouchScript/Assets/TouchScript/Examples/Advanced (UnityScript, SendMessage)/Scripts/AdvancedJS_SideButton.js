#pragma strict

private var startY:float;

function Awake()
{
	startY = transform.localPosition.y;
}

function OnReleased()
{
	transform.localPosition = Vector3.up*startY;
    var cubes:AdvancedJS_Cubes = GameObject.Find("Cubes").GetComponent(AdvancedJS_Cubes);
    cubes.Rotate(transform.up);
}

function OnPressed()
{
	transform.localPosition = Vector3.up*(startY - .08f);
}