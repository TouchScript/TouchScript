/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Devices.Display;
using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// <para>Core manager of all touch input in <b>TouchScript</b>. It is responsible for assigning unique touch ids and keeping the list of active touches. Controls touch frames and dispatches touch events.</para>
    /// </summary>
    /// <remarks>
    /// <para>Every frame touch events are dispatched in this order:</para>
    /// <list type="number">
    /// <item><description>FrameStarted</description></item>
    /// <item><description>TouchesBegan</description></item>
    /// <item><description>TouchesMoved</description></item>
    /// <item><description>TouchesEnded</description></item>
    /// <item><description>TouchesCancelled</description></item>
    /// <item><description>FrameFinished</description></item>
    /// </list>
    /// <para>FrameStarted and FrameFinished events mark the start and the end of current touch frame and allow to implement specific logic at these moments.</para>
    /// <para>Current instance of an active object implementing <see cref="ITouchManager"/> can be obtained via <see cref="TouchManager.Instance"/>.</para>
    /// </remarks>
    /// <seealso cref="TouchEventArgs"/>
    /// <example>
    /// This sample shows how to get Touch Manager instance and subscribe to events.
    /// <code>
    /// TouchManager.Instance.TouchesBegan += 
    ///     (sender, args) => { foreach (var touch in args.Touches) Debug.Log("Began: " + touch.Id); }; 
    /// TouchManager.Instance.TouchesEnded += 
    ///     (sender, args) => { foreach (var touch in args.Touches) Debug.Log("Ended: " + touch.Id); }; 
    /// </code>
    /// </example>
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

        /// <summary>
        /// Gets or sets current display device.
        /// </summary>
        /// <value>Object which holds properties of current display device, like DPI and others.</value>
        IDisplayDevice DisplayDevice { get; set; }

        /// <summary>
        /// Gets current DPI.
        /// </summary>
        /// <remarks>Shortcut for <see cref="IDisplayDevice.DPI"/>.</remarks>
        float DPI { get; }

        /// <summary>
        /// Indicates if TouchScript should create a CameraLayer for you if no layers present in a scene.
        /// </summary>
        /// <value><c>true</c> if a CameraLayer should be created on startup; otherwise, <c>false</c>.</value>
        /// <remarks>This is usually a desired behavior but sometimes you would want to turn this off if you are using TouchScript only to get touch input from some device.</remarks>
        Boolean ShouldCreateCameraLayer { get; set; }

        /// <summary>
        /// Gets the list of touch layers.
        /// </summary>
        /// <value>A sorted list of currently active touch layers.</value>
        IList<TouchLayer> Layers { get; }

        /// <summary>
        /// Gets number of pixels in a cm with current DPI.
        /// </summary>
        float DotsPerCentimeter { get; }

        /// <summary>
        /// Gets number of active touches.
        /// </summary>
        int NumberOfTouches { get; }

        /// <summary>
        /// Gets the list of active touches.
        /// </summary>
        /// <value>An unsorted list of all touches which began but have not ended yet.</value>
        IList<ITouch> ActiveTouches { get; }

        /// <summary>
        /// Adds a touch layer.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>True if the layer was added.</returns>
        bool AddLayer(TouchLayer layer);

        /// <summary>
        /// Adds a touch layer in a specific position.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <param name="index">Layer index to add the layer to.</param>
        /// <returns>True if the layer was added.</returns>
        bool AddLayer(TouchLayer layer, int index);

        /// <summary>
        /// Removes a touch layer.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        /// <returns>True if the layer was removed.</returns>
        bool RemoveLayer(TouchLayer layer);

        /// <summary>
        /// Swaps layers.
        /// </summary>
        /// <param name="at">Layer index 1.</param>
        /// <param name="to">Layer index 2.</param>
        void ChangeLayerIndex(int at, int to);

        /// <summary>
        /// Checks if a touch hits anything.
        /// </summary>
        /// <param name="position">Screen position of the touch.</param>
        /// <returns>Transform which has been hit or null otherwise.</returns>
        Transform GetHitTarget(Vector2 position);

        /// <summary>
        /// Checks if a touch hits anything.
        /// <seealso cref="ITouchHit"/>
        /// </summary>
        /// <param name="position">Screen position of the touch.</param>
        /// <param name="hit">An object which represents hit information.</param>
        /// <returns>True if the touch hits any Transform.</returns>
        bool GetHitTarget(Vector2 position, out ITouchHit hit);

        /// <summary>
        /// Checks if a touch hits anything.
        /// <seealso cref="ITouchHit"/>
        /// <seealso cref="TouchLayer"/>
        /// </summary>
        /// <param name="position">Screen position of the touch.</param>
        /// <param name="hit">An object which represents hit information.</param>
        /// <param name="layer">A layer which was hit.</param>
        /// <returns>True if the touch hits any Transform.</returns>
        bool GetHitTarget(Vector2 position, out ITouchHit hit, out TouchLayer layer);
    }

    /// <summary>
    /// Arguments dispatched with Touch Manager events.
    /// </summary>
    public class TouchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets list of touches participating in the event.
        /// </summary>
        /// <value>List of touches added, changed or removed this frame.</value>
        public IList<ITouch> Touches { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchEventArgs"/> class.
        /// </summary>
        /// <param name="touches">List of touches for an event.</param>
        public TouchEventArgs(IList<ITouch> touches)
        {
            Touches = touches;
        }
    }
}
