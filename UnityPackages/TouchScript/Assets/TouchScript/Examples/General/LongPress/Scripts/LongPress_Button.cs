using System;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;

public class LongPress_Button : MonoBehaviour
{
    public GameObject Plane;

    private Transform button, thebase;
    private float timeToPress;
    private Vector3 startScale, targetScale;

    private void Awake()
    {
        button = transform.FindChild("Button");
        thebase = transform.FindChild("Base");
        startScale = button.localScale;
        targetScale = thebase.localScale;
    }

    private void OnEnable()
    {
        timeToPress = GetComponent<LongPressGesture>().TimeToPress;

        GetComponent<PressGesture>().Pressed += pressedHandler;
        GetComponent<ReleaseGesture>().Released += releasedHandler;
        GetComponent<LongPressGesture>().StateChanged += longPressStateChangedHandler;
    }

    private void OnDisable()
    {
        GetComponent<PressGesture>().Pressed -= pressedHandler;
        GetComponent<ReleaseGesture>().Released -= releasedHandler;
        GetComponent<LongPressGesture>().StateChanged -= longPressStateChangedHandler;
    }

    private void press()
    {
        button.transform.localPosition = new Vector3(0, -button.transform.localScale.y*.4f, 0);
    }

    private void release()
    {
        button.transform.localPosition = new Vector3(0, 0, 0);
    }

    private void reset()
    {
        button.transform.localScale = startScale;
        StopCoroutine("grow");
    }

    private void changeColor()
    {
        if (Plane == null) return;

        Plane.GetComponent<Renderer>().material.color = button.GetComponent<Renderer>().sharedMaterial.color;
    }

    private IEnumerator grow()
    {
        while (true)
        {
            button.transform.localScale += (targetScale.x - startScale.x)/timeToPress*Time.unscaledDeltaTime*new Vector3(1, 0, 1);
            yield return null;
        }
    }

    private void longPressStateChangedHandler(object sender, GestureStateChangeEventArgs e)
    {
        switch (e.State)
        {
            case Gesture.GestureState.Recognized:
            case Gesture.GestureState.Failed:
            case Gesture.GestureState.Cancelled:
                reset();
                break;
        }

        if (e.State == Gesture.GestureState.Recognized)
        {
            changeColor();
        }
    }

    private void pressedHandler(object sender, EventArgs e)
    {
        press();
        StartCoroutine("grow");
    }

    private void releasedHandler(object sender, EventArgs e)
    {
        release();
    }
}