using System;
using TouchScript.Events;
using TouchScript.Gestures;
using UnityEngine;
using System.Collections;

public class Side : MonoBehaviour
{

    public float Speed = 10f;

    private Vector3 startPosition;
    private Vector3 targetPosition; 

    private void Start()
    {
        startPosition = targetPosition = transform.localPosition;
        GetComponent<PanGesture>().StateChanged += OnStateChanged;
    }

    private void Update()
    {
        //Debug.Log(targetY);
        var fraction = Speed * Time.deltaTime;
        //transform.localPosition = targetPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, fraction);
    }

    private void OnStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:
                var target = sender as PanGesture;
                Debug.DrawRay(transform.position, target.WorldTransformPlane.normal);
                Debug.DrawRay(transform.position, target.WorldDeltaPosition.normalized);
                
                var local = new Vector3(0, transform.InverseTransformDirection(target.WorldDeltaPosition).y, 0);
                targetPosition += transform.parent.InverseTransformDirection(transform.TransformDirection(local));

                if (transform.InverseTransformDirection(transform.parent.TransformDirection(targetPosition - startPosition)).y < 0) targetPosition = startPosition;
                break;
        }
    }

}