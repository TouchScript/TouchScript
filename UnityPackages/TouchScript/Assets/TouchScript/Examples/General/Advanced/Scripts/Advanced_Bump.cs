using TouchScript.Gestures;
using UnityEngine;

public class Advanced_Bump : MonoBehaviour
{
    private Vector3 startScale;

    private void Awake()
    {
        startScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged += pressStateChangeHandler;
        if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged += releaseStateChangedHandler;
    }

    private void OnDisable()
    {
        if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged -= pressStateChangeHandler;
        if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged -= releaseStateChangedHandler;
    }

    private void releaseStateChangedHandler(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
            transform.localScale = startScale;
    }

    private void pressStateChangeHandler(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
            transform.localScale = startScale*.7f;
    }
}