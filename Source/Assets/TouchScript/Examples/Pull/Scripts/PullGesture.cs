/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Pointers;
using System;
using TouchScript.Layers;

// Let's put our gesture into a namespace so it wouldn't clash with other classes in our project
namespace TouchScript.Tutorial
{
    // The class must inherit from Gesture
    public class PullGesture : Gesture
    {
        public event EventHandler<EventArgs> Pressed
        {
            add { pressedInvoker += value; }
            remove { pressedInvoker -= value; }
        }

        public event EventHandler<EventArgs> Pulled
        {
            add { pulledInvoker += value; }
            remove { pulledInvoker -= value; }
        }

        public event EventHandler<EventArgs> Released
        {
            add { releasedInvoker += value; }
            remove { releasedInvoker -= value; }
        }

        public Vector3 StartPosition
        {
            get
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                    case GestureState.Ended:
                        return startPosition;
                    default:
                        return transform.position;
                }
            }
        }

        public Vector3 Position
        {
            get
            {
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                    case GestureState.Ended:
                        return projection.ProjectTo(primaryPointer.Position, plane);
                    default:
                        return transform.position;
                }
            }
        }

        public Vector3 Force
        {
            get { return StartPosition - Position; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> pressedInvoker, pulledInvoker, releasedInvoker;

        // The only pointer we are interested in
        private Pointer primaryPointer;

        // Layer projection parameters
        private ProjectionParams projection;

        // 3D plane to project to
        private Plane plane;

        // The world coordinates of the point where the gesture started
        private Vector3 startPosition;

        // Pointers pressed this frame
        protected override void pointersPressed(IList<Pointer> pointers)
        {
            if (State == GestureState.Idle)
            {
                primaryPointer = pointers[0];
                projection = primaryPointer.GetPressData().Layer.GetProjectionParams(primaryPointer);
                plane = new Plane(Vector3.up, transform.position);
                startPosition = projection.ProjectTo(primaryPointer.Position, plane);

                // Start the gesture
                setState(GestureState.Began);
            }
        }

        // Pointers updated this frame
        protected override void pointersUpdated(IList<Pointer> pointers)
        {
            foreach (var p in pointers)
            {
                if (p.Id == primaryPointer.Id)
                {
                    // If the pointer we are interested in moved, change the state
                    setState(GestureState.Changed);
                    return;
                }
            }
        }

        // Pointers released this frame
        protected override void pointersReleased(IList<Pointer> pointers)
        {
            foreach (var p in pointers)
            {
                if (p.Id == primaryPointer.Id)
                {
                    // If the pointer we are interested was released, end the gesture
                    setState(GestureState.Ended);
                    return;
                }
            }
        }

        // Pointers cancelled this frame
        protected override void pointersCancelled(IList<Pointer> pointers)
        {
            foreach (var p in pointers)
            {
                if (p.Id == primaryPointer.Id)
                {
                    // If the pointer we are interested was cancelled, cancel the gesture
                    setState(GestureState.Cancelled);
                    return;
                }
            }
        }

        // Called when the gesture transitions to Began state
        protected override void onBegan()
        {
            if (pressedInvoker != null) pressedInvoker(this, EventArgs.Empty);
        }

        // Called when the gesture transitions to Ended or Recognized states
        protected override void onRecognized()
        {
            if (releasedInvoker != null) releasedInvoker(this, EventArgs.Empty);
        }

        // Called when the gesture transitions to Changed state
        protected override void onChanged()
        {
            if (pulledInvoker != null) pulledInvoker(this, EventArgs.Empty);
//			Debug.LogFormat("Start position: {0}, current position: {1}, force: {2}", StartPosition, Position, Force.magnitude);
        }

        // This method is called when gesture is reset when recognized or failed
        protected override void reset()
        {
            base.reset();
            primaryPointer = null;
        }
    }
}