/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Devices.Display;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// <para>Core manager of all pointer input in <b>TouchScript</b>. It is responsible for assigning unique pointer ids and keeping the list of active pointers. Controls pointer frames and dispatches pointer events.</para>
    /// </summary>
    /// <remarks>
    /// <para>Every frame pointer events are dispatched in this order:</para>
    /// <list type="number">
    /// <item><description>FrameStarted</description></item>
    /// <item><description>PointersBegan</description></item>
    /// <item><description>PointersMoved</description></item>
    /// <item><description>PointersEnded</description></item>
    /// <item><description>PointersCancelled</description></item>
    /// <item><description>FrameFinished</description></item>
    /// </list>
    /// <para>FrameStarted and FrameFinished events mark the start and the end of current pointer frame and allow to implement specific logic at these moments.</para>
    /// <para>Current instance of an active object implementing <see cref="ITouchManager"/> can be obtained via <see cref="TouchManager.Instance"/>.</para>
    /// </remarks>
    /// <seealso cref="PointerEventArgs"/>
    /// <example>
    /// This sample shows how to get TouchManager instance and subscribe to events.
    /// <code>
    /// TouchManager.Instance.PointersBegan += 
    ///     (sender, args) => { foreach (var pointer in args.Pointers) Debug.Log("Began: " + pointer.Id); }; 
    /// TouchManager.Instance.PointersEnded += 
    ///     (sender, args) => { foreach (var pointer in args.Pointers) Debug.Log("Ended: " + pointer.Id); }; 
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
        /// Occurs when new pointers are added.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersBegan;

        /// <summary>
        /// Occurs when pointers are updated.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersMoved;

        /// <summary>
        /// Occurs when pointers are removed.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersEnded;

        /// <summary>
        /// Occurs when pointers are cancelled.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersCancelled;

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
        /// Indicates if TouchScript should create a <see cref="CameraLayer"/> for you if no layers present in a scene.
        /// </summary>
        /// <value><c>true</c> if a CameraLayer should be created on startup; otherwise, <c>false</c>.</value>
        /// <remarks>This is usually a desired behavior but sometimes you would want to turn this off if you are using TouchScript only to get pointer input from some device.</remarks>
        bool ShouldCreateCameraLayer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="StandardInput"/> should be created in scene if no inputs present.
        /// </summary>
        /// <value> <c>true</c> if StandardInput should be created; otherwise, <c>false</c>. </value>
        /// <remarks>This is usually a desired behavior but sometimes you would want to turn this off.</remarks>
        bool ShouldCreateStandardInput { get; set; }

        /// <summary>
        /// Gets the list of <see cref="TouchLayer"/>.
        /// </summary>
        /// <value>A sorted list of currently active layers.</value>
        IList<TouchLayer> Layers { get; }

        /// <summary>
        /// Gets the list of <see cref="IInputSource"/>
        /// </summary>
        /// <value> A sorted list of input sources. </value>
        IList<IInputSource> Inputs { get; }

        /// <summary>
        /// Gets number of pixels in a cm with current DPI.
        /// </summary>
        float DotsPerCentimeter { get; }

        /// <summary>
        /// Gets number of active pointers.
        /// </summary>
        int NumberOfPointers { get; }

        /// <summary>
        /// Gets the list of active pointers.
        /// </summary>
        /// <value>An unsorted list of all pointers which began but have not ended yet.</value>
        IList<Pointer> ActivePointers { get; }

        /// <summary>
        /// Adds a layer in a specific position.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <param name="index">Layer index to add the layer to or <c>-1</c> to add to the end of the list.</param>
        /// <param name="addIfExists">if set to <c>true</c> move the layer to another index if it is already added; don't move otherwise.</param>
        /// <returns>
        /// True if the layer was added.
        /// </returns>
        bool AddLayer(TouchLayer layer, int index = -1, bool addIfExists = true);

        /// <summary>
        /// Removes a layer.
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
        /// Adds an input source.
        /// </summary>
        /// <param name="input"> Input source to add. </param>
        /// <returns> <c>true</c> if the input source wasn't in the list and was added; <c>false</c> otherwise. </returns>
        bool AddInput(IInputSource input);

        /// <summary>
        /// Removes the input.
        /// </summary>
        /// <param name="input"> Input source to remove. </param>
        /// <returns> <c>true</c> if the input source was removed; <c>false</c> otherwise. </returns>
        bool RemoveInput(IInputSource input);

        /// <summary>
        /// Checks if a pointer hits anything.
        /// </summary>
        /// <param name="position">Screen position of the pointer.</param>
        /// <returns>Transform which has been hit or null otherwise.</returns>
        Transform GetHitTarget(Vector2 position);

        /// <summary>
        /// Checks if a pointer hits anything.
        /// <seealso cref="TouchHit"/>
        /// </summary>
        /// <param name="position">Screen position of the pointer.</param>
        /// <param name="hit">An object which represents hit information.</param>
        /// <returns>True if the pointer hits any Transform.</returns>
        bool GetHitTarget(Vector2 position, out TouchHit hit);

        /// <summary>
        /// Checks if a pointer hits anything.
        /// <seealso cref="TouchHit"/>
        /// <seealso cref="TouchLayer"/>
        /// </summary>
        /// <param name="position">Screen position of the pointer.</param>
        /// <param name="hit">An object which represents hit information.</param>
        /// <param name="layer">A layer which was hit.</param>
        /// <returns>True if the pointer hits any Transform.</returns>
        bool GetHitTarget(Vector2 position, out TouchHit hit, out TouchLayer layer);

        /// <summary>
        /// Cancels a pointer and returns it to the system of need.
        /// </summary>
        /// <param name="id">Pointer id to cancel.</param>
        /// <param name="return">Should the pointer be returned to the system.</param>
        void CancelPointer(int id, bool @return);

        /// <summary>
        /// Cancels a pointer.
        /// </summary>
        /// <param name="id">Pointer id to cancel.</param>
        void CancelPointer(int id);
    }

    /// <summary>
    /// Arguments dispatched with TouchManager events.
    /// </summary>
    public class PointerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets list of pointers participating in the event.
        /// </summary>
        /// <value>List of pointers added, changed or removed this frame.</value>
        public IList<Pointer> Pointers { get; private set; }

        private static PointerEventArgs instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerEventArgs"/> class.
        /// </summary>
        private PointerEventArgs() {}

        /// <summary>
        /// Returns cached instance of EventArgs.
        /// This cached EventArgs is reused throughout the library not to alocate new ones on every call.
        /// </summary>
        /// <param name="pointers">A list of pointers for event.</param>
        /// <returns>Cached EventArgs object.</returns>
        public static PointerEventArgs GetCachedEventArgs(IList<Pointer> pointers)
        {
            if (instance == null) instance = new PointerEventArgs();
            instance.Pointers = pointers;
            return instance;
        }
    }
}