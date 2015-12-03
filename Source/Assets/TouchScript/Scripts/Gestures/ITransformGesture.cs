/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Gesture which performs some kind of transformation in 3d space, i.e. translation, rotation, scaling or a combination of these.
    /// </summary>
    public interface ITransformGesture
    {
        /// <summary>
        /// Occurs when gesture starts.
        /// </summary>
        event EventHandler<EventArgs> TransformStarted;

        /// <summary>
        /// Occurs when gesture data updates.
        /// </summary>
        event EventHandler<EventArgs> Transformed;

        /// <summary>
        /// Occurs when gesture finishes.
        /// </summary>
        event EventHandler<EventArgs> TransformCompleted;

        /// <summary>
        /// Applies gesture's transform for this frame to target Transform.
        /// </summary>
        /// <param name="target"> Object to transform. </param>
        void ApplyTransform(Transform target);
    }
}