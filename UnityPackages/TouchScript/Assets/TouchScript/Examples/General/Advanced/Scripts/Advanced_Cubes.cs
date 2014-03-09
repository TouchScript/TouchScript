using TouchScript.Gestures;
using UnityEngine;

public class Advanced_Cubes : MonoBehaviour
{
    private enum CubesState
    {
        Idle,
        Rotating
    }

    public float AutoRotationSpeed = 10f;
    public float RotationSpeed = 20f;

    private CubesState state;
    private Quaternion targetRotation;

    public void Rotate(Vector3 axis)
    {
        if (state != CubesState.Idle) return;

        state = CubesState.Rotating;
        targetRotation = Quaternion.AngleAxis(90, axis)*transform.localRotation;
    }

    private void Awake()
    {
        targetRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        GetComponent<RotateGesture>().StateChanged += rotateStateChangedHandler;
    }

    private void OnDisable()
    {
        GetComponent<RotateGesture>().StateChanged -= rotateStateChangedHandler;
    }

    private void Update()
    {
        if (state == CubesState.Rotating)
        {
            var fraction = AutoRotationSpeed*Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, fraction);
            if (Quaternion.Angle(transform.localRotation, targetRotation) < .1)
            {
                transform.localRotation = targetRotation;
                state = CubesState.Idle;
            }
        } else
        {
            var fraction = RotationSpeed*Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, fraction);
        }
    }

    private void rotateStateChangedHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (state != CubesState.Idle) return;
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:
                var gesture = (RotateGesture)sender;

                if (Mathf.Abs(gesture.DeltaRotation) > 0.01)
                {
                    targetRotation = Quaternion.AngleAxis(gesture.DeltaRotation, gesture.RotationAxis) * targetRotation;
                }
                break;
        }
    }
}