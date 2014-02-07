/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Events;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    [AddComponentMenu("TouchScript/Touch Manager")]
    public class TouchManager : MonoBehaviour
    {
        #region Constants

        [Flags]
        public enum MessageTypes
        {
            FrameStarted = 1 << 0,
            FrameFinished = 1 << 1,
            TouchesBegan = 1 << 2,
            TouchesMoved = 1 << 3,
            TouchesEnded = 1 << 4,
            TouchesCancelled = 1 << 5
        }

        public enum MessageNames
        {
            OnTouchFrameStarted = MessageTypes.FrameStarted,
            OnTouchFrameFinished = MessageTypes.FrameFinished,
            OnTouchesBegan = MessageTypes.TouchesBegan,
            OnTouchesMoved = MessageTypes.TouchesMoved,
            OnTouchesEnded = MessageTypes.TouchesEnded,
            OnTouchesCancelled = MessageTypes.TouchesCancelled
        }

        /// <summary>
        /// Ratio of cm to inch
        /// </summary>
        public const float CM_TO_INCH = 0.393700787f;

        /// <summary>
        /// Ratio of inch to cm
        /// </summary>
        public const float INCH_TO_CM = 1/CM_TO_INCH;

        #endregion

        #region Public properties

        /// <summary>
        /// TouchManager singleton instance.
        /// </summary>
        public static ITouchManager Instance
        {
            get { return TouchManagerInstance.Instance; }
        }

        /// <summary>
        /// Current DPI.
        /// </summary>
        public float DPI
        {
            get
            {
                if (Instance == null)
                {
                    return Application.isEditor ? editorDpi : liveDpi;
                }
                return Instance.DPI;
            }
            set
            {
                if (Instance == null)
                {
                    if (Application.isEditor) editorDpi = value;
                    else liveDpi = value;
                } else
                {
                    Instance.DPI = value;
                }
            }
        }

        /// <summary>
        /// DPI while testing in editor.
        /// </summary>
        public float EditorDPI
        {
            get
            {
                if (Instance == null) return editorDpi;
                return Instance.EditorDPI;
            }
            set
            {
                if (Instance == null) editorDpi = value;
                else Instance.EditorDPI = value;
            }
        }

        /// <summary>
        /// DPI of target touch device.
        /// </summary>
        public float LiveDPI
        {
            get
            {
                if (Instance == null) return liveDpi;
                return Instance.LiveDPI;
            }
            set
            {
                if (Instance == null) liveDpi = value;
                else Instance.LiveDPI = value;
            }
        }

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

        public MessageTypes SendMessageEvents
        {
            get { return sendMessageEvents; }
            set
            {
                if (sendMessageEvents == value) return;
                sendMessageEvents = value;
                updateSubscription();
            }
        }

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

        #region Private variables

        [SerializeField]
        private float liveDpi = 72;

        [SerializeField]
        private float editorDpi = 72;

        [SerializeField]
        private bool useSendMessage = false;

        [SerializeField]
        private MessageTypes sendMessageEvents = MessageTypes.TouchesBegan | MessageTypes.TouchesCancelled | MessageTypes.TouchesEnded | MessageTypes.TouchesMoved;

        [SerializeField]
        private GameObject sendMessageTarget;

        [SerializeField]
        private List<TouchLayer> layers = new List<TouchLayer>();

        #endregion

        #region Unity

        private void OnEnable()
        {
            if (Instance == null) return;

            Instance.LiveDPI = liveDpi;
            Instance.EditorDPI = editorDpi;
            for (var i = 0; i < layers.Count; i++)
            {
                Instance.AddLayer(layers[i], i);
            }

            updateSubscription();
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

            if ((SendMessageEvents & MessageTypes.FrameStarted) != 0) Instance.FrameStarted += frameStartedhandler;
            if ((SendMessageEvents & MessageTypes.FrameFinished) != 0) Instance.FrameFinished += frameFinishedHandler;
            if ((SendMessageEvents & MessageTypes.TouchesBegan) != 0) Instance.TouchesBegan += touchesBeganHandler;
            if ((SendMessageEvents & MessageTypes.TouchesMoved) != 0) Instance.TouchesMoved += touchesMovedHandler;
            if ((SendMessageEvents & MessageTypes.TouchesEnded) != 0) Instance.TouchesEnded += touchesEndedHandler;
            if ((SendMessageEvents & MessageTypes.TouchesCancelled) != 0) Instance.TouchesCancelled += touchesCancelledHandler;
        }

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageNames.OnTouchesBegan.ToString(), e.TouchPoints);
        }

        private void touchesMovedHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageNames.OnTouchesMoved.ToString(), e.TouchPoints);
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageNames.OnTouchesEnded.ToString(), e.TouchPoints);
        }

        private void touchesCancelledHandler(object sender, TouchEventArgs e)
        {
            sendMessageTarget.SendMessage(MessageNames.OnTouchesCancelled.ToString(), e.TouchPoints);
        }

        private void frameStartedhandler(object sender, EventArgs e)
        {
            sendMessageTarget.SendMessage(MessageNames.OnTouchFrameStarted.ToString());
        }

        private void frameFinishedHandler(object sender, EventArgs e)
        {
            sendMessageTarget.SendMessage(MessageNames.OnTouchFrameFinished.ToString());
        }

        #endregion
    }
}