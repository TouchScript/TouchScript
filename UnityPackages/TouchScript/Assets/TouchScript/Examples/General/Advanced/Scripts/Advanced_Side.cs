using TouchScript.Gestures;
using UnityEngine;

public class Advanced_Side : MonoBehaviour
{
    public float Speed = 10f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        startPosition = targetPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        GetComponent<PanGesture>().StateChanged += panStateChangeHandler;
    }

    private void OnDisable()
    {
        GetComponent<PanGesture>().StateChanged -= panStateChangeHandler;
    }

    private void Update()
    {
        var fraction = Speed*Time.deltaTime;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, fraction);
    }

    private void panStateChangeHandler(object sender, GestureStateChangeEventArgs e)
    {
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:
                var target = sender as PanGesture;

                var local = new Vector3(0, transform.InverseTransformDirection(target.WorldDeltaPosition).y, 0);
                targetPosition += transform.parent.InverseTransformDirection(transform.TransformDirection(local));

                if (transform.InverseTransformDirection(transform.parent.TransformDirection(targetPosition - startPosition)).y < 0) targetPosition = startPosition;
                break;
        }
    }
}