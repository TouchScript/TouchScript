/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Clusters;
using TouchScript.Gestures.Simple;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes cluster movement.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Pan Gesture")]
    public class PanGesture : SimplePanGesture
    {

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                return Cluster.Get2DCenterPosition(activeTouches);
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                return Cluster.GetPrevious2DCenterPosition(activeTouches);
            }
        }

    }
}