using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class Button : MonoBehaviour
{
    private void Start()
    {
        if (GetComponent<LongPressGesture>() != null) GetComponent<LongPressGesture>().StateChanged += onPress;
    }

    private void onPress(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
            GameObject.Find("Big Cube").renderer.material.color = renderer.material.color;
    }

}