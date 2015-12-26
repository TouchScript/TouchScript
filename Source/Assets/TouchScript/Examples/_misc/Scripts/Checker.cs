/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Behaviors;

namespace TouchScript.Examples
{
    public class Checker : MonoBehaviour
    {
        private TransformGesture gesture;
        private Transformer transformer;
        private Rigidbody rb;

        private void OnEnable()
        {
            gesture = GetComponent<TransformGesture>();
            transformer = GetComponent<Transformer>();
            rb = GetComponent<Rigidbody>();

            transformer.enabled = false;
            rb.isKinematic = false;
            gesture.TransformStarted += transformStartedHandler;
            gesture.TransformCompleted += transformCompletedHandler;
        }

        private void OnDisable()
        {
            gesture.TransformStarted -= transformStartedHandler;
            gesture.TransformCompleted -= transformCompletedHandler;
        }

        private void transformStartedHandler(object sender, EventArgs e)
        {
            rb.isKinematic = true;
            transformer.enabled = true;
        }

        private void transformCompletedHandler(object sender, EventArgs e)
        {
            transformer.enabled = false;
            rb.isKinematic = false;
            rb.WakeUp();
        }
    }
}