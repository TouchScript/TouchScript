using System;
using System.Collections.Generic;

namespace TouchScript.Utils
{
    internal class TimedTouchSequence
    {
        private List<TouchPoint> points = new List<TouchPoint>();
        private List<float> timestamps = new List<float>();

        public TimedTouchSequence()
        {}

        public void Add(TouchPoint touch, float time)
        {
            points.Add(touch);
            timestamps.Add(time);
        }

        public void Clear()
        {
            points.Clear();
            timestamps.Clear();
        }

        public IList<TouchPoint> FindTouchPointsLaterThan(float time)
        {
            var list = new List<TouchPoint>();
            for (var i = points.Count - 1; i >= 0; i--)
            {
                if (timestamps[i] > time) list.Add(points[i]);
                else break;
            }
            list.Reverse();
            return list;
        }

        public IList<TouchPoint> FindTouchPointsLaterThan(float time, Predicate<TouchPoint> predicate)
        {
            var list = new List<TouchPoint>();
            for (var i = points.Count - 1; i >= 0; i--)
            {
                if (timestamps[i] > time)
                {
                    if (predicate(points[i])) list.Add(points[i]);
                }
                else break;
            }
            list.Reverse();
            return list;
        }

    }
}