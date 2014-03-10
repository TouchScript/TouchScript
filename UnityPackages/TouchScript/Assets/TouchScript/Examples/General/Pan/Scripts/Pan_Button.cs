using TouchScript.Gestures;
using UnityEngine;

public class Pan_Button : MonoBehaviour
{

    private float startY;

    private void Awake()
    {
        startY = transform.localPosition.y;
    }

    private void OnEnable()
    {
        GetComponent<PressGesture>().StateChanged += pressStateChangeHandler;
        GetComponent<ReleaseGesture>().StateChanged += releaseStateChangeHandler;
    }

    private void OnDisable()
    {
        GetComponent<PressGesture>().StateChanged -= pressStateChangeHandler;
        GetComponent<ReleaseGesture>().StateChanged -= releaseStateChangeHandler;
    }

    private void releaseStateChangeHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            transform.localPosition = new Vector3(0, startY, 0);
        }
    }

    private void pressStateChangeHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            transform.localPosition = new Vector3(0, startY - transform.localScale.y * .9f, 0);
        }
    }

}
