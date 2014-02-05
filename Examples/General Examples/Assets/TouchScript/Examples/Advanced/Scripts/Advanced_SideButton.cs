using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class Advanced_SideButton : MonoBehaviour
{
    private float startY;

    private void Awake()
    {
        startY = transform.localPosition.y;
    }

    private void OnEnable()
    {
        if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged += pressStateChangeHandler;
        if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged += releaseStateChangeHandler;
    }

    private void OnDisable()
    {
        if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged -= pressStateChangeHandler;
        if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged -= releaseStateChangeHandler;
    }

    private void releaseStateChangeHandler(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
        {
            transform.localPosition = Vector3.up*startY;
            var cubes = GameObject.Find("Cubes").GetComponent<Advanced_Cubes>();
            cubes.Rotate(transform.up);
        }
    }

    private void pressStateChangeHandler(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
        {
            transform.localPosition = Vector3.up*(startY - .08f);
        }
    }
}