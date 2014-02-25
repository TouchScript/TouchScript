using System;
using System.Collections.Generic;

namespace TouchScript.Utils
{
    internal sealed class TimedTouchSequence
    {
        private List<ITouchPoint> points = new List<ITouchPoint>();
        private List<float> timestamps = new List<float>();

        public TimedTouchSequence()
        {}

        public void Add(ITouchPoint touch, float time)
        {
            points.Add(touch);
            timestamps.Add(time);
        }

        public void Clear()
        {
            points.Clear();
            timestamps.Clear();
        }

        public IList<ITouchPoint> FindTouchPointsLaterThan(float time)
        {
            var list = new List<ITouchPoint>();
            for (var i = points.Count - 1; i >= 0; i--)
            {
                if (timestamps[i] > time) list.Add(points[i]);
                else break;
            }
            list.Reverse();
            return list;
        }

        public IList<ITouchPoint> FindTouchPointsLaterThan(float time, Predicate<ITouchPoint> predicate)
        {
            var list = new List<ITouchPoint>();
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