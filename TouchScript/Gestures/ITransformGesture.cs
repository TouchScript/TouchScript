/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Gestures
{
    public interface ITransformGesture
    {
        event EventHandler<EventArgs> TransformStarted;
        event EventHandler<EventArgs> Transformed;
        event EventHandler<EventArgs> TransformCompleted;
        void ApplyTransform(Transform target);
    }
}
