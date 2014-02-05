using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class Advanced_Button : MonoBehaviour
{
    private void OnEnable()
    {
        if (GetComponent<LongPressGesture>() != null) GetComponent<LongPressGesture>().StateChanged += pressStateChangeHandler;
    }

    private void OnDisable()
    {
        if (GetComponent<LongPressGesture>() != null) GetComponent<LongPressGesture>().StateChanged -= pressStateChangeHandler;
    }

    private void pressStateChangeHandler(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
        {
            GameObject.Find("Big Cube").renderer.material.color = renderer.material.color;
        }
    }
}