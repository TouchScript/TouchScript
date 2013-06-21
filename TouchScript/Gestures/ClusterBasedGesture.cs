/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Gestures
{
    public abstract class ClusterBasedGesture : Gesture
    {

        public override Vector2 ScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                return Clusters.Clusters.Get2DCenterPosition(activeTouches);
            }
        }

        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (activeTouches.Count == 0) return TouchPoint.InvalidPosition;
                return Clusters.Clusters.GetPrevious2DCenterPosition(activeTouches);
            }
        }

    }
}
