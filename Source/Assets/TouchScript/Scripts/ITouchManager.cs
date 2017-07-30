/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Devices.Display;
using TouchScript.InputSources;
using TouchScript.Layers;
using TouchScript.Pointers;

namespace TouchScript
{
    /// <summary>
    /// <para>Core manager of all pointer input in <b>TouchScript</b>. It is responsible for assigning unique pointer ids and keeping the list of active pointers. Controls pointer frames and dispatches pointer events.</para>
    /// </summary>
    /// <remarks>
    /// <para>Every frame pointer events are dispatched in this order:</para>
    /// <list type="number">
    /// <item><description>FrameStarted</description></item>
    /// <item><description>PointersAdded</description></item>
    /// <item><description>PointersUpdated</description></item>
    /// <item><description>PointersPressed</description></item>
    /// <item><description>PointersReleased</description></item>
    /// <item><description>PointersRemoved</description></item>
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
    /// TouchManager.Instance.PointersPressed += 
    ///     (sender, args) => { foreach (var pointer in args.Pointers) Debug.Log("Pressed: " + pointer.Id); }; 
    /// TouchManager.Instance.PointersReleased += 
    ///     (sender, args) => { foreach (var pointer in args.Pointers) Debug.Log("Released: " + pointer.Id); }; 
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
        /// Occurs when new hovering pointers are added.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersAdded;

        /// <summary>
        /// Occurs when pointers are updated.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersUpdated;

        /// <summary>
        /// Occurs when pointers touch the surface.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersPressed;

        /// <summary>
        /// Occurs when pointers are released.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersReleased;

        /// <summary>
        /// Occurs when pointers are removed from the system.
        /// </summary>
        event EventHandler<PointerEventArgs> PointersRemoved;

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
        /// Indicates if TouchScript should create a <see cref="StandardLayer"/> for you if no layers present in a scene.
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
        /// Gets the list of <see cref="IInputSource"/>
        /// </summary>
        /// <value> A sorted list of input sources. </value>
        IList<IInputSource> Inputs { get; }

        /// <summary>
        /// Gets number of pixels in a cm with current DPI.
        /// </summary>
        float DotsPerCentimeter { get; }

        /// <summary>
        /// Gets number of pointers in the system.
        /// </summary>
        int PointersCount { get; }

        /// <summary>
        /// Gets the number of pressed pointer in the system.
        /// </summary>
        int PressedPointersCount { get; }

        /// <summary>
        /// Gets the list of pointers.
        /// </summary>
        /// <value>An unsorted list of all pointers.</value>
        IList<Pointer> Pointers { get; }

        /// <summary>
        /// Gets the list of pressed pointers.
        /// </summary>
        /// <value>An unsorted list of all pointers which were pressed but not released yet.</value>
        IList<Pointer> PressedPointers { get; }

        /// <summary>
        /// Indicates that execution is currently inside a TouchScript Pointer Frame, i.e. before <see cref="FrameFinished"/> and after <see cref="FrameStarted"/> events.
        /// </summary>
        /// <value>
        ///   <c>true</c> if execution is inside a TouchScript Pointer Frame; otherwise, <c>false</c>.
        /// </value>
        bool IsInsidePointerFrame { get; }

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
        /// Cancels a pointer and returns it to the system of need.
        /// </summary>
        /// <param name="id">Pointer id to cancel.</param>
        /// <param name="shouldReturn">If the pointer should be redispatched to the system.</param>
        void CancelPointer(int id, bool shouldReturn);

        /// <summary>
        /// Cancels a pointer.
        /// </summary>
        /// <param name="id">Pointer id to cancel.</param>
        void CancelPointer(int id);

        /// <summary>
        /// Tells TouchScript to update internal state after a resolution change.
        /// </summary>
        void UpdateResolution();
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