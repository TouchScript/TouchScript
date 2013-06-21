/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Clusters;
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
                return Cluster.Get2DCenterPosition(activeTouches);
            }
        }

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
