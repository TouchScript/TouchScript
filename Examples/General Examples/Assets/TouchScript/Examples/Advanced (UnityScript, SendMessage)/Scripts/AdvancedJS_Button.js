#pragma strict

function OnLongPressed()
{
	GameObject.Find("Big Cube").renderer.material.color = renderer.material.color;
}