/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Gesture which recognizes a point cluster which didn't move for specified time since it appeared.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Long Press Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_LongPressGesture.htm")]
    public class LongPressGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string LONG_PRESS_MESSAGE = "OnLongPress";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> LongPressed
        {
            add { longPressedInvoker += value; }
            remove { longPressedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> longPressedInvoker;

        /// <summary>
        /// Unity event, occurs when gesture is recognized.
        /// </summary>
        public GestureEvent OnLongPress = new GestureEvent();

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets total time in seconds required to hold pointers still.
        /// </summary>
        /// <value> Time in seconds. </value>
        public float TimeToPress
        {
            get { return timeToPress; }
            set { timeToPress = value; }
        }

        /// <summary>
        /// Gets or sets maximum distance in cm pointers can move before gesture fails.
        /// </summary>
        /// <value> Distance in cm. </value>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set
            {
                distanceLimit = value;
                distanceLimitInPixelsSquared = Mathf.Pow(distanceLimit * touchManager.DotsPerCentimeter, 2);
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float timeToPress = 1;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float distanceLimit = float.PositiveInfinity;

        private float distanceLimitInPixelsSquared;

        private Vector2 totalMovement;

		private CustomSampler gestureSampler;

        #endregion

        #region Unity methods

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();

			gestureSampler = CustomSampler.Create("[TouchScript] Long Press Gesture");
		}

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            distanceLimitInPixelsSquared = Mathf.Pow(distanceLimit * touchManager.DotsPerCentimeter, 2);
        }

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
			basicEditor = true;
		}

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void pointersPressed(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersPressed(pointers);

            if (pointersNumState == PointersNumState.PassedMaxThreshold ||
                pointersNumState == PointersNumState.PassedMinMaxThreshold)
            {
                setState(GestureState.Failed);
            }
            else if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                setState(GestureState.Possible);
                StartCoroutine("wait");
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void pointersUpdated(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersUpdated(pointers);

            if (distanceLimit < float.PositiveInfinity)
            {
                totalMovement += ScreenPosition - PreviousScreenPosition;
                if (totalMovement.sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void pointersReleased(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersReleased(pointers);

            if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                setState(GestureState.Failed);
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (longPressedInvoker != null) longPressedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(LONG_PRESS_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            if (UseUnityEvents) OnLongPress.Invoke(this);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            totalMovement = Vector2.zero;
            StopCoroutine("wait");
        }

        #endregion

        #region Private functions

        private IEnumerator wait()
        {
            // WaitForSeconds is affected by time scale!
            var targetTime = Time.unscaledTime + TimeToPress;
            while (targetTime > Time.unscaledTime) yield return null;

            if (State == GestureState.Possible)
            {
                var data = GetScreenPositionHitData();
                if (data.Target == null || !data.Target.IsChildOf(cachedTransform))
                    setState(GestureState.Failed);
                else
                    setState(GestureState.Recognized);
            }
        }

        #endregion
    }
}