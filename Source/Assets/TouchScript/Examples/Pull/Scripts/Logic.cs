/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Tutorial
{
    public class Logic : MonoBehaviour
    {
        // Force multiplier
        public float ForceMultiplier = 100f;
        public LineRenderer Line;

        private PullGesture gesture;
        private Rigidbody body;

        private Vector3 forceToApply;
        private bool shouldApplyForce = false;

        private void OnEnable()
        {
            body = GetComponent<Rigidbody>();
            gesture = GetComponent<PullGesture>();

            Line.enabled = false;

            gesture.Pressed += pressedHandler;
            gesture.Pulled += pulledHandler;
            gesture.Released += releasedHandler;
            gesture.Cancelled += cancelledHandler;

            releaseObject();
        }

        private void OnDisable()
        {
            gesture.Pressed -= pressedHandler;
            gesture.Pulled -= pulledHandler;
            gesture.Released -= releasedHandler;
            gesture.Cancelled -= cancelledHandler;
        }

        private void FixedUpdate()
        {
            // Apply force in FixedUpdate to make physics happy
            if (shouldApplyForce)
            {
                body.AddForce(forceToApply);
                shouldApplyForce = false;
            }
        }

        // Switch to manual mode
        private void takeObject()
        {
            body.isKinematic = true;
            Line.enabled = true;
            updateLine();
        }

        // Switch to automatic mode
        private void releaseObject()
        {
            body.isKinematic = false;
            Line.enabled = false;
        }

        // Push the object when the gesture is ended
        private void pushObject()
        {
            forceToApply = ForceMultiplier * gesture.Force;
            shouldApplyForce = true;
        }

        // Update the line
        private void updateLine()
        {
            Line.SetPosition(0, gesture.StartPosition);
            Line.SetPosition(1, gesture.Position);
        }

        private void pressedHandler(object sender, System.EventArgs e)
        {
            takeObject();
        }

        private void pulledHandler(object sender, System.EventArgs e)
        {
            updateLine();
        }

        private void releasedHandler(object sender, System.EventArgs e)
        {
            releaseObject();
            pushObject();
        }

        private void cancelledHandler(object sender, System.EventArgs e)
        {
            releaseObject();
        }
    }
}