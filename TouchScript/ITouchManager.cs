/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Devices.Display;
using TouchScript.Events;
using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    public interface ITouchManager
    {
        /// <summary>
        /// Occurs when a new frame is started before all other events.
        /// </summary>
        event EventHandler FrameStarted;

        /// <summary>
        /// Occurs when a frame is finished. After all other events.
        /// </summary>
        event EventHandler FrameFinished;

        /// <summary>
        /// Occurs when new touch points are added.
        /// </summary>
        event EventHandler<TouchEventArgs> TouchesBegan;

        /// <summary>
        /// Occurs when touch points are updated.
        /// </summary>
        event EventHandler<TouchEventArgs> TouchesMoved;

        /// <summary>
        /// Occurs when touch points are removed.
        /// </summary>
        event EventHandler<TouchEventArgs> TouchesEnded;

        /// <summary>
        /// Occurs when touch points are cancelled.
        /// </summary>
        event EventHandler<TouchEventArgs> TouchesCancelled;

        IDisplayDevice DisplayDevice { get; set; }

        /// <summary>
        /// Current DPI.
        /// </summary>
        float DPI { get; }

        /// <summary>
        /// List of touch layers.
        /// </summary>
        IList<TouchLayer> Layers { get; }

        /// <summary>
        /// Pixels in a cm with current DPI.
        /// </summary>
        float DotsPerCentimeter { get; }

        /// <summary>
        /// Number of active touches.
        /// </summary>
        int TouchPointsCount { get; }

        /// <summary>
        /// List of active touches.
        /// </summary>
        IList<ITouchPoint> TouchPoints { get; }

        /// <summary>
        /// Adds a layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>True if layer was added.</returns>
        bool AddLayer(TouchLayer layer);

        bool AddLayer(TouchLayer layer, int index);

        /// <summary>
        /// Removes a layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>True if layer was removed.</returns>
        bool RemoveLayer(TouchLayer layer);

        /// <summary>
        /// Swaps layers in sorted array.
        /// </summary>
        /// <param name="at">Layer index 1.</param>
        /// <param name="to">Layer index 2</param>
        void ChangeLayerIndex(int at, int to);

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="position">Touch screen position.</param>
        /// <returns>Object's transform which has been hit or null otherwise.</returns>
        Transform GetHitTarget(Vector2 position);

        /// <summary>
        /// Checks if the touch has hit something.
        /// </summary>
        /// <param name="position">Touch point screen position.</param>
        /// <param name="hit">Output RaycastHit.</param>
        /// <param name="layer">Output touch layer which was hit.</param>
        /// <returns>True if something was hit.</returns>
        bool GetHitTarget(Vector2 position, out ITouchHit hit, out TouchLayer layer);

        /// <summary>
        /// Registers a touch.
        /// </summary>
        /// <param name="position">Touch position.</param>
        /// <returns>Internal id of the new touch.</returns>
        int BeginTouch(Vector2 position);

        /// <summary>
        /// Moves a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        /// <param name="position">New position.</param>
        void MoveTouch(int id, Vector2 position);

        /// <summary>
        /// Ends a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        void EndTouch(int id);

        /// <summary>
        /// Cancels a touch.
        /// </summary>
        /// <param name="id">Internal touch id.</param>
        void CancelTouch(int id);
    }
}