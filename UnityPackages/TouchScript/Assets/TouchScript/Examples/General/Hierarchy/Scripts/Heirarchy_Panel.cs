using System;
using TouchScript.Gestures;
using UnityEngine;

public class Heirarchy_Panel : MonoBehaviour
{

    public float MaxHeight = 2;
    public float JumpSpeed = 5;

    private bool selected = false;
    private float startHeight;

    private void Start()
    {
        startHeight = transform.localPosition.y;

        GetComponent<PressGesture>().Pressed += pressedHandler;
        GetComponent<ReleaseGesture>().Released += releasedHandler;
    }

    private void Update()
    {
        var targetY = startHeight;
        if (selected) targetY = startHeight + MaxHeight;
        var newPosition = transform.localPosition;
        newPosition.y = Mathf.Lerp(transform.localPosition.y, targetY, Time.deltaTime * JumpSpeed);
        transform.localPosition = newPosition;
    }

    private void releasedHandler(object sender, EventArgs eventArgs)
    {
        selected = false;
    }

    private void pressedHandler(object sender, EventArgs eventArgs)
    {
        selected = true;
    }
}