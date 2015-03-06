/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TouchScript.Devices.Display;
using TouchScript.Layers;
using TouchScript.Utils.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouchScript
{
    /// <summary>
    /// A façade object to configure and hold parameters for an instance of <see cref="ITouchManager"/>. Contains constants used throughout the library.
    /// <seealso cref="ITouchManager"/>
    /// </summary>
    /// <remarks>
    /// <para>An instance of <see cref="TouchManager"/> may be added to a Unity scene to hold (i.e. serialize them to the scene) parameters needed to configure an instance of <see cref="ITouchManager"/> used in application. Which can be accessed via <see cref="TouchManager.Instance"/> static property.</para>
    /// <para>Though it's not required it is a convenient way to configure <b>TouchScript</b> for your scene. You can use different configuration options for different scenes.</para>
    /// </remarks>
    /// <example>
    /// This sample shows how to get Touch Manager instance and subscribe to events.
    /// <code>
    /// TouchManager.Instance.TouchesBegan += 
    ///     (sender, args) => { foreach (var touch in args.Touches) Debug.Log("Began: " + touch.Id); }; 
    /// TouchManager.Instance.TouchesEnded += 
    ///     (sender, args) => { foreach (var touch in args.Touches) Debug.Log("Ended: " + touch.Id); }; 
    /// </code>
    /// </example>
    [AddComponentMenu("TouchScript/Touch Manager")]
    public sealed class TouchManager : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Values of a bit-mask representing which Unity messages an instance of <see cref="TouchManager"/> will dispatch.
        /// </summary>
        [Flags]
        public enum MessageType
        {
            /// <summary>
            /// Touch frame started.
            /// </summary>
            FrameStarted = 1 << 0,

            /// <summary>
            /// Touch frame finished.
            /// </summary>
            FrameFinished = 1 << 1,

            /// <summary>
            /// Some touches have begun during the frame.
            /// </summary>
            TouchesBegan = 1 << 2,

            /// <summary>
            /// Some touches have moved during the frame.
            /// </summary>
            TouchesMoved = 1 << 3,

            /// <summary>
            /// Some touches have ended during the frame.
            /// </summary>
            TouchesEnded = 1 << 4,

            /// <summary>
            /// Some touches were cancelled during the frame.
            /// </summary>
            TouchesCancelled = 1 << 5
        }

        /// <summary>
        /// Names of dispatched Unity messages.
        /// </summary>
        public enum MessageName
        {
            /// <summary>
            /// Touch frame started.
            /// </summary>
            OnTouchFrameStarted = MessageType.FrameStarted,

            /// <summary>
            /// Touch frame finished.
            /// </summary>
            OnTouchFrameFinished = MessageType.FrameFinished,

            /// <summary>
            /// Some touches have begun during the frame.
            /// </summary>
            OnTouchesBegan = MessageType.TouchesBegan,

            /// <summary>
            /// Some touches have moved during the frame.
            /// </summary>
            OnTouchesMoved = MessageType.TouchesMoved,

            /// <summary>
            /// Some touches have ended during the frame.
            /// </summary>
            OnTouchesEnded = MessageType.TouchesEnded,

            /// <summary>
            /// Some touches were cancelled during the frame.
            /// </summary>
            OnTouchesCancelled = MessageType.TouchesCancelled
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
        public static readonly Version VERSION = new Version(6, 5);

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the instance of <see cref="ITouchManager"/> implementation used in the application.
        /// </summary>
        /// <value>An instance of <see cref="ITouchManager"/> which is in charge of global touch input control in the application.</value>
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
                updateSubscription();
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
                updateSubscription();
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

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether a Vector2 represents an invalid position, i.e. if it is equal to <see cref="INVALID_POSITION"/>.
        /// </summary>
        /// <param name="position">Screen position.</param>
        /// <returns><c>true</c> if position is invalid; otherwise, <c>false</c>.</returns>
        public static bool IsInvalidPosition(Vector2 position)
        {
            return position.Equals(INVALID_POSITION);
        }

        #endregion

        #region Private variables

        [SerializeField]
        private Object displayDevice;

        [SerializeField]
        [ToggleLeft]
        private bool useSendMessage = false;

        [SerializeField]
        private MessageType sendMessageEvents = MessageType.TouchesBegan | MessageType.TouchesCancelled | MessageType.TouchesEnded | MessageType.TouchesMoved;

        [SerializeField]
        private GameObject sendMessageTarget;

        [SerializeField]
        private List<TouchLayer> layers = new List<TouchLayer>();

        #endregion

        #region Unity

        private void OnEnable()
        {
            if (Instance == null) return;

            Instance.DisplayDevice = displayDevice as IDisplayDevice;

            UpdateLayers();

            updateSubscription();
        }

        private void Start()
        {
            UpdateLayers();
        }

        #endregion

        #region Public functions

        public void UpdateLayers()
        {
            for (var i = 0; i < layers.Count; ++i)
            {
                Instance.AddLayer(layers[i], i);
            }
            layers = Instance.Layers.ToList();
        }

        #endregion

        #region Private functions

        private void updateSubscription()
        {
            if (!Application.isPlaying) return;
            if (Instance == null) return;

            if (sendMessageTarget == null) sendMessageTarget = gameObject;

            Instance.FrameStarted -= frameStartedhandler;
            Instance.FrameFinished -= frameFinishedHandler;
            Instance.TouchesBegan -= touchesBeganHandler;
            Instance.TouchesMoved -= touchesMovedHandler;
            Instance.TouchesEnded -= touchesEndedHandler;
            Instance.TouchesCancelled -= touchesCancelledHandler;

            if (!useSendMessage) return;

            if ((SendMessageEvents & MessageType.FrameStarted) != 0) Instance.FrameStarted += frameStartedhandler;
            if ((SendMessageEvents & MessageType.FrameFinished) != 0) Instance.FrameFinished += frameFinishedHandler;
            if ((SendMessageEvents & MessageType.TouchesBegan) != 0) Instance.TouchesBegan += touchesBeganHandler;
            if ((SendMessageEvents & MessageType.TouchesMoved) != 0) Instance.TouchesMoved += touchesMovedHandler;
            if ((SendMessageEvents & MessageType.TouchesEnded) != 0) Instance.TouchesEnded += touchesEndedHandler;
            if ((SendMessageEvents & MessageType.TouchesCancelled) != 0) Instance.TouchesCancelled += touchesCancelledHandler;
        }

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnTouchesBegan.ToString(), e.Touches, SendMessageOptions.DontRequireReceiver);
        }

        private void touchesMovedHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnTouchesMoved.ToString(), e.Touches, SendMessageOptions.DontRequireReceiver);
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnTouchesEnded.ToString(), e.Touches, SendMessageOptions.DontRequireReceiver);
        }

        private void touchesCancelledHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnTouchesCancelled.ToString(), e.Touches, SendMessageOptions.DontRequireReceiver);
        }

        private void frameStartedhandler(object sender, EventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnTouchFrameStarted.ToString(), SendMessageOptions.DontRequireReceiver);
        }

        private void frameFinishedHandler(object sender, EventArgs e)
        {
            sendMessageTarget.SendMessage(MessageName.OnTouchFrameFinished.ToString(), SendMessageOptions.DontRequireReceiver);
        }

        #endregion
    }
}
