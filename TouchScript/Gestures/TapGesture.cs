/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections;
using System.Collections.Generic;
using TouchScript.Utils.Editor.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : Gesture
    {

        #region Public properties

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
        /// Maximum time to hold touches until gesture fails.
        /// </summary>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture fails.
        /// </summary>
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
        private int numberOfTapsRequired = 1;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float timeLimit = float.PositiveInfinity;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float distanceLimit = float.PositiveInfinity;
        private float distanceLimitInPixelsSquared;

        private int tapsDone;
        private Vector2 startPosition;
        private Vector2 totalMovement;

        #endregion

        #region Unity methods

        protected override void OnEnable()
        {
            base.OnEnable();

            distanceLimitInPixelsSquared = Mathf.Pow(distanceLimit * touchManager.DotsPerCentimeter, 2);
        }

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouch> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count == touches.Count)
            {
                if (tapsDone == 0)
                {
                    startPosition = touches[0].Position;
                    if (timeLimit < float.PositiveInfinity) StartCoroutine("wait");
                }
                else if (tapsDone >= numberOfTapsRequired) // Might be delayed and retapped while waiting
                {
                    setState(GestureState.Possible);
                    reset();
                    startPosition = touches[0].Position;
                    if (timeLimit < float.PositiveInfinity) StartCoroutine("wait");
                } else
                {
                    if (distanceLimit < float.PositiveInfinity)
                    {
                        if ((touches[0].Position - startPosition).sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouch> touches)
        {
            base.touchesMoved(touches);

            if (distanceLimit < float.PositiveInfinity)
            {
                totalMovement += ScreenPosition - PreviousScreenPosition;
                if (totalMovement.sqrMagnitude > distanceLimitInPixelsSquared) setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouch> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
            {
                if (TouchManager.IsInvalidPosition(ScreenPosition))
                {
                    setState(GestureState.Failed);
                } else
                {
                    tapsDone++;
                    if (tapsDone >= numberOfTapsRequired) setState(GestureState.Recognized);
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<ITouch> touches)
        {
            base.touchesCancelled(touches);

            setState(GestureState.Failed);
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();

            StopCoroutine("wait");
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            totalMovement = Vector2.zero;
            StopCoroutine("wait");
            tapsDone = 0;
        }

        /// <inheritdoc />
        protected override bool shouldCacheTouchPosition(ITouch value)
        {
            // Points must be over target when released
            return GetTargetHitResult(value.Position);
        }

        #endregion

        #region private functions

        private IEnumerator wait()
        {
            yield return new WaitForSeconds(TimeLimit);

            if (State == GestureState.Possible) setState(GestureState.Failed);
        }

        #endregion

    }
}