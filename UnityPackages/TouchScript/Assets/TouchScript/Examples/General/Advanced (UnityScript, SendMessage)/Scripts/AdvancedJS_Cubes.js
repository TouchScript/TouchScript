#pragma strict
import TouchScript.Gestures;

private enum CubesState
{
	Idle,
	Rotating
}

var AutoRotationSpeed:float = 10;
var RotationSpeed:float = 20;

private var state:CubesState;
private var targetRotation:Quaternion;

function Rotate(axis:Vector3)
{
	if (state != CubesState.Idle) return;
	
	state = CubesState.Rotating;
    targetRotation = Quaternion.AngleAxis(90, axis)*transform.localRotation;
}

function Awake()
{
    targetRotation = transform.localRotation;
}

function Update()
{
    if (state == CubesState.Rotating)
    {
        var fraction:float = AutoRotationSpeed*Time.deltaTime;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, fraction);
        if (Quaternion.Angle(transform.localRotation, targetRotation) < .1)
        {
            transform.localRotation = targetRotation;
            state = CubesState.Idle;
        }
    } else
    {
        fraction = RotationSpeed*Time.deltaTime;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, fraction);
    }
}

function OnRotated(gesture:RotateGesture)
{
    if (Mathf.Abs(gesture.DeltaRotation) > 0.01)
    {
        targetRotation = Quaternion.AngleAxis(gesture.DeltaRotation, gesture.RotationAxis)*targetRotation;
    }
}