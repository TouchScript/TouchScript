/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Utils.Editor.Attributes;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Gesture which recognizes a point cluster which didn't move for specified time since it appeared.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Long Press Gesture")]
    public class LongPressGesture : Gesture
    {

        #region Constants

        public const string LONG_PRESSED_MESSAGE = "OnLongPressed";

        #endregion

        #region Public properties

        /// <summary>
        /// Maximum number of simultaneous touch points.
        /// </summary>
        public int MaxTouches
        {
            get { return maxTouches; }
            set { maxTouches = value; }
        }

        /// <summary>
        /// Total time in seconds required to hold touches still.
        /// </summary>
        public float TimeToPress
        {
            get { return timeToPress; }
            set { timeToPress = value; }
        }

        /// <summary>
        /// Maximum distance in cm touch points can move before gesture fails.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        [NullToggle(NullIntValue = int.MaxValue)]
        private int maxTouches = int.MaxValue;

        [SerializeField]
        private float timeToPress = 1;

        [SerializeField]
        [NullToggle(NullFloatValue = float.PositiveInfinity)]
        private float distanceLimit = float.PositiveInfinity;

        private Vector2 startPosition;

        #endregion

        #region Unity methods

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<ITouchPoint> touches)
        {
            base.touchesBegan(touches);

            if (activeTouches.Count > MaxTouches)
            {
                setState(GestureState.Failed);
                return;
            }

            if (activeTouches.Count == touches.Count)
            {
                startPosition = touches[0].Position;
                StartCoroutine("wait");
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<ITouchPoint> touches)
        {
            base.touchesMoved(touches);

            if (distanceLimit < float.PositiveInfinity && (ScreenPosition - startPosition).magnitude / touchManager.DotsPerCentimeter >= DistanceLimit)
            {
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<ITouchPoint> touches)
        {
            base.touchesEnded(touches);

            if (activeTouches.Count == 0)
            {
                StopCoroutine("wait");
                setState(GestureState.Failed);
            }
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (UseSendMessage) SendMessageTarget.SendMessage(LONG_PRESSED_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            StopCoroutine("wait");
        }

        #endregion

        #region Private functions

        private IEnumerator wait()
        {
            yield return new WaitForSeconds(TimeToPress);

            if (State == GestureState.Possible)
            {
                if (base.GetTargetHitResult())
                {
                    setState(GestureState.Recognized);
                } else
                {
                    setState(GestureState.Failed);
                }
            }
        }

        #endregion
    }
}