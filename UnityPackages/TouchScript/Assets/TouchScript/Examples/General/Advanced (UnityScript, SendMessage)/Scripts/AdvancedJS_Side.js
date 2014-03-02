#pragma strict
import TouchScript.Gestures;

var Speed:float = 10f;

private var startPosition:Vector3;
private var targetPosition:Vector3;

function Awake()
{
    startPosition = targetPosition = transform.localPosition;
}

function Update()
{
	var fraction:float = Speed*Time.deltaTime;
    transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, fraction);
}

function OnPanned(gesture:PanGesture)
{
	var local = new Vector3(0, transform.InverseTransformDirection(gesture.WorldDeltaPosition).y, 0);
    targetPosition += transform.parent.InverseTransformDirection(transform.TransformDirection(local));

    if (transform.InverseTransformDirection(transform.parent.TransformDirection(targetPosition - startPosition)).y < 0) targetPosition = startPosition;
}