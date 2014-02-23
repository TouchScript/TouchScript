using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;

public class FlickExample_Rotator : MonoBehaviour
{

    public Rigidbody Target;

    private Quaternion targetRotation;
    private Vector3 speed;

    private void OnEnable()
    {
        GetComponent<FlickGesture>().StateChanged += flickStateChangedHandler;
        GetComponent<PressGesture>().StateChanged += pressStateChangedHandler;
    }

    private void OnDisable()
    {
        GetComponent<FlickGesture>().StateChanged -= flickStateChangedHandler;
        GetComponent<PressGesture>().StateChanged -= pressStateChangedHandler;
    }

    private void FixedUpdate()
    {
        if (speed != Vector3.zero)
        {
            Target.AddTorque(speed);
            speed = Vector2.zero;
        }
    }

    private void flickStateChangedHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            var spd = ((sender as FlickGesture).ScreenFlickVector/(sender as FlickGesture).ScreenFlickTime);
            speed = new Vector3(spd.y, -spd.x, 0);
        }
    }

    private void pressStateChangedHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            Target.angularVelocity = Vector3.zero;
        }
    }

}