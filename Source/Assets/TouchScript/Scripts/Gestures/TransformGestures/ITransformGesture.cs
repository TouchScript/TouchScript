/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Gestures.TransformGestures
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
        /// Contains transform operations which happened this frame.
        /// </summary>
        TransformGesture.TransformType TransformMask { get; }

		/// <summary>
		/// Gets delta position between this frame and the last frame in world coordinates.
		/// This value is only available during <see cref="Transformed"/> or <see cref="Gesture.StateChanged"/> events.
		/// </summary>
		Vector3 DeltaPosition { get; }

		/// <summary>
		/// Gets delta rotation between this frame and last frame in degrees.
		/// This value is only available during <see cref="Transformed"/> or <see cref="Gesture.StateChanged"/> events.
		/// </summary>
		float DeltaRotation { get; }

		/// <summary>
		/// Contains local delta scale when gesture is recognized.
		/// Value is between 0 and +infinity, where 1 is no scale, 0.5 is scaled in half, 2 scaled twice.
		/// This value is only available during <see cref="Transformed"/> or <see cref="Gesture.StateChanged"/> events.
		/// </summary>
		float DeltaScale { get; }

        /// <summary>
        /// Gets rotation axis of the gesture in world coordinates.
        /// </summary>
        /// <value>Rotation axis of the gesture in world coordinates.</value>
        Vector3 RotationAxis { get; }
    }
}