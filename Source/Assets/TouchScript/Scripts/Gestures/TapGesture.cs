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
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_TapGesture.htm")]
    public class TapGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string TAP_MESSAGE = "OnTap";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> Tapped
        {
            add { tappedInvoker += value; }
            remove { tappedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> tappedInvoker;

        /// <summary>
        /// Unity event, occurs when gesture is recognized.
        /// </summary>
        public GestureEvent OnTap = new GestureEvent();

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the number of taps required for the gesture to recognize.
        /// </summary>
        /// <value> The number of taps required for this gesture to recognize. <c>1</c> — dingle tap, <c>2</c> — double tap. </value>
        public int NumberOfTapsRequired
        {
            get { return numberOfTapsRequired; }
            set
            {
                if (value <= 0) numberOfTapsRequired = 1;
                else numberOfTapsRequired = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum hold time before gesture fails.
        /// </summary>
        /// <value> Number of seconds a user should hold their fingers before gesture fails. </value>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Gets or sets maximum distance for point cluster must move for the gesture to fail.
        /// </summary>
        /// <value> Distance in cm pointers must move before gesture fails. </value>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set
            {
                distanceLimit = value;
                distanceLimitInPixelsSquared = Mathf.Pow(distanceLimit * touchManager.DotsPerCentimeter, 2);
            }
        }

        /// <summary>
        /// Gets or sets the flag if pointers should be treated as a cluster.
        /// </summary>
        /// <value> <c>true</c> if pointers should be treated as a cluster; otherwise, <c>false</c>. </value>
        /// <remarks>
        /// At the end of a gesture when pointers are lifted off due to the fact that computers are faster than humans the very last pointer's position will be gesture's <see cref="Gesture.ScreenPosition"/> after that. This flag is used to combine several pointers which from the point of a user were lifted off simultaneously and set their centroid as gesture's <see cref="Gesture.ScreenPosition"/>.
        /// </remarks>
        public bool CombinePointers
        {
            get { return combinePointers; }
            set { combinePointers = value; }
        }

        /// <summary>
        /// Gets or sets time interval before gesture is recognized to combine all lifted pointers into a cluster to use its center as <see cref="Gesture.ScreenPosition"/>.
        /// </summary>
        /// <value> Time in seconds to treat pointers lifted off during this interval as a single gesture. </value>
        public float CombinePointersInterval
        {
            get { return combinePointersInterval; }
            set { combinePointersInterval = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private int numberOfTapsRequired = 1;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float timeLimit = float.PositiveInfinity;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float distanceLimit = float.PositiveInfinity;

        [SerializeField]
        [ToggleLeft]
        private bool combinePointers = false;

        [SerializeField]
        private float combinePointersInterval = .3f;

        private float distanceLimitInPixelsSquared;

        // isActive works in a tap cycle (i.e. when double/tripple tap is being recognized)
        // State -> Possible happens when the first pointer is detected
        private bool isActive = false;
        private int tapsDone;
        private Vector2 startPosition;
        private Vector2 totalMovement;
        private TimedSequence<Pointer> pointerSequence = new TimedSequence<Pointer>();

		private CustomSampler gestureSampler;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool ShouldReceivePointer(Pointer pointer)
        {
            if (!base.ShouldReceivePointer(pointer)) return false;
            // Ignore redispatched pointers — they come from 2+ pointer gestures when one is left with 1 pointer.
            // In this state it means that the user doesn't have an intention to tap the object.
            return (pointer.Flags & Pointer.FLAG_RETURNED) == 0;
        }

        #endregion

        #region Unity methods

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();

			gestureSampler = CustomSampler.Create("[TouchScript] Tap Gesture");
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
				gestureSampler.End();
                return;
            }

            if (NumPointers == pointers.Count)
            {
                // the first ever pointer
                if (tapsDone == 0)
                {
                    startPosition = pointers[0].Position;
                    if (timeLimit < float.PositiveInfinity) StartCoroutine("wait");
                }
                else if (tapsDone >= numberOfTapsRequired) // Might be delayed and retapped while waiting
                {
                    reset();
                    startPosition = pointers[0].Position;
                    if (timeLimit < float.PositiveInfinity) StartCoroutine("wait");
                }
                else
                {
                    if (distanceLimit < float.PositiveInfinity)
                    {
                        if ((pointers[0].Position - startPosition).sqrMagnitude > distanceLimitInPixelsSquared)
                        {
                            setState(GestureState.Failed);
							gestureSampler.End();
                            return;
                        }
                    }
                }
            }
            if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                // Starting the gesture when it is already active? => we released one finger and pressed again
                if (isActive) setState(GestureState.Failed);
                else
                {
                    if (State == GestureState.Idle) setState(GestureState.Possible);
                    isActive = true;
                }
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
                totalMovement += pointers[0].Position - pointers[0].PreviousPosition;
                if (totalMovement.sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void pointersReleased(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersReleased(pointers);

            if (combinePointers)
            {
                var count = pointers.Count;
                for (var i = 0; i < count; i++) pointerSequence.Add(pointers[i]);

                if (NumPointers == 0)
                {
                    // Checking which points were removed in clusterExistenceTime seconds to set their centroid as cached screen position
                    var cluster = pointerSequence.FindElementsLaterThan(Time.unscaledTime - combinePointersInterval, shouldCachePointerPosition);
                    cachedScreenPosition = ClusterUtils.Get2DCenterPosition(cluster);
                    cachedPreviousScreenPosition = ClusterUtils.GetPrevious2DCenterPosition(cluster);
                }
            }
            else
            {
                if (NumPointers == 0)
                {
                    if (!isActive)
                    {
                        setState(GestureState.Failed);
						gestureSampler.End();
                        return;
                    }

                    // pointers outside of gesture target are ignored in shouldCachePointerPosition()
                    // if all pointers are outside ScreenPosition will be invalid
                    if (TouchManager.IsInvalidPosition(ScreenPosition))
                    {
                        setState(GestureState.Failed);
                    }
                    else
                    {
                        tapsDone++;
                        isActive = false;
                        if (tapsDone >= numberOfTapsRequired) setState(GestureState.Recognized);
                    }
                }
            }

			gestureSampler.End();
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();

            StopCoroutine("wait");
            if (tappedInvoker != null) tappedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null) SendMessageTarget.SendMessage(TAP_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
            if (UseUnityEvents) OnTap.Invoke(this);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            isActive = false;
            totalMovement = Vector2.zero;
            StopCoroutine("wait");
            tapsDone = 0;
        }

        /// <inheritdoc />
        protected override bool shouldCachePointerPosition(Pointer value)
        {
            // Points must be over target when released
            return PointerUtils.IsPointerOnTarget(value, cachedTransform);
        }

        #endregion

        #region private functions

        private IEnumerator wait()
        {
            // WaitForSeconds is affected by time scale!
            var targetTime = Time.unscaledTime + TimeLimit;
            while (targetTime > Time.unscaledTime) yield return null;

            if (State == GestureState.Idle || State == GestureState.Possible) setState(GestureState.Failed);
        }

        #endregion
    }
}