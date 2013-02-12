using System;
using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class Cubes : MonoBehaviour {

    private enum CubesState
    {
        Idle,
        Rotating
    }

    public float AutoRotationSpeed = 10f;
    public float RotationSpeed = 20f;

    private CubesState State;
    private Quaternion targetRotation;

    public void Rotate(Vector3 axis)
    {
        if (State != CubesState.Idle) return;

        State = CubesState.Rotating;
        targetRotation = Quaternion.AngleAxis(90, axis) * transform.localRotation;
    }

    private void Start()
    {
        targetRotation = transform.localRotation;

        GetComponent<RotateGesture>().StateChanged += onRotateStateChanged;
    }

    private void Update()
    {
        if (State == CubesState.Rotating)
        {
            var fraction = AutoRotationSpeed * Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, fraction);
            if (Quaternion.Angle(transform.localRotation, targetRotation) < .1)
            {
                transform.localRotation = targetRotation;
                State = CubesState.Idle;
            }
        } else
        {
            var fraction = RotationSpeed * Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, fraction);
        }
    }

    private void onRotateStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        if (State != CubesState.Idle) return;
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:
                var gesture = (RotateGesture)sender;

                if (Math.Abs(gesture.LocalDeltaRotation) > 0.01)
                {
                    targetRotation = Quaternion.AngleAxis(gesture.LocalDeltaRotation, gesture.WorldTransformPlane.normal) * targetRotation;
                }
                break;
        }
    }

}
