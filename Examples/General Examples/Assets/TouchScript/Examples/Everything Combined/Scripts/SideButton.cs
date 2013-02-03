using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class SideButton : MonoBehaviour
{

    private float startY;

    private void Start()
    {
        startY = transform.localPosition.y;

        if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged += onPress;
        if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged += onRelease;
    }

    private void onRelease(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
        {
            transform.localPosition = Vector3.up*startY;
            var cubes = GameObject.Find("Cubes").GetComponent<Cubes>();
            cubes.Rotate(transform.up);
        }
    }

    private void onPress(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs)
    {
        if (gestureStateChangeEventArgs.State == Gesture.GestureState.Recognized)
        {
            transform.localPosition = Vector3.up*(startY - .08f);
        }
    }
}