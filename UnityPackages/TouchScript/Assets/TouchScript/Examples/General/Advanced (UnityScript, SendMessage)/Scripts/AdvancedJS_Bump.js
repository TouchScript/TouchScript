#pragma strict

private var startScale : Vector3;

function Awake()
{
	startScale = transform.localScale;
}

function OnReleased()
{
	transform.localScale = startScale;
}

function OnPressed()
{
	transform.localScale = startScale * .7;
}