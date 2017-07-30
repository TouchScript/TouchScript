/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Core;
using TouchScript.Devices.Display;
using TouchScript.Layers;
using TouchScript.Pointers;
using TouchScript.Utils.Attributes;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace TouchScript
{
    /// <summary>
    /// A facade object to configure and hold parameters for an instance of <see cref="ITouchManager"/>. Contains constants used throughout the library.
    /// <seealso cref="ITouchManager"/>
    /// </summary>
    /// <remarks>
    /// <para>An instance of <see cref="TouchManager"/> may be added to a Unity scene to hold (i.e. serialize them to the scene) parameters needed to configure an instance of <see cref="ITouchManager"/> used in application. Which can be accessed via <see cref="TouchManager.Instance"/> static property.</para>
    /// <para>Though it's not required it is a convenient way to configure <b>TouchScript</b> for your scene. You can use different configuration options for different scenes.</para>
    /// </remarks>
    [AddComponentMenu("TouchScript/Touch Manager")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_TouchManager.htm")]
    public sealed class TouchManager : DebuggableMonoBehaviour
    {
        #region Constants

#if TOUCHSCRIPT_DEBUG
        public const int DEBUG_GL_START = int.MinValue;
        public const int DEBUG_GL_TOUCH = DEBUG_GL_START;
#endif

        /// <summary>
        /// Event implementation in Unity EventSystem for pointer events.
        /// </summary>
        [Serializable]
        public class PointerEvent : UnityEvent<IList<Pointer>> {}

        /// <summary>
        /// Event implementation in Unity EventSystem for frame events.
        /// </summary>
        /// <seealso cref="UnityEngine.Events.UnityEvent" />
        [Serializable]
        public class FrameEvent : UnityEvent {}

        /// <summary>
        /// Values of a bit-mask representing which Unity messages an instance of <see cref="TouchManager"/> will dispatch.
        /// </summary>
        [Flags]
        public enum MessageType
        {
            /// <summary>
            /// Pointer frame started.
            /// </summary>
            FrameStarted = 1 << 0,

            /// <summary>
            /// Pointer frame finished.
            /// </summary>
            FrameFinished = 1 << 1,

            /// <summary>
            /// Some pointers were added during the frame.
            /// </summary>
            PointersAdded = 1 << 2,

            /// <summary>
            /// Some pointers were updated during the frame.
            /// </summary>
            PointersUpdated = 1 << 3,

            /// <summary>
            /// Some pointers have touched the surface during the frame.
            /// </summary>
            PointersPressed = 1 << 4,

            /// <summary>
            /// Some pointers were released during the frame.
            /// </summary>
            PointersReleased = 1 << 5,

            /// <summary>
            /// Some pointers were removed during the frame.
            /// </summary>
            PointersRemoved = 1 << 6,

            /// <summary>
            /// Some pointers were cancelled during the frame.
            /// </summary>
            PointersCancelled = 1 << 7
        }

        /// <summary>
        /// Names of dispatched Unity messages.
        /// </summary>
        public enum MessageName
        {
            /// <summary>
            /// Pointer frame started.
            /// </summary>
            OnFrameStart = MessageType.FrameStarted,

            /// <summary>
            /// Pointer frame finished.
            /// </summary>
            OnFrameFinish = MessageType.FrameFinished,

            /// <summary>
            /// Some pointers were added during the frame.
            /// </summary>
            OnPointersAdd = MessageType.PointersAdded,

            /// <summary>
            /// Some pointers have updated during the frame.
            /// </summary>
            OnPointersUpdate = MessageType.PointersUpdated,

            /// <summary>
            /// Some pointers have touched the surface during the frame.
            /// </summary>
            OnPointersPress = MessageType.PointersPressed,

            /// <summary>
            /// Some pointers were released during the frame.
            /// </summary>
            OnPointersRelease = MessageType.PointersReleased,

            /// <summary>
            /// Some pointers were removed during the frame.
            /// </summary>
            OnPointersRemove = MessageType.PointersRemoved,

            /// <summary>
            /// Some pointers were cancelled during the frame.
            /// </summary>
            OnPointersCancel = MessageType.PointersCancelled
        }

        /// <summary>
        /// Centimeter to inch ratio to be used in DPI calculations.
        /// </summary>
        public const float CM_TO_INCH = 0.393700787f;

        /// <summary>
        /// Inch to centimeter ratio to be used in DPI calculations.
        /// </summary>
        public const float INCH_TO_CM = 1 / CM_TO_INCH;

        /// <summary>
        /// The value used to represent an unknown state of a screen position. Use <see cref="TouchManager.IsInvalidPosition"/> to check if a point has unknown value.
        /// </summary>
        public static readonly Vector2 INVALID_POSITION = new Vector2(float.NaN, float.NaN);

        /// <summary>
        /// TouchScript version.
        /// </summary>
        public static readonly Version VERSION = new Version(9, 0);

        /// <summary>
        /// TouchScript version suffix.
        /// </summary>
        public static readonly string VERSION_SUFFIX = "";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new frame is started before all other events.
        /// </summary>
        public FrameEvent OnFrameStart = new FrameEvent();

        /// <summary>
        /// Occurs when a frame is finished. After all other events.
        /// </summary>
        [SerializeField]
        public FrameEvent OnFrameFinish = new FrameEvent();

        /// <summary>
        /// Occurs when new hovering pointers are added.
        /// </summary>
        [SerializeField]
        public PointerEvent OnPointersAdd = new PointerEvent();

        /// <summary>
        /// Occurs when pointers are updated.
        /// </summary>
        [SerializeField]
        public PointerEvent OnPointersUpdate = new PointerEvent();

        /// <summary>
        /// Occurs when pointers touch the surface.
        /// </summary>
        [SerializeField]
        public PointerEvent OnPointersPress = new PointerEvent();

        /// <summary>
        /// Occurs when pointers are released.
        /// </summary>
        [SerializeField]
        public PointerEvent OnPointersRelease = new PointerEvent();

        /// <summary>
        /// Occurs when pointers are removed from the system.
        /// </summary>
        [SerializeField]
        public PointerEvent OnPointersRemove = new PointerEvent();

        /// <summary>
        /// Occurs when pointers are cancelled.
        /// </summary>
        [SerializeField]
        public PointerEvent OnPointersCancel = new PointerEvent();

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the instance of <see cref="ITouchManager"/> implementation used in the application.
        /// </summary>
        /// <value>An instance of <see cref="ITouchManager"/> which is in charge of global pointer input control in the application.</value>
        public static ITouchManager Instance
        {
            get { return TouchManagerInstance.Instance; }
        }

        /// <summary>
        /// Gets or sets current display device.
        /// </summary>
        /// <value>Object which holds properties of current display device, like DPI and others.</value>
        /// <remarks>A shortcut for <see cref="ITouchManager.DisplayDevice"/> which is also serialized into scene.</remarks>
        public IDisplayDevice DisplayDevice
        {
            get
            {
                if (Instance == null) return displayDevice as IDisplayDevice;
                return Instance.DisplayDevice;
            }
            set
            {
                if (Instance == null)
                {
                    displayDevice = value as Object;
                    return;
                }
                Instance.DisplayDevice = value;
            }
        }

        /// <summary>
        /// Indicates if TouchScript should create a CameraLayer for you if no layers present in a scene.
        /// </summary>
        /// <value><c>true</c> if a CameraLayer should be created on startup; otherwise, <c>false</c>.</value>
        /// <remarks>This is usually a desired behavior but sometimes you would want to turn this off if you are using TouchScript only to get pointer input from some device.</remarks>
        public bool ShouldCreateCameraLayer
        {
            get { return shouldCreateCameraLayer; }
            set { shouldCreateCameraLayer = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="TouchScript.InputSources.StandardInput"/> should be created in scene if no inputs present.
        /// </summary>
        /// <value> <c>true</c> if StandardInput should be created; otherwise, <c>false</c>. </value>
        /// <remarks>This is usually a desired behavior but sometimes you would want to turn this off.</remarks>
        public bool ShouldCreateStandardInput
        {
            get { return shouldCreateStandardInput; }
            set { shouldCreateStandardInput = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Unity messages are sent when <see cref="ITouchManager"/> dispatches events.
        /// </summary>
        /// <value><c>true</c> if Unity messages are used; otherwise, <c>false</c>.</value>
        /// <remarks>If Unity messages are used they are sent to an object set as a value of <see cref="SendMessageTarget"/> property or to TouchManager's GameObject if it's <c>null</c>.</remarks>
        public bool UseSendMessage
        {
            get { return useSendMessage; }
            set
            {
                if (value == useSendMessage) return;
                useSendMessage = value;
                updateSendMessageSubscription();
            }
        }

        /// <summary>
        /// Gets or sets the bit-mask which indicates which events from an instance of <see cref="ITouchManager"/> are sent as Unity messages.
        /// </summary>
        /// <value>Bit-mask with corresponding bits for used events.</value>
        public MessageType SendMessageEvents
        {
            get { return sendMessageEvents; }
            set
            {
                if (sendMessageEvents == value) return;
                sendMessageEvents = value;
                updateSendMessageSubscription();
            }
        }

        /// <summary>
        /// Gets or sets the SendMessage target GameObject.
        /// </summary>
        /// <value>Which GameObject to use to dispatch Unity messages. If <c>null</c>, TouchManager's GameObject is used.</value>
        public GameObject SendMessageTarget
        {
            get { return sendMessageTarget; }
            set
            {
                sendMessageTarget = value;
                if (value == null) sendMessageTarget = gameObject;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Unity Events should be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if TouchManager should use Unity Events; otherwise, <c>false</c>.
        /// </value>
        public bool UseUnityEvents
        {
            get { return useUnityEvents; }
            set
            {
                if (useUnityEvents == value) return;
                useUnityEvents = value;
                updateUnityEventsSubscription();
            }
        }

#if TOUCHSCRIPT_DEBUG

        /// <inheritdoc />
        public override bool DebugMode
        {
            get { return base.DebugMode; }
            set
            {
                base.DebugMode = value;
                if (Application.isPlaying) (Instance as TouchManagerInstance).DebugMode = value;
            }
        }

#endif

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether a Vector2 represents an invalid position, i.e. if it is equal to <see cref="INVALID_POSITION"/>.
        /// </summary>
        /// <param name="position">Screen position.</param>
        /// <returns><c>true</c> if position is invalid; otherwise, <c>false</c>.</returns>
        public static bool IsInvalidPosition(Vector2 position)
        {
			return position.x == INVALID_POSITION.x && position.y == INVALID_POSITION.y;
        }

        #endregion

        #region Private variables

        #pragma warning disable CS0414

        [SerializeField]
        [HideInInspector]
        private bool basicEditor = true;

        #pragma warning restore CS0414

		[SerializeField]
        private Object displayDevice;

        [SerializeField]
        [ToggleLeft]
        private bool shouldCreateCameraLayer = true;

        [SerializeField]
        [ToggleLeft]
        private bool shouldCreateStandardInput = true;

        [SerializeField]
        [ToggleLeft]
        private bool useSendMessage = false;

        [SerializeField]
        private MessageType sendMessageEvents = MessageType.PointersPressed | MessageType.PointersCancelled |
                                                MessageType.PointersReleased | MessageType.PointersUpdated |
                                                MessageType.PointersAdded | MessageType.PointersRemoved;

        [SerializeField]
        private GameObject sendMessageTarget;

        [SerializeField]
        private bool useUnityEvents = false;

        [SerializeField]
        private List<TouchLayer> layers = new List<TouchLayer>();

        #endregion

        #region Unity

        private void Awake()
        {
            if (Instance == null) return;

#if TOUCHSCRIPT_DEBUG
            if (DebugMode) (Instance as TouchManagerInstance).DebugMode = true;
#endif

            Instance.DisplayDevice = displayDevice as IDisplayDevice;
            Instance.ShouldCreateCameraLayer = ShouldCreateCameraLayer;
            Instance.ShouldCreateStandardInput = ShouldCreateStandardInput;
            for (var i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer != null) LayerManager.Instance.AddLayer(layer, i);
            }
        }

        private void OnEnable()
        {
            updateSendMessageSubscription();
            updateUnityEventsSubscription();
        }

        private void OnDisable()
        {
            removeSendMessageSubscriptions();
            removeUnityEventsSubscriptions();
        }

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
            basicEditor = true;
		}

        #endregion

        #region Private functions

        private void updateSendMessageSubscription()
        {
            if (!Application.isPlaying) return;
            if (Instance == null) return;

            if (sendMessageTarget == null) sendMessageTarget = gameObject;

            removeSendMessageSubscriptions();

            if (!useSendMessage) return;

            if ((SendMessageEvents & MessageType.FrameStarted) != 0) Instance.FrameStarted += frameStartedSendMessageHandler;
            if ((SendMessageEvents & MessageType.FrameFinished) != 0) Instance.FrameFinished += frameFinishedSendMessageHandler;
            if ((SendMessageEvents & MessageType.PointersAdded) != 0) Instance.PointersAdded += pointersAddedSendMessageHandler;
            if ((SendMessageEvents & MessageType.PointersUpdated) != 0) Instance.PointersUpdated += pointersUpdatedSendMessageHandler;
            if ((SendMessageEvents & MessageType.PointersPressed) != 0) Instance.PointersPressed += pointersPressedSendMessageHandler;
            if ((SendMessageEvents & MessageType.PointersReleased) != 0) Instance.PointersReleased += pointersReleasedSendMessageHandler;
            if ((SendMessageEvents & MessageType.PointersRemoved) != 0) Instance.PointersRemoved += pointersRemovedSendMessageHandler;
            if ((SendMessageEvents & MessageType.PointersCancelled) != 0) Instance.PointersCancelled += pointersCancelledSendMessageHandler;
        }

        private void removeSendMessageSubscriptions()
        {
            if (!Application.isPlaying) return;
            if (Instance == null) return;

            Instance.FrameStarted -= frameStartedSendMessageHandler;
            Instance.FrameFinished -= frameFinishedSendMessageHandler;
            Instance.PointersAdded -= pointersAddedSendMessageHandler;
            Instance.PointersUpdated -= pointersUpdatedSendMessageHandler;
            Instance.PointersPressed -= pointersPressedSendMessageHandler;
            Instance.PointersReleased -= pointersReleasedSendMessageHandler;
            Instance.PointersRemoved -= pointersRemovedSendMessageHandler;
            Instance.PointersCancelled -= pointersCancelledSendMessageHandler;
        }

        private void pointersAddedSendMessageHandler(object sender, PointerEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnPointersAdd.ToString(), e.Pointers,
                SendMessageOptions.DontRequireReceiver);
        }

        private void pointersUpdatedSendMessageHandler(object sender, PointerEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnPointersUpdate.ToString(), e.Pointers,
                SendMessageOptions.DontRequireReceiver);
        }

        private void pointersPressedSendMessageHandler(object sender, PointerEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnPointersPress.ToString(), e.Pointers,
                SendMessageOptions.DontRequireReceiver);
        }

        private void pointersReleasedSendMessageHandler(object sender, PointerEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnPointersRelease.ToString(), e.Pointers,
                SendMessageOptions.DontRequireReceiver);
        }

        private void pointersRemovedSendMessageHandler(object sender, PointerEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnPointersRemove.ToString(), e.Pointers,
                SendMessageOptions.DontRequireReceiver);
        }

        private void pointersCancelledSendMessageHandler(object sender, PointerEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnPointersCancel.ToString(), e.Pointers,
                SendMessageOptions.DontRequireReceiver);
        }

        private void frameStartedSendMessageHandler(object sender, EventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnFrameStart.ToString(),
                SendMessageOptions.DontRequireReceiver);
        }

        private void frameFinishedSendMessageHandler(object sender, EventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnFrameFinish.ToString(),
                SendMessageOptions.DontRequireReceiver);
        }

        private void updateUnityEventsSubscription()
        {
            if (!Application.isPlaying) return;
            if (Instance == null) return;

            removeUnityEventsSubscriptions();

            if (!useUnityEvents) return;

            Instance.FrameStarted += frameStartedUnityEventsHandler;
            Instance.FrameFinished += frameFinishedUnityEventsHandler;
            Instance.PointersAdded += pointersAddedUnityEventsHandler;
            Instance.PointersUpdated += pointersUpdatedUnityEventsHandler;
            Instance.PointersPressed += pointersPressedUnityEventsHandler;
            Instance.PointersReleased += pointersReleasedUnityEventsHandler;
            Instance.PointersRemoved += pointersRemovedUnityEventsHandler;
            Instance.PointersCancelled += pointersCancelledUnityEventsHandler;
        }

        private void removeUnityEventsSubscriptions()
        {
            if (!Application.isPlaying) return;
            if (Instance == null) return;

            Instance.FrameStarted -= frameStartedUnityEventsHandler;
            Instance.FrameFinished -= frameFinishedUnityEventsHandler;
            Instance.PointersAdded -= pointersAddedUnityEventsHandler;
            Instance.PointersUpdated -= pointersUpdatedUnityEventsHandler;
            Instance.PointersPressed -= pointersPressedUnityEventsHandler;
            Instance.PointersReleased -= pointersReleasedUnityEventsHandler;
            Instance.PointersRemoved -= pointersRemovedUnityEventsHandler;
            Instance.PointersCancelled -= pointersCancelledUnityEventsHandler;
        }

        private void pointersAddedUnityEventsHandler(object sender, PointerEventArgs e)
        {
            OnPointersAdd.Invoke(e.Pointers);
        }

        private void pointersUpdatedUnityEventsHandler(object sender, PointerEventArgs e)
        {
            OnPointersUpdate.Invoke(e.Pointers);
        }

        private void pointersPressedUnityEventsHandler(object sender, PointerEventArgs e)
        {
            OnPointersPress.Invoke(e.Pointers);
        }

        private void pointersReleasedUnityEventsHandler(object sender, PointerEventArgs e)
        {
            OnPointersRelease.Invoke(e.Pointers);
        }

        private void pointersRemovedUnityEventsHandler(object sender, PointerEventArgs e)
        {
            OnPointersRemove.Invoke(e.Pointers);
        }

        private void pointersCancelledUnityEventsHandler(object sender, PointerEventArgs e)
        {
            OnPointersCancel.Invoke(e.Pointers);
        }

        private void frameStartedUnityEventsHandler(object sender, EventArgs e)
        {
            OnFrameStart.Invoke();
        }

        private void frameFinishedUnityEventsHandler(object sender, EventArgs e)
        {
            OnFrameFinish.Invoke();
        }

        #endregion
    }
}