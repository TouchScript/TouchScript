using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class Bump : MonoBehaviour
{
    private Vector3 startScale;

    private void Start()
    {
        if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged += onPress;
        if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged += onRelease;

        startScale = transform.localScale;
    }

    private void onRelease(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
            transform.localScale = startScale;
    }

    private void onPress(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
            transform.localScale = startScale*.7f;
    }
}